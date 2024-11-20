// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Testing.Core;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("crud-api-testing-create")]
public class CrudApiTestingCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('r', "resource-name")]
    public string ResourceName { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class CrudApiTestingCreateRequestHandler : IRequestHandler<CrudApiTestingCreateRequest>
{
    private readonly ILogger<CrudApiTestingCreateRequestHandler> _logger;
    private readonly IArtifactFactory _artifactFactory;
    private readonly IArtifactGenerator _artifactGenerator;

    public CrudApiTestingCreateRequestHandler(ILogger<CrudApiTestingCreateRequestHandler> logger, IArtifactFactory artifactFactory, IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(artifactFactory);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _logger = logger;
        _artifactFactory = artifactFactory;
        _artifactGenerator = artifactGenerator;
    }

    public async Task Handle(CrudApiTestingCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(CrudApiTestingCreateRequestHandler));

        var solutionModel = await _artifactFactory.SolutionCreateAsync(request.Name, request.ResourceName, request.Directory, cancellationToken);

        await _artifactGenerator.GenerateAsync(solutionModel);

        var workspaceModel = await _artifactFactory.AngularWorkspaceCreateAsync(request.Name, request.ResourceName, request.Directory, cancellationToken);

        await _artifactGenerator.GenerateAsync(workspaceModel);
    }
}