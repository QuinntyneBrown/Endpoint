// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using Endpoint.Angular.Artifacts;
using Endpoint.Artifacts.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Angular.Cli.Commands;

[Verb("workspace-create")]
public class WorkspaceCreateRequest : IRequest
{
    [Option('n', "name")]
    public string? Name { get; set; }

    [Option('t', "project-type")]
    public string? ProjectType { get; set; }

    [Option('p', "project-name")]
    public string? ProjectName { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class WorkspaceCreateRequestHandler : IRequestHandler<WorkspaceCreateRequest>
{
    private readonly ILogger<WorkspaceCreateRequestHandler> _logger;
    private readonly IArtifactGenerator _artifactGenerator;

    public WorkspaceCreateRequestHandler(ILogger<WorkspaceCreateRequestHandler> logger, IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _logger = logger;
        _artifactGenerator = artifactGenerator;
    }

    public async Task Handle(WorkspaceCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(WorkspaceCreateRequestHandler));

        var model = new WorkspaceModel(request.Name, "18.2.7", request.Directory);

        await _artifactGenerator.GenerateAsync(model);
    }
}