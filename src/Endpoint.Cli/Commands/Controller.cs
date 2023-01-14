using CommandLine;
using Endpoint.Core.Builders;
using Endpoint.Core.Models.Syntax;
using Endpoint.Core.Services;
using Endpoint.Core.ValueObjects;
using MediatR;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Cli.Commands;


[Verb("controller-create")]
public class ControllerCreateRequest : IRequest<Unit>
{
    [Value(0)]
    public string Entity { get; set; }

    [Option('d')]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class ControllerCreateRequestHandler : IRequestHandler<ControllerCreateRequest, Unit>
{
    private readonly ISettingsProvider _settingsProvder;
    private readonly IFileSystem _fileSystem;

    public ControllerCreateRequestHandler(ISettingsProvider settingsProvider, IFileSystem fileSystem)
    {
        _settingsProvder = settingsProvider;
        _fileSystem = fileSystem;
    }

    public Task<Unit> Handle(ControllerCreateRequest request, CancellationToken cancellationToken)
    {
        var settings = _settingsProvder.Get(request.Directory);

        new ClassBuilder($"{((Token)request.Entity).PascalCase}Controller", new Context(), _fileSystem)
            .WithDirectory($"{settings.ApiDirectory}{Path.DirectorySeparatorChar}Controllers")
            .WithUsing("System.Net")
            .WithUsing("System.Threading.Tasks")
            .WithUsing($"{settings.ApplicationNamespace}.Features")
            .WithUsing("MediatR")
            .WithUsing("Microsoft.AspNetCore.Mvc")
            .WithNamespace($"{settings.ApiNamespace}.Controllers")
            .WithAttribute(new GenericAttributeGenerationStrategy().WithName("ApiController").Build())
            .WithAttribute(new GenericAttributeGenerationStrategy().WithName("Route").WithParam("\"api/[controller]\"").Build())
            .WithDependency("IMediator", "mediator")
            .WithMethod(new MethodBuilder().WithEndpointType(RouteType.GetById).WithResource(request.Entity).WithAuthorize(false).Build())
            .WithMethod(new MethodBuilder().WithEndpointType(RouteType.Get).WithResource(request.Entity).WithAuthorize(false).Build())
            .WithMethod(new MethodBuilder().WithEndpointType(RouteType.Create).WithResource(request.Entity).WithAuthorize(false).Build())
            .WithMethod(new MethodBuilder().WithEndpointType(RouteType.Page).WithResource(request.Entity).WithAuthorize(false).Build())
            .WithMethod(new MethodBuilder().WithEndpointType(RouteType.Update).WithResource(request.Entity).WithAuthorize(false).Build())
            .WithMethod(new MethodBuilder().WithEndpointType(RouteType.Delete).WithResource(request.Entity).WithAuthorize(false).Build())
            .Build();

        return Task.FromResult(new Unit());
    }
}