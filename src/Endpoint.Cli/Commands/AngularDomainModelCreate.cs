// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Endpoint.Core.Models.Artifacts.Folders.Factories;
using Endpoint.Core.Abstractions;

namespace Endpoint.Cli.Commands;


[Verb("angular-domain-model-create")]
public class AngularDomainModelCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }


    [Option('p', "properties")]
    public string Properties { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class AngularDomainModelCreateRequestHandler : IRequestHandler<AngularDomainModelCreateRequest>
{
    private readonly ILogger<AngularDomainModelCreateRequestHandler> _logger;
    private readonly IFolderFactory _folderFactory;
    private readonly IArtifactGenerationStrategyFactory _artifactGenerationStrategyFactory;

    public AngularDomainModelCreateRequestHandler(
        ILogger<AngularDomainModelCreateRequestHandler> logger,
        IFolderFactory folderFactory,
        IArtifactGenerationStrategyFactory artifactGenerationStrategyFactory
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _artifactGenerationStrategyFactory = artifactGenerationStrategyFactory ?? throw new ArgumentNullException(nameof(artifactGenerationStrategyFactory));
        _folderFactory = folderFactory ?? throw new ArgumentNullException(nameof(folderFactory));
    }

    public async Task Handle(AngularDomainModelCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(AngularDomainModelCreateRequestHandler));

        if (string.IsNullOrEmpty(request.Properties))
        {
            request.Properties = $"{request.Name}Id:string";
        }

        var model = _folderFactory.AngularDomainModel(request.Name, request.Properties, request.Directory);

        _artifactGenerationStrategyFactory.CreateFor(model);
    }
}
