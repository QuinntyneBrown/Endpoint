// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Files.Factories;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Factories;
using Microsoft.CodeAnalysis;

namespace Endpoint.DotNet.Artifacts.Projects.Services;

// https://learn.microsoft.com/en-us/visualstudio/extensibility/internals/solution-dot-sln-file?view=vs-2022
public class ProjectService : IProjectService
{
    private readonly ICommandService commandService;
    private readonly IFileProvider fileProvider;
    private readonly IArtifactGenerator artifactGenerator;
    private readonly IFileSystem fileSystem;
    private readonly IFileFactory fileFactory;

    public ProjectService(
        IArtifactGenerator artifactGenerator,
        ICommandService commandService,
        IFileProvider fileProvider,
        IFileSystem fileSystem,
        IFileFactory fileFactory)
    {
        ArgumentNullException.ThrowIfNull(commandService);
        ArgumentNullException.ThrowIfNull(fileProvider);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(fileFactory);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        this.commandService = commandService;
        this.fileProvider = fileProvider;
        this.artifactGenerator = artifactGenerator;
        this.fileSystem = fileSystem;
        this.fileFactory = fileFactory;
    }

    public async Task AddProjectAsync(ProjectModel model)
    {
        await artifactGenerator.GenerateAsync(model);

        await AddToSolution(model);
    }

    public async Task AddToSolution(ProjectModel model)
    {
        var solution = fileProvider.Get("*.sln", model.Directory);
        var solutionName = Path.GetFileName(solution);
        var solutionDirectory = fileSystem.Path.GetDirectoryName(solution);

        if (model.Extension == ".csproj")
        {
            commandService.Start($"dotnet sln {solutionName} add {model.Path}", solutionDirectory);
        }
        else
        {
            var lines = new List<string>();

            var projectEntry = new string[2]
            {
                "Project(\"{" + $"{Guid.NewGuid()}".ToUpper() + "}\") = \"" + model.Name + "\", \"" + model.Path.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", string.Empty) + "\", \"{" + $"{Guid.NewGuid()}".ToUpper() + "}\"",
                "EndProject",
            };

            foreach (var line in fileSystem.File.ReadAllLines(solution))
            {
                lines.Add(line);

                if (line.StartsWith("MinimumVisualStudioVersion"))
                {
                    foreach (var entry in projectEntry)
                    {
                        lines.Add(entry);
                    }
                }
            }

            fileSystem.File.WriteAllLines(solution, lines.ToArray());
        }
    }

    public async Task AddEndpointPostBuildTargetElement(string csprojFilePath)
    {
        var doc = XDocument.Load(csprojFilePath);
        var projectNode = doc.FirstNode as XElement;
        projectNode.Add(CreateEndpointPostBuildTargetElement());
        doc.Save(csprojFilePath);
    }

    public async Task PackageAdd(string name, string directory)
    {
        var projectPath = fileProvider.Get("*.csproj", directory);

        var projectDirectory = fileSystem.Path.GetDirectoryName(projectPath);

        var projectFileContents = fileSystem.File.ReadAllText(projectPath);

        if (!projectFileContents.Contains($"PackageReference Include=\"{name}\""))
        {
            commandService.Start($"dotnet add package {name}", projectDirectory);
        }
    }

    public async Task CoreFilesAdd(string directory)
    {
        var projectPath = fileProvider.Get("*.csproj", directory);

        var projectDirectory = fileSystem.Path.GetDirectoryName(projectPath);

        foreach (var file in new List<FileModel>()
        {
            fileFactory.CreateResponseBase(projectDirectory),
            fileFactory.CreateCoreUsings(projectDirectory),
            fileFactory.CreateLinqExtensions(projectDirectory),
        })
        {
            if (!fileSystem.File.Exists(file.Path))
            {
                await artifactGenerator.GenerateAsync(file);
            }
        }
    }

    public async Task CorePackagesAdd(string directory)
    {
        PackageAdd("MediatR", directory);
        PackageAdd("FluentValidation", directory);
        PackageAdd("Microsoft.EntityFrameworkCore", directory);
        PackageAdd("Microsoft.Extensions.Logging.Abstractions", directory);
    }

    public async Task CorePackagesAndFiles(string directory)
    {
        CorePackagesAdd(directory);
        CoreFilesAdd(directory);
    }

    private XElement CreateEndpointPostBuildTargetElement()
    {
        var dotnetToolRestoreCommand = new XElement("Exec");

        dotnetToolRestoreCommand.SetAttributeValue("Command", "dotnet tool restore");

        var toFileCommand = new XElement("Exec");

        toFileCommand.SetAttributeValue("Command", "dotnet tool run swagger tofile --serializeasv2  --output \"$(ProjectDir)swagger.json\" \"$(TargetDir)$(TargetFileName)\" v1");

        var endpointCommand = new XElement("Exec");

        endpointCommand.SetAttributeValue("Command", "endpoint post-api-build");

        var element = new XElement("Target", dotnetToolRestoreCommand, toFileCommand, endpointCommand);

        element.SetAttributeValue("Name", "EndpointPostBuildTarget");

        element.SetAttributeValue("AfterTargets", "Build");

        return element;
    }

    public async Task UdpServiceBusProjectAddAsync(string name, string directory)
    {
        var model = new ProjectModel("classlib", name, directory);

        model.Packages.Add(new ("Microsoft.Extensions.Logging.Abstractions", "8.0.0"));

        model.Packages.Add(new ("Microsoft.Extensions.Hosting.Abstractions", "8.0.0"));

        // var udpClientFactoryInterface = await fileFactory.CreateUdpClientFactoryInterfaceAsync(directory);

        // var udpServiceBusConfigureServices = await classFactory.CreateUdpMessageSender();

        // var udpServiceBusHostExtensions = await classFactory.CreateUdpMessageSender();

        // var udpServiceBusMessage = await classFactory.CreateUdpMessageSender();

        // var messageSender = await classFactory.CreateUdpMessageSender();

        // var messageSenderInterface = await classFactory.CreateUdpMessageSenderInterface();

        // var messageReceiver = await classFactory.CreateUdpMessageReceiver();

        // var messageReceiverInterface = await classFactory.CreateUdpMessageReceiverInterface();

        // var udpClientFactoryInterface = await classFactory.CreateUdpClientFactoryInterface();

        // model.Files.Add(new CodeFileModel<ClassModel>() { });

        await AddProjectAsync(model);
    }
}
