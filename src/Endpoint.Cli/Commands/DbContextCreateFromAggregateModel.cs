// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using Endpoint.Core.Abstractions;
using Endpoint.Core.Models.Artifacts.Files;
using Endpoint.Core.Models.Syntax.Classes;
using Endpoint.Core.Models.Syntax.Entities;
using Endpoint.Core.Models.Syntax.Interfaces;
using Endpoint.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Cli.Commands;


[Verb("db-context-create-from-aggregate-model")]
public class DbContextCreateFromAggregatesModelRequest : IRequest
{

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class DbContextCreateFromAggregatesModelRequestHandler : IRequestHandler<DbContextCreateFromAggregatesModelRequest>
{
    private readonly ILogger<DbContextCreateFromAggregatesModelRequestHandler> _logger;
    private readonly IFileProvider _fileProvider;
    private readonly INamingConventionConverter _namingConventionConverter;
    private readonly IArtifactGenerationStrategyFactory _artifactGenerationStrategyFactory;

    public DbContextCreateFromAggregatesModelRequestHandler(
        ILogger<DbContextCreateFromAggregatesModelRequestHandler> logger,
        IFileProvider fileProvider,
        INamingConventionConverter namingConventionConverter,
        IArtifactGenerationStrategyFactory artifactGenerationStrategyFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        _namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        _artifactGenerationStrategyFactory = artifactGenerationStrategyFactory ?? throw new ArgumentNullException(nameof(artifactGenerationStrategyFactory));
    }

    public async Task Handle(DbContextCreateFromAggregatesModelRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(DbContextCreateFromAggregatesModelRequestHandler));

        var entities = new List<EntityModel>();

        var projectDirectory = Path.GetDirectoryName(_fileProvider.Get("*.csproj", request.Directory));

        bool isInsideCoreProject = projectDirectory.EndsWith(".Core");

        var serviceName = Path.GetFileNameWithoutExtension(projectDirectory).Remove(isInsideCoreProject ? ".Core" : ".Infrastructure");

        var aggregatesModelDirectory = isInsideCoreProject ? $"{projectDirectory}{Path.DirectorySeparatorChar}AggregatesModel" : null;

        foreach (var folder in Directory.GetDirectories(aggregatesModelDirectory))
        {
            var folderName = Path.GetFileNameWithoutExtension(folder);

            if (folderName.EndsWith("Aggregate"))
                entities.Add(new EntityModel(folderName.Remove("Aggregate")));
        }

        var classModel = new DbContextModel(_namingConventionConverter, $"{serviceName}DbContext", entities, serviceName);

        var interfaceModel = classModel.ToInterface();

        Directory.CreateDirectory($"{projectDirectory.Replace("Core", "Infrastructure")}{Path.DirectorySeparatorChar}Data");

        _artifactGenerationStrategyFactory.CreateFor(
            new ObjectFileModel<InterfaceModel>(
                interfaceModel,
                interfaceModel.UsingDirectives,
                interfaceModel.Name,
                projectDirectory,
                "cs"));

        _artifactGenerationStrategyFactory.CreateFor(
            new ObjectFileModel<ClassModel>(
                classModel,
                classModel.UsingDirectives,
                classModel.Name,
                $"{projectDirectory.Replace("Core", "Infrastructure")}{Path.DirectorySeparatorChar}Data",
                "cs"));



    }
}
