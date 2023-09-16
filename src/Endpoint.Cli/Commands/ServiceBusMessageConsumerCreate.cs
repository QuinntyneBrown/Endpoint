// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using Endpoint.Core.Artifacts.Units;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Cli.Commands;


[Verb("service-bus-message-consumer-create")]
public class ServiceBusMessageConsumerCreateRequest : IRequest
{
    [Option('n')]
    public string Name { get; set; } = "ServiceBusMessageConsumer";

    [Option('m')]
    public string MessagesNamespace { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = Environment.CurrentDirectory;
}

public class ServiceBusMessageConsumerCreateRequestHandler : IRequestHandler<ServiceBusMessageConsumerCreateRequest>
{
    private readonly ILogger<ServiceBusMessageConsumerCreateRequestHandler> _logger;
    private readonly IDomainDrivenDesignFileService _domainDrivenDesignFileService;
    public ServiceBusMessageConsumerCreateRequestHandler(
        ILogger<ServiceBusMessageConsumerCreateRequestHandler> logger,
        IDomainDrivenDesignFileService domainDrivenDesignFileService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _domainDrivenDesignFileService = domainDrivenDesignFileService ?? throw new ArgumentNullException(nameof(domainDrivenDesignFileService));
    }

    public async Task Handle(ServiceBusMessageConsumerCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(ServiceBusMessageConsumerCreateRequestHandler));

        _domainDrivenDesignFileService.ServiceBusMessageConsumerCreate(request.Name, request.MessagesNamespace, request.Directory);
    }
}
