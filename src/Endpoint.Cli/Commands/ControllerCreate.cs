// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.ModernWebAppPattern.Core.Artifacts;
using Endpoint.ModernWebAppPattern.Core.Models;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("controller-create")]
public class ControllerCreateRequest : IRequest
{
    [Option('n')]
    public string EntityName { get; set; }

    [Option('e', "empty")]
    public bool Empty { get; set; }

    [Option('p', "product-name")]
    public string ProductName { get; set; }

    [Option('b', "bounded-context-name")]
    public string BoundedContextName { get; set; }

    [Option('d')]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class ControllerCreateRequestHandler : IRequestHandler<ControllerCreateRequest>
{
    private readonly ILogger<ControllerCreateRequestHandler> _logger;
    private readonly IArtifactFactory _artifactFactory;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly IApiProjectService _apiProjectService;

    public ControllerCreateRequestHandler(ILogger<ControllerCreateRequestHandler> logger, IApiProjectService apiProjectService, IArtifactGenerator artifactGenerator, IArtifactFactory artifactFactory)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(apiProjectService);
        ArgumentNullException.ThrowIfNull(artifactGenerator);
        ArgumentNullException.ThrowIfNull(artifactFactory);

        _logger = logger;
        _apiProjectService = apiProjectService;
        _artifactGenerator = artifactGenerator;
        _artifactFactory = artifactFactory;
    }

    public async Task Handle(ControllerCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(ControllerCreateRequestHandler));

        if (!string.IsNullOrEmpty(request.ProductName))
        {
            var (aggregate, dataContext) = Aggregate.Create(request.EntityName, request.ProductName);

            request.BoundedContextName ??= request.EntityName.Pluralize();

            var microservice = new Microservice($"{request.ProductName}.{request.BoundedContextName}.Api", request.BoundedContextName, MicroseviceKind.Api) 
            {
                ProductName = request.ProductName,
            };

            var controller = await _artifactFactory.ControllerCreateAsync(microservice, aggregate, request.Directory);

            await _artifactGenerator.GenerateAsync(controller);
        }
        else
        {
            await _apiProjectService.ControllerCreateAsync(request.EntityName, request.Empty, request.Directory);
        }
    }
}
