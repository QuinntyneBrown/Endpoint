// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Text;
using Endpoint.Core.Artifacts.Files;
using Endpoint.Core.Artifacts.Files.Factories;
using Endpoint.Core.Services;
using Endpoint.Core.Syntax.Classes;
using Endpoint.Core.Syntax.Classes.Factories;
using Endpoint.Core.Syntax.Properties;
using Endpoint.Core.Syntax.TypeScript;
using Endpoint.Core.Syntax.Units;
using Endpoint.Core.Syntax.Units.Factories;
using Microsoft.Extensions.Logging;

namespace Endpoint.Core.Artifacts.Folders.Factories;

public class FolderFactory : IFolderFactory
{
    private readonly ILogger<FolderFactory> logger;
    private readonly IFileProvider fileProvider;
    private readonly IFileSystem fileSystem;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly IFileFactory fileFactory;
    private readonly IClassFactory classFactory;
    private readonly IUnitFactory unitFactory;

    public FolderFactory(
        ILogger<FolderFactory> logger,
        IFileProvider fileProvider,
        IFileSystem fileSystem,
        INamingConventionConverter namingConventionConverter,
        IFileFactory fileFactory,
        IClassFactory classFactory,
        IUnitFactory unitFactory)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(fileSystem));
        this.fileFactory = fileFactory ?? throw new ArgumentNullException(nameof(fileFactory));
        this.classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        this.unitFactory = unitFactory ?? throw new ArgumentNullException(nameof(unitFactory));
    }

    public async Task<FolderModel> CreateAggregateCommandsAsync(ClassModel aggregate)
    {
        logger.LogInformation("Generating Aggregate Commands. {name}", aggregate.Name);

        var model = new FolderModel("Commands", string.Empty);

        foreach (var routeType in new RouteType[] { RouteType.Create, RouteType.Delete, RouteType.Update })
        {
            var command = await unitFactory.CreateCommandAsync(aggregate, routeType);

            model.Files.Add(new CodeFileModel<SyntaxUnitModel>(command, command.Name, model.Directory, ".cs"));
        }

        return model;
    }

    public async Task<FolderModel> CreateAggregateQueriesAsync(ClassModel aggregate)
    {
        var model = new FolderModel("Queries", string.Empty);

        foreach (var routeType in new RouteType[] { RouteType.GetById, RouteType.Get, RouteType.Page })
        {
            var query = new QueryModel(); // (microserviceName, _namingConventionConverter, aggregate, routeType);

            model.Files.Add(new CodeFileModel<QueryModel>((QueryModel)query, (System.Collections.Generic.List<UsingModel>)query.UsingDirectives, (string)query.Name, model.Directory, ".cs"));
        }

        return model;
    }

    public async Task<FolderModel> CreateAngularDomainModelAsync(string modelName, string properties)
    {
        var modelNameSnakeCase = namingConventionConverter.Convert(NamingConvention.SnakeCase, modelName);

        var model = new FolderModel(modelNameSnakeCase, string.Empty);

        var typeScriptTypeModel = new TypeScriptTypeModel(modelNameSnakeCase);

        foreach (var property in properties.Split(','))
        {
            var propertyName = namingConventionConverter.Convert(NamingConvention.CamelCase, property.Split(':')[0]);

            var propertyType = property.Split(':')[1] switch
            {
                "guid" => "string",
                "int" => "number",
                _ => property.Split(':')[1]
            };

            typeScriptTypeModel.Properties.Add(PropertyModel.TypeScriptProperty(propertyName, propertyType));
        }

        model.Files.Add(new CodeFileModel<TypeScriptTypeModel>(typeScriptTypeModel, typeScriptTypeModel.Name, model.Directory, ".ts"));

        model.Files.Add(fileFactory.CreateTemplate("http-service", $"{modelNameSnakeCase}.service", model.Directory, ".ts", tokens: new TokensBuilder()
            .With("entityName", modelNameSnakeCase)
            .Build()));

        model.Files.Add(fileFactory.CreateTemplate("component-store", $"{modelNameSnakeCase}.store", model.Directory, ".ts", tokens: new TokensBuilder()
            .With("entityName", modelNameSnakeCase)
            .Build()));

        model.Files.Add(new ContentFileModel(
            new StringBuilder()
            .AppendLine($"export * from './{modelNameSnakeCase}';")
            .AppendLine($"export * from './{modelNameSnakeCase}.service';")
            .AppendLine($"export * from './{modelNameSnakeCase}.store';")
            .ToString(), "index", model.Directory, ".ts"));

        return model;
    }

    public async Task<FolderModel> CreateAggregateAsync(string aggregateName, string properties)
    {
        var aggregate = classFactory.CreateEntity(aggregateName, properties);

        var model = new FolderModel($"{aggregateName}Aggregate", string.Empty);

        model.SubFolders.Add(await CreateAggregateCommandsAsync(aggregate));

        model.SubFolders.Add(await CreateAggregateQueriesAsync(aggregate));

        var aggregateDto = aggregate.CreateDto();

        var extensions = await classFactory.DtoExtensionsCreateAsync(aggregate);

        model.Files.Add(new CodeFileModel<ClassModel>(aggregate, aggregate.Usings, aggregate.Name, model.Directory, ".cs"));

        model.Files.Add(new CodeFileModel<ClassModel>(aggregateDto, aggregateDto.Usings, aggregateDto.Name, model.Directory, ".cs"));

        model.Files.Add(new CodeFileModel<ClassModel>(extensions, extensions.Usings, extensions.Name, model.Directory, ".cs"));

        return model;
    }
}
