using CommandLine;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Endpoint.Core.Models.WebArtifacts.Services;
using Endpoint.Core.Models.WebArtifacts;
using System.Linq;

namespace Endpoint.Cli.Commands;


[Verb("ng-localize-add")]
public class AngularLocalizeAddRequest : IRequest<Unit> {
    [Option('n',"name", Required = true)]
    public string Name { get; set; }

    [Option('l', "locales")]
    public string Locales { get; set; } = "fr-CA";


    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class AngularLocalizeAddRequestHandler : IRequestHandler<AngularLocalizeAddRequest, Unit>
{
    private readonly ILogger<AngularLocalizeAddRequestHandler> _logger;
    private readonly IAngularService _angularService;

    public AngularLocalizeAddRequestHandler(
        ILogger<AngularLocalizeAddRequestHandler> logger,
        IAngularService angularService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _angularService = angularService ?? throw new ArgumentNullException(nameof(angularService));
    }

    public async Task<Unit> Handle(AngularLocalizeAddRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(AngularLocalizeAddRequestHandler));

        _angularService.LocalizeAdd(new AngularProjectReferenceModel(request.Name, request.Directory), request.Locales.Split(',').ToList());

        return new();
    }
}