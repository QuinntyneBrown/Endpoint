// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Engineering.RedisPubSub.Artifacts;
using Endpoint.Engineering.RedisPubSub.Models;
using Endpoint.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

/// <summary>
/// Request to add a messaging project to a solution.
/// </summary>
[Verb("messaging-add")]
public class MessagingAddRequest : IRequest
{
    /// <summary>
    /// Gets or sets the name of the solution. If not provided, it will be determined from the .sln file.
    /// </summary>
    [Option('n', "name", HelpText = "The name of the solution. If not provided, it will be determined from the .sln file.")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the directory to search for the solution file.
    /// </summary>
    [Option('d', "directory", Required = false, HelpText = "The directory to search for the solution file.")]
    public string Directory { get; set; } = Environment.CurrentDirectory;

    /// <summary>
    /// Gets or sets whether to use LZ4 compression with MessagePack.
    /// </summary>
    [Option("lz4", Required = false, Default = true, HelpText = "Whether to use LZ4 compression with MessagePack.")]
    public bool UseLz4Compression { get; set; } = true;
}

/// <summary>
/// Handler for adding a messaging project to a solution.
/// </summary>
public class MessagingAddRequestHandler : IRequestHandler<MessagingAddRequest>
{
    private readonly ILogger<MessagingAddRequestHandler> _logger;
    private readonly IFileProvider _fileProvider;
    private readonly IFileSystem _fileSystem;
    private readonly IArtifactFactory _artifactFactory;
    private readonly IArtifactGenerator _artifactGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessagingAddRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="fileProvider">The file provider.</param>
    /// <param name="fileSystem">The file system abstraction.</param>
    /// <param name="artifactFactory">The artifact factory.</param>
    /// <param name="artifactGenerator">The artifact generator.</param>
    public MessagingAddRequestHandler(
        ILogger<MessagingAddRequestHandler> logger,
        IFileProvider fileProvider,
        IFileSystem fileSystem,
        IArtifactFactory artifactFactory,
        IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileProvider);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(artifactFactory);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _logger = logger;
        _fileProvider = fileProvider;
        _fileSystem = fileSystem;
        _artifactFactory = artifactFactory;
        _artifactGenerator = artifactGenerator;
    }

    /// <inheritdoc/>
    public async Task Handle(MessagingAddRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding messaging project to solution...");

        // Find the solution file
        var solutionPath = _fileProvider.Get("*.sln", request.Directory);

        if (solutionPath == Endpoint.Constants.FileNotFound)
        {
            _logger.LogError("No solution file found in directory: {Directory}", request.Directory);
            throw new InvalidOperationException($"No solution file found in directory: {request.Directory}");
        }

        // Determine the solution name
        var solutionName = request.Name ?? _fileSystem.Path.GetFileNameWithoutExtension(solutionPath);
        var solutionDirectory = _fileSystem.Path.GetDirectoryName(solutionPath)!;

        // Determine the src directory (prefer src subdirectory if it exists)
        var srcDirectory = _fileSystem.Path.Combine(solutionDirectory, "src");
        if (!_fileSystem.Directory.Exists(srcDirectory))
        {
            srcDirectory = solutionDirectory;
        }

        _logger.LogInformation("Found solution: {SolutionName} at {SolutionPath}", solutionName, solutionPath);
        _logger.LogInformation("Creating messaging project in: {SrcDirectory}", srcDirectory);

        // Create the messaging model
        var messagingModel = new MessagingModel
        {
            SolutionName = solutionName,
            Directory = srcDirectory,
            UseLz4Compression = request.UseLz4Compression
        };

        // Create the messaging project model
        var projectModel = await _artifactFactory.CreateMessagingProjectAsync(messagingModel, cancellationToken);

        // Generate the messaging project
        await _artifactGenerator.GenerateAsync(projectModel, cancellationToken);

        _logger.LogInformation("Messaging project '{ProjectName}' added successfully!", projectModel.Name);
    }
}
