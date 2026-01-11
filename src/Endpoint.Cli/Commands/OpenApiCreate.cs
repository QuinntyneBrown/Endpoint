// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.OpenApi;
using Endpoint.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("open-api")]
public class OpenApiCreateRequest : IRequest
{
    [Option('d', Required = false)]
    public string Directory { get; set; } = Environment.CurrentDirectory;

    [Option('o', "output")]
    public string OutputPath { get; set; }
}

public class OpenApiCreateRequestHandler : IRequestHandler<OpenApiCreateRequest>
{
    private readonly ILogger<OpenApiCreateRequestHandler> _logger;
    private readonly IFileProvider _fileProvider;
    private readonly IArtifactGenerator _artifactGenerator;

    public OpenApiCreateRequestHandler(
        ILogger<OpenApiCreateRequestHandler> logger,
        IFileProvider fileProvider,
        IArtifactGenerator artifactGenerator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
    }

    public async Task Handle(OpenApiCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating OpenAPI specification for solution in directory: {directory}", request.Directory);

        var solutionPath = _fileProvider.Get("*.sln", request.Directory);

        if (string.IsNullOrEmpty(solutionPath))
        {
            _logger.LogError("No solution file found in directory: {directory}", request.Directory);
            return;
        }

        _logger.LogInformation("Found solution: {solutionPath}", solutionPath);

        var solutionDirectory = Path.GetDirectoryName(solutionPath);
        var solutionName = Path.GetFileNameWithoutExtension(solutionPath);
        var outputPath = request.OutputPath ?? Path.Combine(solutionDirectory, "openapi.json");

        var model = new OpenApiDocumentModel(solutionDirectory, solutionName, outputPath);

        await _artifactGenerator.GenerateAsync(model);
    }
}
