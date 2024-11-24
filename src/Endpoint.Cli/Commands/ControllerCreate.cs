// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts.Projects.Services;
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

    [Option('d')]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class ControllerCreateRequestHandler : IRequestHandler<ControllerCreateRequest>
{
    private readonly ILogger<ControllerCreateRequestHandler> _logger;
    private readonly IApiProjectService _apiProjectService;

    public ControllerCreateRequestHandler(ILogger<ControllerCreateRequestHandler> logger, IApiProjectService apiProjectService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(apiProjectService);

        _logger = logger;
        _apiProjectService = apiProjectService;
    }

    public async Task Handle(ControllerCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(ControllerCreateRequestHandler));

        await _apiProjectService.ControllerCreateAsync(request.EntityName, request.Empty, request.Directory);
    }
}
