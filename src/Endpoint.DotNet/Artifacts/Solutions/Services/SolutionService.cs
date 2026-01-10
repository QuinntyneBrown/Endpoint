// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Endpoint.Core.Artifacts.Abstractions;
using Endpoint.Core.Internal;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects.Enums;
using Endpoint.DotNet.Artifacts.Projects.Factories;
using Endpoint.DotNet.Artifacts.Units;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Services;
using MediatR;

namespace Endpoint.DotNet.Artifacts.Solutions.Services;

public class SolutionService : ISolutionService
{
    private readonly IArtifactGenerator artifactGenerator;
    private readonly IProjectFactory projectFactory;
    private readonly IDomainDrivenDesignFileService domainDrivenDesignFileService;
    private readonly IDomainDrivenDesignService domainDrivenDesignService;
    private readonly Observable<INotification> notificationListener;
    private readonly IFileProvider fileProvider;
    private readonly ICommandService commandService;

    public SolutionService(
        IArtifactGenerator artifactGenerator,
        IProjectFactory projectFactory,
        IDomainDrivenDesignFileService domainDrivenDesignFileService,
        IDomainDrivenDesignService domainDrivenDesignService,
        Observable<INotification> notificationListener,
        IFileProvider fileProvider,
        ICommandService commandService)
    {
        this.artifactGenerator = artifactGenerator;
        this.projectFactory = projectFactory;
        this.domainDrivenDesignFileService = domainDrivenDesignFileService;
        this.domainDrivenDesignService = domainDrivenDesignService;
        this.notificationListener = notificationListener;
        this.fileProvider = fileProvider;
        this.commandService = commandService;
    }

    public async Task AddSolutionItem(string path)
    {
        throw new NotImplementedException();
    }

    public async Task Create(SolutionModel model)
    {
        await artifactGenerator.GenerateAsync(model);
    }

    public async Task EventDrivenMicroservicesCreate(string name, string services, string directory)
    {
        var notifications = new List<INotification>();

        var solutionModel = new SolutionModel(name, directory);

        solutionModel.Folders.Add(await BuildingBlocksCreate(solutionModel.SolutionDirectory));

        solutionModel.Folders.Add(await ServicesCreate(services, solutionModel.SolutionDirectory, notifications));

        await artifactGenerator.GenerateAsync(solutionModel);

        foreach (var notification in notifications)
        {
            notificationListener.Broadcast(notification);
        }
    }

    public async Task Create(string name, string plantUmlSourcePath, string directory)
    {
    }

    public async Task MessagingBuildingBlockAdd(string directory)
    {
        var solutionPath = fileProvider.Get("*.sln", directory);

        var solutionName = Path.GetFileName(solutionPath);

        var solutionDirectory = Path.GetDirectoryName(solutionPath);

        var buildingBlocksDirectory = Path.Combine(solutionDirectory, "src", "BuildingBlocks");

        var messagingDirectory = Path.Combine(buildingBlocksDirectory, "Messaging");

        if (!Directory.Exists(buildingBlocksDirectory))
        {
            Directory.CreateDirectory(buildingBlocksDirectory);
        }

        if (!Directory.Exists(messagingDirectory))
        {
            Directory.CreateDirectory(messagingDirectory);
        }

        var messagingProjectModel = await projectFactory.CreateMessagingProject(messagingDirectory);

        await artifactGenerator.GenerateAsync(messagingProjectModel);

        commandService.Start($"dotnet sln {solutionName} add {messagingProjectModel.Path}", solutionDirectory);

        var messagingUdpProjectModel = await projectFactory.CreateMessagingUdpProject(messagingDirectory);

        await artifactGenerator.GenerateAsync(messagingUdpProjectModel);

        commandService.Start($"dotnet sln {solutionName} add {messagingUdpProjectModel.Path}", solutionDirectory);
    }

    public async Task IOCompressionBuildingBlockAdd(string directory)
    {
        var solutionPath = fileProvider.Get("*.sln", directory);

        var solutionName = Path.GetFileName(solutionPath);

        var solutionDirectory = Path.GetDirectoryName(solutionPath);

        var buildingBlocksDirectory = Path.Combine(solutionDirectory, "src", "BuildingBlocks");

        var messagingDirectory = Path.Combine(buildingBlocksDirectory, "IO.Compression");

        if (!Directory.Exists(buildingBlocksDirectory))
        {
            Directory.CreateDirectory(buildingBlocksDirectory);
        }

        if (!Directory.Exists(messagingDirectory))
        {
            Directory.CreateDirectory(messagingDirectory);
        }

        var messagingProjectModel = await projectFactory.CreateMessagingProject(messagingDirectory);

        await artifactGenerator.GenerateAsync(messagingProjectModel);

        commandService.Start($"dotnet sln {solutionName} add {messagingProjectModel.Path}", solutionDirectory);

        var messagingUdpProjectModel = await projectFactory.CreateMessagingUdpProject(messagingDirectory);

        await artifactGenerator.GenerateAsync(messagingUdpProjectModel);

        commandService.Start($"dotnet sln {solutionName} add {messagingUdpProjectModel.Path}", solutionDirectory);
    }

    private async Task<dynamic> BuildingBlocksCreate(string directory)
    {
        throw new NotImplementedException();
    }

    private async Task<dynamic> ServicesCreate(string services, string directory, List<INotification> notifications)
    {
        throw new NotImplementedException();
    }

    private dynamic AppsCreate(string directory)
    {
        throw new NotImplementedException();
    }
}
