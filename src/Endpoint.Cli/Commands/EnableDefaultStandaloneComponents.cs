using CommandLine;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Endpoint.Core.Models.WebArtifacts.Services;

namespace Endpoint.Cli.Commands;

[Verb("enable-default-standalone-components")]
public class EnableDefaultStandaloneComponentsRequest : IRequest<Unit> {
    [Option('n',"name")]
    public string Name { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class EnableDefaultStandaloneComponentsRequestHandler : IRequestHandler<EnableDefaultStandaloneComponentsRequest, Unit>
{
    private readonly ILogger<EnableDefaultStandaloneComponentsRequestHandler> _logger;
    private readonly IAngularService _angularService;

    public EnableDefaultStandaloneComponentsRequestHandler(
        ILogger<EnableDefaultStandaloneComponentsRequestHandler> logger,
        IAngularService angularService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _angularService = angularService ?? throw new ArgumentNullException(nameof(angularService));
    }

    public async Task<Unit> Handle(EnableDefaultStandaloneComponentsRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(EnableDefaultStandaloneComponentsRequestHandler));

        _angularService.EnableDefaultStandaloneComponents(request.Name, request.Directory);

        return new();
    }
}