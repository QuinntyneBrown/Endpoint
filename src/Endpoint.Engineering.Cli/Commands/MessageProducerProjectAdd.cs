// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Factories;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Factories;
using Endpoint.DotNet.Syntax.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("message-producer-project-add")]
public class MessageProducerProjectAddRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class MessageProducerProjectAddRequestHandler : IRequestHandler<MessageProducerProjectAddRequest>
{
    private readonly ILogger<MessageProducerProjectAddRequestHandler> _logger;
    private readonly IProjectService _projectService;
    private readonly IProjectFactory _projectFactory;
    private readonly IClassFactory _classFactory;
    private readonly IDependencyInjectionService _dependencyInjectionService;
    private readonly IFileSystem _fileSystem;

    public MessageProducerProjectAddRequestHandler(ILogger<MessageProducerProjectAddRequestHandler> logger, IProjectService projectService, IProjectFactory projectFactory, IClassFactory classFactory, IDependencyInjectionService dependencyInjectionService, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(projectService);
        ArgumentNullException.ThrowIfNull(projectFactory);
        ArgumentNullException.ThrowIfNull(classFactory);
        ArgumentNullException.ThrowIfNull(dependencyInjectionService);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _logger = logger;
        _projectService = projectService;
        _projectFactory = projectFactory;
        _classFactory = classFactory;
        _dependencyInjectionService = dependencyInjectionService;
        _fileSystem = fileSystem;
    }

    public async Task Handle(MessageProducerProjectAddRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(MessageProducerProjectAddRequestHandler));

        var model = await _projectFactory.Create("worker", request.Name, request.Directory);

        model.Packages.Add(new PackageModel("Microsoft.AspNetCore.SignalR", "*"));

        var program = $$"""
using {{model.Name}};

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<MessageProducer>();

var host = builder.Build();

host.Run();

""";

        var hubClassModel = _classFactory.CreateHubModel(request.Name);

        var interfaceModel = _classFactory.CreateHubInterfaceModel(request.Name);

        var messageModel = _classFactory.CreateMessageModel();

        var workerModel = await _classFactory.CreateMessageProducerWorkerAsync(request.Name, model.Directory);

        model.Files.Add(new CodeFileModel<ClassModel>(workerModel, workerModel.Name, model.Directory, CSharp));

        model.Files.Add(new CodeFileModel<ClassModel>(messageModel, messageModel.Name, model.Directory, CSharp));

        model.Files.Add(new CodeFileModel<ClassModel>(hubClassModel, hubClassModel.Name, model.Directory, CSharp));

        model.Files.Add(new CodeFileModel<InterfaceModel>(interfaceModel, interfaceModel.Name, model.Directory, CSharp));

        await _projectService.AddProjectAsync(model);

        await _dependencyInjectionService.AddConfigureServices(model.Name, model.Directory);

        var configureServicePath = Path.Combine(model.Directory, "ConfigureServices.cs");

        var configureServicesContent = new List<string>();

        foreach (var line in _fileSystem.File.ReadAllLines(configureServicePath))
        {
            configureServicesContent.Add(line);

            if (line.StartsWith("    {"))
            {
                configureServicesContent.Add("services.AddHostedService<MessageProducer>();".Indent(2));
            }
        }

        _fileSystem.File.WriteAllLines(configureServicePath, configureServicesContent.ToArray());

        _fileSystem.File.WriteAllText(Path.Combine(model.Directory, "Program.cs"), program);
    }
}