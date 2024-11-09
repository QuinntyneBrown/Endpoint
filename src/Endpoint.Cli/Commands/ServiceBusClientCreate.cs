// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects.Factories;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.DotNet.Artifacts.Solutions.Factories;
using Endpoint.DotNet.Artifacts.Solutions.Services;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Factories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;
[Verb("service-bus-client-create")]
public class ServiceBusClientCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('t', "project-type")]
    public string ProjectType { get; set; } = "web";

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class ServiceBusClientCreateRequestHandler : IRequestHandler<ServiceBusClientCreateRequest>
{
    private readonly ILogger<ServiceBusClientCreateRequestHandler> logger;
    private readonly ISolutionFactory solutionFactory;
    private readonly ISolutionService solutionService;
    private readonly ICommandService commandService;
    private readonly IProjectFactory projectFactory;
    private readonly IClassFactory classFactory;
    private readonly IProjectService projectService;

    public ServiceBusClientCreateRequestHandler(
        ILogger<ServiceBusClientCreateRequestHandler> logger,
        ISolutionService solutionService,
        ISolutionFactory solutionFactory,
        ICommandService commandService,
        IProjectFactory projectFactory,
        IClassFactory classFactory,
        IProjectService projectService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.solutionService = solutionService ?? throw new ArgumentNullException(nameof(solutionService));
        this.solutionFactory = solutionFactory ?? throw new ArgumentNullException(nameof(solutionFactory));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.projectFactory = projectFactory ?? throw new ArgumentNullException(nameof(projectFactory));
        this.classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        this.projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
    }

    public async Task Handle(ServiceBusClientCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handled: {0}", nameof(SolutionCreateRequestHandler));

        var model = await projectFactory.Create(request.ProjectType, request.Name, request.Directory);

        var serviceBusMessageConsumerClassModel = classFactory.CreateServiceBusMessageConsumer("ServiceBusMessageConsumer", model.Name);

        if (request.ProjectType == "worker")
        {
            var programFileModel = new ContentFileModel(
                new StringBuilder()
                .AppendLine("var host = Host.CreateDefaultBuilder(args)")
                .AppendLine(".ConfigureServices(services =>".Indent(1))
                .AppendLine("{".Indent(1))
                .AppendLine($"services.Add{model.Name.Split('.').Last()}Services();".Indent(2))
                .AppendLine("})".Indent(1))
                .AppendLine(".Build();".Indent(1))
                .AppendLine(string.Empty)
                .AppendLine("host.Run();")
                .ToString(), "Program", model.Directory, ".cs");

            model.Files.Add(programFileModel);
        }

        var configureServicesClassModel = classFactory.CreateConfigureServices(model.Name.Split('.').Last());

        var configureServicesMethodBodyBuilder = new StringBuilder();

        if (request.ProjectType == "web")
        {
            configureServicesMethodBodyBuilder
                .AppendLine("services.AddCors(options => options.AddPolicy(\"CorsPolicy\",")
                .AppendLine("builder => builder".Indent(1))
                .AppendLine(".WithOrigins(\"http://localhost:4200\")".Indent(1))
                .AppendLine(".AllowAnyMethod()".Indent(1))
                .AppendLine(".AllowAnyHeader()".Indent(1))
                .AppendLine(".SetIsOriginAllowed(isOriginAllowed: _ => true)".Indent(1))
                .AppendLine(".AllowCredentials()));".Indent(1))
                .AppendLine("services.AddControllers();")
                .AppendLine("services.AddEndpointsApiExplorer();")
                .AppendLine("services.AddSwaggerGen();");
        }

        configureServicesMethodBodyBuilder.AppendLine("services.AddMessagingUdpServices();")
            .AppendLine("services.AddHostedService<ServiceBusMessageConsumer>();");

        configureServicesClassModel.Methods.First().Body = new DotNet.Syntax.Expressions.ExpressionModel(configureServicesMethodBodyBuilder.ToString());

        var configureServicesFileModel = new CodeFileModel<ClassModel>(configureServicesClassModel, new ()
        {
            new (model.Name),
        }, configureServicesClassModel.Name, model.Directory, ".cs");

        configureServicesFileModel.Namespace = "Microsoft.Extensions.DependencyInjection";

        model.Files.Add(configureServicesFileModel);

        model.Files.Add(new CodeFileModel<ClassModel>(serviceBusMessageConsumerClassModel, serviceBusMessageConsumerClassModel.Usings, serviceBusMessageConsumerClassModel.Name, model.Directory, ".cs"));

        model.References.Add(@"..\Messaging.Udp\Messaging.Udp.csproj");

        projectService.AddProjectAsync(model);

        if (File.Exists(Path.Combine(model.Directory, "Worker.cs")))
        {
            File.Delete(Path.Combine(model.Directory, "Worker.cs"));
        }
    }
}
