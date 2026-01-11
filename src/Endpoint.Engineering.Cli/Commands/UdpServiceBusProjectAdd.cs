// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.Projects.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("udp-service-bus-project-add")]
public class UdpServiceBusProjectAddRequest : IRequest
{
    [Option('n', "name", Required = false)]
    public string Name { get; set; } = "ServiceBus";

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class UdpServiceBusProjectAddRequestHandler : IRequestHandler<UdpServiceBusProjectAddRequest>
{
    private readonly ILogger<UdpServiceBusProjectAddRequestHandler> _logger;
    private readonly IProjectService _projectService;

    public UdpServiceBusProjectAddRequestHandler(ILogger<UdpServiceBusProjectAddRequestHandler> logger, IProjectService projectService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _projectService = projectService;
    }

    public async Task Handle(UdpServiceBusProjectAddRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(UdpServiceBusProjectAddRequestHandler));

        await _projectService.UdpServiceBusProjectAddAsync(request.Name, request.Directory);
    }
}