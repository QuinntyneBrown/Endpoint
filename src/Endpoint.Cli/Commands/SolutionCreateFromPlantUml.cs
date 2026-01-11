// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.PlantUml.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("solution-create-from-plant-uml")]
public class SolutionCreateFromPlantUmlRequest : IRequest
{
    [Option('n', "name", Required = true, HelpText = "The name of the solution to create.")]
    public string Name { get; set; }

    [Option('p', "plant-uml-source-path", Required = true, HelpText = "Path to the directory containing PlantUML files.")]
    public string PlantUmlSourcePath { get; set; }

    [Option('d', "directory", Required = false, HelpText = "Output directory for the generated solution.")]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class SolutionCreateFromPlantUmlRequestHandler : IRequestHandler<SolutionCreateFromPlantUmlRequest>
{
    private readonly ILogger<SolutionCreateFromPlantUmlRequestHandler> logger;
    private readonly IPlantUmlParserService plantUmlParserService;
    private readonly IPlantUmlSolutionModelFactory plantUmlSolutionModelFactory;
    private readonly IArtifactGenerator artifactGenerator;
    private readonly IFileSystem fileSystem;

    public SolutionCreateFromPlantUmlRequestHandler(
        ILogger<SolutionCreateFromPlantUmlRequestHandler> logger,
        IPlantUmlParserService plantUmlParserService,
        IPlantUmlSolutionModelFactory plantUmlSolutionModelFactory,
        IArtifactGenerator artifactGenerator,
        IFileSystem fileSystem)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.plantUmlParserService = plantUmlParserService ?? throw new ArgumentNullException(nameof(plantUmlParserService));
        this.plantUmlSolutionModelFactory = plantUmlSolutionModelFactory ?? throw new ArgumentNullException(nameof(plantUmlSolutionModelFactory));
        this.artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task Handle(SolutionCreateFromPlantUmlRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating solution from PlantUML: {Name}", request.Name);
        logger.LogInformation("PlantUML source path: {SourcePath}", request.PlantUmlSourcePath);
        logger.LogInformation("Output directory: {Directory}", request.Directory);

        // Validate inputs
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Solution name is required.", nameof(request.Name));
        }

        if (string.IsNullOrWhiteSpace(request.PlantUmlSourcePath))
        {
            throw new ArgumentException("PlantUML source path is required.", nameof(request.PlantUmlSourcePath));
        }

        // Resolve full path
        var sourcePath = fileSystem.Path.GetFullPath(request.PlantUmlSourcePath);
        var outputDirectory = fileSystem.Path.GetFullPath(request.Directory);

        if (!fileSystem.Directory.Exists(sourcePath))
        {
            throw new DirectoryNotFoundException($"PlantUML source directory not found: {sourcePath}");
        }

        // Parse PlantUML files from the source directory
        logger.LogInformation("Parsing PlantUML files from: {SourcePath}", sourcePath);
        var plantUmlModel = await plantUmlParserService.ParseDirectoryAsync(sourcePath, cancellationToken);

        if (plantUmlModel.Documents.Count == 0)
        {
            logger.LogWarning("No PlantUML documents found in: {SourcePath}. Attempting to create solution with default structure.", sourcePath);
        }
        else
        {
            logger.LogInformation("Parsed {Count} PlantUML documents", plantUmlModel.Documents.Count);
            logger.LogInformation("Found {EntityCount} entities, {EnumCount} enums",
                plantUmlModel.GetAllClasses().Count(),
                plantUmlModel.GetAllEnums().Count());
        }

        // Create solution model from parsed PlantUML
        logger.LogInformation("Building solution model...");
        var solutionModel = await plantUmlSolutionModelFactory.CreateAsync(
            plantUmlModel,
            request.Name,
            outputDirectory,
            cancellationToken);

        // Generate the solution using the artifact generator
        logger.LogInformation("Generating solution: {SolutionPath}", solutionModel.SolutionPath);
        await artifactGenerator.GenerateAsync(solutionModel);

        logger.LogInformation("Solution created successfully at: {SolutionDirectory}", solutionModel.SolutionDirectory);
    }
}
