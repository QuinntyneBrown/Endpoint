// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.Engineering.Api;
using Endpoint.Engineering.Api.Models;
using Endpoint.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

/// <summary>
/// Request to add an API Gateway project to a solution.
/// </summary>
[Verb("api-gateway-add")]
public class ApiGatewayAddRequest : IRequest
{
    /// <summary>
    /// Gets or sets the name of the solution. If not provided, it will be determined from the .sln file.
    /// </summary>
    [Option('n', "name", HelpText = "The name of the solution. If not provided, it will be determined from the .sln file.")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the directory to search for the solution file.
    /// </summary>
    [Option('d', Required = false, HelpText = "The directory to search for the solution file.")]
    public string Directory { get; set; } = Environment.CurrentDirectory;
}

/// <summary>
/// Handler for adding an API Gateway project to a solution.
/// </summary>
public class ApiGatewayAddRequestHandler : IRequestHandler<ApiGatewayAddRequest>
{
    private readonly ILogger<ApiGatewayAddRequestHandler> _logger;
    private readonly IFileProvider _fileProvider;
    private readonly IFileSystem _fileSystem;
    private readonly IApiArtifactFactory _artifactFactory;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly IProjectService _projectService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiGatewayAddRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileProvider">The file provider.</param>
    /// <param name="fileSystem">The file system abstraction.</param>
    /// <param name="artifactFactory">The artifact factory.</param>
    /// <param name="artifactGenerator">The artifact generator.</param>
    /// <param name="projectService">The project service.</param>
    public ApiGatewayAddRequestHandler(
        ILogger<ApiGatewayAddRequestHandler> logger,
        IFileProvider fileProvider,
        IFileSystem fileSystem,
        IApiArtifactFactory artifactFactory,
        IArtifactGenerator artifactGenerator,
        IProjectService projectService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileProvider);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(artifactFactory);
        ArgumentNullException.ThrowIfNull(artifactGenerator);
        ArgumentNullException.ThrowIfNull(projectService);

        _logger = logger;
        _fileProvider = fileProvider;
        _fileSystem = fileSystem;
        _artifactFactory = artifactFactory;
        _artifactGenerator = artifactGenerator;
        _projectService = projectService;
    }

    /// <inheritdoc/>
    public async Task Handle(ApiGatewayAddRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding API Gateway project to solution...");

        // Find the solution file
        var solutionPath = _fileProvider.Get("*.sln", request.Directory, 0);

        if (solutionPath == Endpoint.Constants.FileNotFound)
        {
            _logger.LogError("No solution file found in directory: {Directory}", request.Directory);
            throw new InvalidOperationException($"No solution file found in directory: {request.Directory}");
        }

        // Determine the solution name
        var solutionName = request.Name ?? _fileSystem.Path.GetFileNameWithoutExtension(solutionPath);
        var solutionDirectory = _fileSystem.Path.GetDirectoryName(solutionPath)!;

        // Determine the src directory (always use src; create if missing)
        var srcDirectory = _fileSystem.Path.Combine(solutionDirectory, "src");
        if (!_fileSystem.Directory.Exists(srcDirectory))
        {
            _fileSystem.Directory.CreateDirectory(srcDirectory);
        }

        _logger.LogInformation("Found solution: {SolutionName} at {SolutionPath}", solutionName, solutionPath);
        _logger.LogInformation("Creating API Gateway project in: {SrcDirectory}", srcDirectory);

        // Create the API Gateway model
        var apiGatewayInputModel = new ApiGatewayInputModel
        {
            SolutionName = solutionName,
            Directory = srcDirectory
        };

        // Create the API Gateway project model
        var projectModel = await _artifactFactory.CreateApiGatewayProjectAsync(apiGatewayInputModel, cancellationToken);

        // Generate the API Gateway project
        await _artifactGenerator.GenerateAsync(projectModel);

        // Add project to solution
        await _projectService.AddToSolution(projectModel);

        _logger.LogInformation("API Gateway project '{ProjectName}' added successfully!", projectModel.Name);
    }
}