// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using Endpoint.Core.Models.Artifacts.Files;
using Endpoint.Core.Models.Artifacts.Projects.Factories;
using Endpoint.Core.Models.Artifacts.Solutions;
using Endpoint.Core.Models.Syntax.Classes;
using Endpoint.Core.Models.Syntax.Classes.Factories;
using Endpoint.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Cli.Commands;


[Verb("service-bus-solution-create")]
public class ServiceBusSolutionCreateRequest : IRequest {
    [Option('n',"name")]
    public string Name { get; set; }

    [Option('p')]
    public string ProjectName { get; set; }

    [Option('t')]
    public string ProjectType { get; set; } = "web";

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class ServiceBusSolutionCreateRequestHandler : IRequestHandler<ServiceBusSolutionCreateRequest>
{
    private readonly ILogger<ServiceBusSolutionCreateRequestHandler> _logger;
    private readonly ISolutionModelFactory _solutionModelFactory;
    private readonly ISolutionService _solutionService;
    private readonly ICommandService _commandService;
    private readonly IProjectModelFactory _projectModelFactory;
    private readonly IClassModelFactory _classModelFactory;
    public ServiceBusSolutionCreateRequestHandler(
        ILogger<ServiceBusSolutionCreateRequestHandler> logger,
        ISolutionService solutionService,
        ISolutionModelFactory solutionModelFactory,
        ICommandService commandService,
        IProjectModelFactory projectModelFactory,
        IClassModelFactory classModelFactory
        )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _solutionService = solutionService ?? throw new ArgumentNullException(nameof(solutionService));
        _solutionModelFactory = solutionModelFactory ?? throw new ArgumentNullException(nameof(solutionModelFactory));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _projectModelFactory = projectModelFactory ?? throw new ArgumentNullException(nameof(projectModelFactory));
        _classModelFactory = classModelFactory ?? throw new ArgumentNullException(nameof(classModelFactory));
    }

    public async Task Handle(ServiceBusSolutionCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(SolutionCreateRequestHandler));

        if(string.IsNullOrEmpty(request.ProjectName))
        {
            request.ProjectName = $"{request.Name}";
        }

        var model = _solutionModelFactory.Create(request.Name, request.ProjectName, request.ProjectType, string.Empty, request.Directory);
        
        var srcFolder = model.Folders.Single();

        var projectModel = srcFolder.Projects.Single();

        if(projectModel.Files.Any(x => x.Name == "ConfigureServices"))
        {
            projectModel.Files.Remove(projectModel.Files.Single(x => x.Name == "ConfigureServices"));
        }

        projectModel.Order = int.MaxValue;

        var serviceBusMessageConsumerClassModel = _classModelFactory.CreateServiceBusMessageConsumer("ServiceBusMessageConsumer", projectModel.Name);

        var configureServicesClassModel = _classModelFactory.CreateConfigureServices(model.Name.Split('.').Last());

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

        configureServicesMethodBodyBuilder
            .AppendLine("services.AddMessagingUdpServices();")
            .AppendLine("services.AddHostedService<ServiceBusMessageConsumer>();");

        configureServicesClassModel.Methods.First().Body = configureServicesMethodBodyBuilder.ToString();

        var configureServicesFileModel = new ObjectFileModel<ClassModel>(configureServicesClassModel, new()
        {
            new (model.Name)

        }, configureServicesClassModel.Name, projectModel.Directory, "cs");

        configureServicesFileModel.Namespace = "Microsoft.Extensions.DependencyInjection";

        projectModel.Files.Add(configureServicesFileModel);

        if (request.ProjectType == "worker")
        {
            var programFileModel = new ContentFileModel(new StringBuilder()
                .AppendLine("var host = Host.CreateDefaultBuilder(args)")
                .AppendLine(".ConfigureServices(services =>".Indent(1))
                .AppendLine("{".Indent(1))
                .AppendLine($"services.Add{projectModel.Name.Split('.').Last()}Services();".Indent(2))
                .AppendLine("})".Indent(1))
                .AppendLine(".Build();".Indent(1))
                .AppendLine(string.Empty)
                .AppendLine("host.Run();")
                .ToString(), "Program", projectModel.Directory, "cs");

            projectModel.Files.Add(programFileModel);
        }

        projectModel.Files.Add(new ObjectFileModel<ClassModel>(serviceBusMessageConsumerClassModel, serviceBusMessageConsumerClassModel.UsingDirectives, serviceBusMessageConsumerClassModel.Name, projectModel.Directory, "cs"));

        projectModel.References.Add(@"..\Messaging.Udp\Messaging.Udp.csproj");

        srcFolder.Projects.Add(_projectModelFactory.CreateMessagingProject(srcFolder.Directory));

        srcFolder.Projects.Add(_projectModelFactory.CreateMessagingUdpProject(srcFolder.Directory));

        _solutionService.Create(model);

        if (File.Exists(Path.Combine(projectModel.Directory, "Worker.cs")))
        {
            File.Delete(Path.Combine(projectModel.Directory, "Worker.cs"));
        }

        _commandService.Start($"start {model.SolultionFileName}", model.SolutionDirectory);

    }
}
