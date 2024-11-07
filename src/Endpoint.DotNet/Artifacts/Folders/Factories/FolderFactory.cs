// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Data;
using System.Text;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Files.Factories;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Factories;
using Endpoint.DotNet.Syntax.Documents;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.TypeScript;
using Endpoint.DotNet.Syntax.Units;
using Endpoint.DotNet.Syntax.Units.Factories;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.DotNet.Artifacts.Folders.Factories;

public class FolderFactory : IFolderFactory
{
    private readonly ILogger<FolderFactory> logger;
    private readonly IFileProvider fileProvider;
    private readonly IFileSystem fileSystem;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly IFileFactory fileFactory;
    private readonly IClassFactory classFactory;
    private readonly IDocumentFactory documentFactory;

    public FolderFactory(
        ILogger<FolderFactory> logger,
        IFileProvider fileProvider,
        IFileSystem fileSystem,
        INamingConventionConverter namingConventionConverter,
        IFileFactory fileFactory,
        IClassFactory classFactory,
        IDocumentFactory documentFactory)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(fileSystem));
        this.fileFactory = fileFactory ?? throw new ArgumentNullException(nameof(fileFactory));
        this.classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        this.documentFactory = documentFactory ?? throw new ArgumentNullException(nameof(documentFactory));
    }

    public async Task<FolderModel> CreateAggregateCommandsAsync(ClassModel aggregate, string directory)
    {
        logger.LogInformation("Generating Aggregate Commands. {name}", aggregate.Name);

        var model = new FolderModel("Commands", directory);

        foreach (var routeType in new RouteType[] { RouteType.Create, RouteType.Delete, RouteType.Update })
        {
            var command = await documentFactory.CreateCommandAsync(aggregate, routeType);

            model.Files.Add(new CodeFileModel<DocumentModel>(command, command.Name, model.Directory, CSharpFile));
        }

        return model;
    }

    public async Task<FolderModel> CreateAggregateQueriesAsync(ClassModel aggregate, string directory)
    {
        var model = new FolderModel("Queries", directory);

        foreach (var routeType in new RouteType[] { RouteType.GetById, RouteType.Get, RouteType.Page })
        {
            var query = await documentFactory.CreateQueryAsync(aggregate, routeType);

            model.Files.Add(new CodeFileModel<DocumentModel>(query, query.Name, model.Directory, CSharpFile));
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
        var aggregate = await classFactory.CreateEntityAsync(aggregateName, properties.ToKeyValuePairList());

        var model = new FolderModel($"{aggregateName}Aggregate", string.Empty);

        model.SubFolders.Add(await CreateAggregateCommandsAsync(aggregate, model.Directory));

        model.SubFolders.Add(await CreateAggregateQueriesAsync(aggregate, model.Directory));

        var aggregateDto = aggregate.CreateDto();

        var extensions = await classFactory.DtoExtensionsCreateAsync(aggregate);

        model.Files.Add(new CodeFileModel<ClassModel>(aggregate, aggregate.Usings, aggregate.Name, model.Directory, CSharpFile));

        model.Files.Add(new CodeFileModel<ClassModel>(aggregateDto, aggregateDto.Usings, aggregateDto.Name, model.Directory, CSharpFile));

        model.Files.Add(new CodeFileModel<ClassModel>(extensions, extensions.Usings, extensions.Name, model.Directory, CSharpFile));

        return model;
    }

    public async Task<FolderModel> CreateAggregateAsync(string aggregateName, string properties, string directory)
    {
        var aggregate = await classFactory.CreateEntityAsync(aggregateName, properties.ToKeyValuePairList());

        var model = new FolderModel($"{aggregateName}Aggregate", directory);

        model.SubFolders.Add(await CreateAggregateCommandsAsync(aggregate, model.Directory));

        model.SubFolders.Add(await CreateAggregateQueriesAsync(aggregate, model.Directory));

        var aggregateDto = aggregate.CreateDto();

        var extensions = await classFactory.DtoExtensionsCreateAsync(aggregate);

        model.Files.Add(new CodeFileModel<ClassModel>(aggregate, aggregate.Usings, aggregate.Name, model.Directory, CSharpFile));

        model.Files.Add(new CodeFileModel<ClassModel>(aggregateDto, aggregateDto.Usings, aggregateDto.Name, model.Directory, CSharpFile));

        model.Files.Add(new CodeFileModel<ClassModel>(extensions, extensions.Usings, extensions.Name, model.Directory, CSharpFile));

        return model;
    }
}
