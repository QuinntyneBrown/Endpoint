// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Factories;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.DotNet.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("project-add")]
public class ProjectAddRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('t')]
    public string DotNetProjectType { get; set; } = "classlib";

    [Option('f')]
    public string FolderName { get; set; }

    [Option('r')]
    public string References { get; set; }

    [Option('m', "metadata")]
    public string Metadata { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = Environment.CurrentDirectory;
}

public class ProjectAddRequestHandler : IRequestHandler<ProjectAddRequest>
{
    private readonly ILogger<ProjectAddRequestHandler> logger;
    private readonly IArtifactGenerator artifactGenerator;
    private readonly ICommandService commandService;
    private readonly IFileSystem fileSystem;
    private readonly IProjectService projectService;
    private readonly IProjectFactory projectFactory;
    private readonly IFileProvider fileProvider;

    public ProjectAddRequestHandler(
        ILogger<ProjectAddRequestHandler> logger,
        IArtifactGenerator artifactGenerator,
        ICommandService commandService,
        IFileSystem fileSystem,
        IProjectService projectService,
        IProjectFactory projectFactory,
        IFileProvider fileProvider)
    {
        this.logger = logger;
        this.artifactGenerator = artifactGenerator;
        this.commandService = commandService;
        this.fileSystem = fileSystem;
        this.projectService = projectService;
        this.projectFactory = projectFactory;
        this.fileProvider = fileProvider;
    }

    public async Task Handle(ProjectAddRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding Project. {name}", request.Name);

        if (string.IsNullOrEmpty(request.Name))
        {
            ProjectAdd(request.Directory);
            return;
        }

        var projectPath = Path.Combine(request.Directory, request.Name);

        var projectDirectory = Path.GetDirectoryName(projectPath);

        if (string.IsNullOrEmpty(request.FolderName))
        {
            fileSystem.Directory.CreateDirectory(request.Directory);
        }

        var model = await projectFactory.Create(request.DotNetProjectType, request.Name, projectDirectory, request.References?.Split(',').ToList(), request.Metadata);

        await projectService.AddProjectAsync(model);
    }

    public void ProjectAdd(string directory)
    {
        var projectPath = fileProvider.Get("*.*sproj", directory);

        if (projectPath != Endpoint.Core.Constants.FileNotFound)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);

            var projectDirectory = Path.GetDirectoryName(projectPath);

            projectService.AddToSolution(new ProjectModel
            {
                Name = projectName,
                Directory = projectDirectory,
            });
        }

        foreach (var subDirectory in Directory.GetDirectories(directory))
        {
            var subDirectoryName = Path.GetFileName(subDirectory);

            var invalidDirectories = new[]
            {
                ".git",
                "node_modules",
                "bin",
                "obj",
                "Properties",
                "nupkg",
                "dist",
                ".angular",
                ".vs",
                ".vscode",
            };

            if (!invalidDirectories.Contains(subDirectoryName))
            {
                ProjectAdd(subDirectory);
            }
        }
    }
}
