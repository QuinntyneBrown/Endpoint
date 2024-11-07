// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts.Units;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("service-create")]
public class ServiceCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class ServiceCreateRequestHandler : IRequestHandler<ServiceCreateRequest>
{
    private readonly ILogger<ServiceCreateRequestHandler> logger;
    private readonly IDomainDrivenDesignFileService domainDrivenDesignFileService;

    public ServiceCreateRequestHandler(
        ILogger<ServiceCreateRequestHandler> logger,
        IDomainDrivenDesignFileService domainDrivenDesignFileService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.domainDrivenDesignFileService = domainDrivenDesignFileService ?? throw new ArgumentNullException(nameof(domainDrivenDesignFileService));
    }

    public async Task Handle(ServiceCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating Service. {name}", request.Name);

        await domainDrivenDesignFileService.ServiceCreateAsync(request.Name, request.Directory);
    }
}
