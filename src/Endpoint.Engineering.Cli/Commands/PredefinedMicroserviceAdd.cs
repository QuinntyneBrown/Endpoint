// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.DotNet.Services;
using Endpoint.Engineering.Microservices;
using Endpoint.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("predefined-microservice-add")]
public class PredefinedMicroserviceAddRequest : IRequest
{
    [Option('n', "name", Required = true, HelpText = "Name of the predefined microservice to add (e.g., Identity, Tenant, Notification, etc.)")]
    public string Name { get; set; }

    [Option('d', "directory", Required = false, HelpText = "Directory where the microservice will be created")]
    public string Directory { get; set; } = Environment.CurrentDirectory;

    [Option('l', "list", Required = false, Default = false, HelpText = "List all available predefined microservices")]
    public bool ListAvailable { get; set; }
}

public class PredefinedMicroserviceAddRequestHandler : IRequestHandler<PredefinedMicroserviceAddRequest>
{
    private readonly ILogger<PredefinedMicroserviceAddRequestHandler> logger;
    private readonly IMicroserviceFactory microserviceFactory;
    private readonly IProjectService projectService;
    private readonly IFileSystem fileSystem;
    private readonly IFileProvider fileProvider;
    private readonly IArtifactGenerator artifactGenerator;

    public PredefinedMicroserviceAddRequestHandler(
        ILogger<PredefinedMicroserviceAddRequestHandler> logger,
        IMicroserviceFactory microserviceFactory,
        IProjectService projectService,
        IFileSystem fileSystem,
        IFileProvider fileProvider,
        IArtifactGenerator artifactGenerator)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.microserviceFactory = microserviceFactory ?? throw new ArgumentNullException(nameof(microserviceFactory));
        this.projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        this.artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
    }

    public async Task Handle(PredefinedMicroserviceAddRequest request, CancellationToken cancellationToken)
    {
        if (request.ListAvailable)
        {
            logger.LogInformation("Available predefined microservices:");
            foreach (var microservice in microserviceFactory.GetAvailableMicroservices())
            {
                logger.LogInformation("  - {Microservice}", microservice);
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            logger.LogError("Microservice name is required. Use --list to see available microservices.");
            return;
        }

        logger.LogInformation("Adding predefined microservice: {Name}", request.Name);

        try
        {
            var solutionCreationRoot = ResolveSrcRootDirectory(request.Directory);
            var solutionModel = await microserviceFactory.CreateByNameAsync(request.Name, solutionCreationRoot, cancellationToken);

            logger.LogInformation("Creating microservice directory structure");

            fileSystem.Directory.CreateDirectory(solutionModel.SolutionDirectory);
            fileSystem.Directory.CreateDirectory(solutionModel.SrcDirectory);

            await artifactGenerator.GenerateAsync(solutionModel);

            foreach (var project in solutionModel.Projects)
            {
                logger.LogInformation("Adding project {ProjectName} to solution", project.Name);
                await projectService.AddProjectAsync(project);
            }

            logger.LogInformation("Successfully added {Name} microservice with projects:", request.Name);
            foreach (var project in solutionModel.Projects)
            {
                logger.LogInformation("  - {ProjectName}", project.Name);
            }
        }
        catch (ArgumentException ex)
        {
            logger.LogError("{Message}", ex.Message);
            logger.LogInformation("Use --list to see available predefined microservices.");
        }
    }

    private string ResolveSrcRootDirectory(string directory)
    {
        // Prefer: <solution-directory>/src (create if needed)
        // Fallback: <directory>/src
        var solutionPath = fileProvider.Get("*.sln", directory, 0);

        var baseDirectory = solutionPath == Endpoint.Constants.FileNotFound
            ? directory
            : fileSystem.Path.GetDirectoryName(solutionPath)!;

        var srcDirectory = fileSystem.Path.Combine(baseDirectory, "src");

        if (!fileSystem.Directory.Exists(srcDirectory))
        {
            fileSystem.Directory.CreateDirectory(srcDirectory);
        }

        logger.LogInformation("Creating microservice under: {SrcDirectory}", srcDirectory);

        return srcDirectory;
    }
}
