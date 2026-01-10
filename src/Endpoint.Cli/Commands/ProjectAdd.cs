// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts.Projects.Factories;
using Endpoint.DotNet.Artifacts.Projects.Services;
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
    private readonly ILogger<ProjectAddRequestHandler> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IProjectService _projectService;
    private readonly IProjectFactory _projectFactory;
    private readonly IFileProvider _fileProvider;

    public ProjectAddRequestHandler(
        ILogger<ProjectAddRequestHandler> logger,
        IFileSystem fileSystem,
        IProjectService projectService,
        IProjectFactory projectFactory,
        IFileProvider fileProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(projectService);
        ArgumentNullException.ThrowIfNull(projectFactory);
        ArgumentNullException.ThrowIfNull(fileProvider);

        _logger = logger;
        _fileSystem = fileSystem;
        _projectService = projectService;
        _projectFactory = projectFactory;
        _fileProvider = fileProvider;
    }

    public async Task Handle(ProjectAddRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding Project. {name}", request.Name);

        if (string.IsNullOrEmpty(request.Name))
        {
            ProjectAdd(request.Directory);

            return;
        }

        var projectPath = _fileSystem.Path.Combine(request.Directory, request.Name);

        var projectDirectory = _fileSystem.Path.GetDirectoryName(projectPath);

        if (string.IsNullOrEmpty(request.FolderName))
        {
            _fileSystem.Directory.CreateDirectory(request.Directory);
        }

        var model = await _projectFactory.Create(request.DotNetProjectType, request.Name, projectDirectory, request.References?.Split(',').ToList(), request.Metadata);

        await _projectService.AddProjectAsync(model);
    }

    public void ProjectAdd(string directory)
    {
        var projectPath = _fileProvider.Get("*.*sproj", directory);

        if (projectPath != Endpoint.Core.Constants.FileNotFound)
        {
            var projectName = _fileSystem.Path.GetFileNameWithoutExtension(projectPath);

            var projectDirectory = _fileSystem.Path.GetDirectoryName(projectPath);

            _projectService.AddToSolution(new()
            {
                Name = projectName,
                Directory = projectDirectory,
            });
        }

        foreach (var subDirectory in Directory.GetDirectories(directory))
        {
            var subDirectoryName = _fileSystem.Path.GetFileName(subDirectory);

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
