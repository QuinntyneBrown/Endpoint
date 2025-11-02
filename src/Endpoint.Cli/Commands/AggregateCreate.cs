// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Core.Artifacts.Abstractions;
using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.ModernWebAppPattern.Core;
using Endpoint.ModernWebAppPattern.Core.Artifacts;
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
    private readonly IUserInputService _userInputService;

    public AggregateCreateRequestHandler(ILogger<AggregateCreateRequestHandler> logger, IArtifactFactory artifactFactory, IArtifactGenerator artifactGenerator, IUserInputService userInputService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(artifactFactory);
        ArgumentNullException.ThrowIfNull(artifactGenerator);
        ArgumentNullException.ThrowIfNull(userInputService);

        _logger = logger;
        _artifactFactory = artifactFactory;
        _artifactGenerator = artifactGenerator;
        _userInputService = userInputService;
    }

    public async Task Handle(AggregateCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(AggregateCreateRequestHandler));

        var (aggregate, dataContext) = AggregateModel.Create(request.Name, request.ProductName);

        var options = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

        var defaultTemplate = JsonSerializer.Serialize(dataContext, options);

        var jsonElement = await _userInputService.ReadJsonAsync(defaultTemplate);

        dataContext = JsonSerializer.Deserialize<DataContext>(jsonElement, options);

        aggregate = dataContext.BoundedContexts[0].Aggregates.Single(x => x.Name == aggregate.Name);

        aggregate.BoundedContext = dataContext.BoundedContexts[0];

        foreach (var command in aggregate.Commands)
        {
            command.Aggregate = aggregate;
        }

        foreach (var query in aggregate.Queries)
        {
            query.Aggregate = aggregate;
        }

        foreach (var entity in aggregate.Entities)
        {
            entity.BoundedContext = aggregate.BoundedContext;
        }

        foreach (var model in await _artifactFactory.AggregateCreateAsync(dataContext, aggregate, request.Directory, cancellationToken))
        {
            await _artifactGenerator.GenerateAsync(model);
        }
    }
}