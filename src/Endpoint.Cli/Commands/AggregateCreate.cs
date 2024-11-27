// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.ModernWebAppPattern.Core;
using Endpoint.ModernWebAppPattern.Core.Artifacts;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("aggregate-create")]
public class AggregateCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('p', "product-name")]
    public string ProductName { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class AggregateCreateRequestHandler : IRequestHandler<AggregateCreateRequest>
{
    private readonly ILogger<AggregateCreateRequestHandler> _logger;
    private readonly IArtifactFactory _artifactFactory;
    private readonly IArtifactGenerator _artifactGenerator;

    public AggregateCreateRequestHandler(ILogger<AggregateCreateRequestHandler> logger, IArtifactFactory artifactFactory, IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(artifactFactory);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _logger = logger;
        _artifactFactory = artifactFactory;
        _artifactGenerator = artifactGenerator;
    }

    public async Task Handle(AggregateCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(AggregateCreateRequestHandler));

        var (aggregate, dataContext) = AggregateModel.Create(request.Name, request.ProductName);

        foreach (var model in await _artifactFactory.AggregateCreateAsync(dataContext, aggregate, request.Directory, cancellationToken))
        {
            await _artifactGenerator.GenerateAsync(model);
        }
    }
}