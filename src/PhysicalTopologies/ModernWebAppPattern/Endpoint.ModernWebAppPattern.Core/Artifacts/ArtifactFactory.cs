// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Factories;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Types;
using Endpoint.ModernWebAppPattern.Core.Extensions;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.ModernWebAppPattern.Core.Artifacts;

using Aggregate = Endpoint.DomainDrivenDesign.Core.Models.Aggregate;
using Microservice = Endpoint.ModernWebAppPattern.Core.Models.Microservice;

public class ArtifactFactory : IArtifactFactory
{
    private readonly ILogger<ArtifactFactory> _logger;
    private readonly IDataContextProvider _dataContextProvider;
    private readonly IFileSystem _fileSystem;
    private readonly IClassFactory _classFactory;

    public ArtifactFactory(ILogger<ArtifactFactory> logger, IDataContextProvider dataContextProvider, IFileSystem fileSystem, IClassFactory classFactory)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dataContextProvider);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(classFactory);

        _logger = logger;
        _dataContextProvider = dataContextProvider;
        _fileSystem = fileSystem;
        _classFactory = classFactory;
    }

    public async Task<SolutionModel> SolutionCreateAsync(string path, string name, string directory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SolutionCreateAsync");

        var model = new SolutionModel(name, directory);

        var context = await _dataContextProvider.GetAsync(path, cancellationToken);

        ProjectModel? messagingProject = null;

        if (context.Messages.Any())
        {
            messagingProject = await MessagingProjectCreateAsync(model.SrcDirectory, cancellationToken);

            model.Projects.Add(messagingProject);
        }

        var modelsProject = await ModelsProjectCreateAsync(model.SrcDirectory, cancellationToken);

        modelsProject.Packages.Add(new PackageModel("MediatR", "12.4.1"));

        model.Projects.Add(modelsProject);

        foreach (var microservice in context.Microservices)
        {
            var microserviceProject = await ApiProjectCreateAsync(microservice, model.SrcDirectory, cancellationToken);
            
            model.Projects.Add(microserviceProject);

            model.DependOns.Add(new DependsOnModel(microserviceProject, modelsProject));

            if(context.Messages.Any())
            {
                model.DependOns.Add(new DependsOnModel(microserviceProject, messagingProject));
            }
        }

        return model;

    }

    public async Task<ProjectModel> MessagingProjectCreateAsync(string directory, CancellationToken cancellationToken)
    {
        var context = await _dataContextProvider.GetAsync(cancellationToken: cancellationToken);

        var model = new ProjectModel($"{context.ProductName}.Messaging", directory);

        var servicesDirectory = _fileSystem.Path.Combine(model.Directory, "Services");

        var messagesDirectory = _fileSystem.Path.Combine(model.Directory, "Messages");

        model.DotNetProjectType = DotNet.Artifacts.Projects.Enums.DotNetProjectType.ClassLib;

        return model;
    }

    public async Task<ProjectModel> ModelsProjectCreateAsync(string directory, CancellationToken cancellationToken)
    {
        var context = await _dataContextProvider.GetAsync(cancellationToken: cancellationToken);

        var model = new ProjectModel($"{context.ProductName}.Models", directory);

        model.DotNetProjectType = DotNet.Artifacts.Projects.Enums.DotNetProjectType.ClassLib;

        foreach(var boundedContext in context.BoundedContexts)
        {
            var microservice =  context.Microservices.Single(x => x.BoundedContextName == boundedContext.Name);

            foreach(var aggregate in boundedContext.Aggregates)
            {
                model.Files.AddRange(await AggregateCreateAsync(context, aggregate, model.Directory, cancellationToken));
            }
        }

        return model;
    }

    public async Task<ProjectModel> ApiProjectCreateAsync(Microservice microservice, string directory, CancellationToken cancellationToken)
    {
        var context = await _dataContextProvider.GetAsync(cancellationToken: cancellationToken);

        var model = new ProjectModel(microservice.Name, directory);

        var controllersDirectory = _fileSystem.Path.Combine(model.Directory, "Controllers");

        var requestHandlersDirectory = _fileSystem.Path.Combine(model.Directory, "RequestHandlers");

        model.DotNetProjectType = DotNet.Artifacts.Projects.Enums.DotNetProjectType.WebApi;

        var boundedContext = context.BoundedContexts.Single(x => x.Name == microservice.BoundedContextName);

        model.Files.Add(await DbContextCreateAsync(context, microservice, boundedContext, model.Directory));

        model.Files.Add(await DbContextInterfaceCreateAsync(context, microservice, boundedContext, model.Directory));

        model.Packages.Add(new PackageModel("Microsoft.EntityFrameworkCore", "8.0.10"));

        foreach (var aggregate in boundedContext.Aggregates)
        {
            var controllerClassModel = await _classFactory.CreateControllerAsync($"{aggregate.Name.Pluralize()}Controller", controllersDirectory);

            model.Files.Add(new CodeFileModel<ClassModel>(controllerClassModel, controllerClassModel.Name, controllersDirectory, CSharp) { Namespace = $"{microservice.Name}.Controllers" });

            foreach(var command in aggregate.Commands)
            {
                var commandHandlerModel = new ClassModel($"{command.Name}Handler");

                model.Files.Add(new CodeFileModel<ClassModel>(commandHandlerModel, commandHandlerModel.Name, requestHandlersDirectory, CSharp) { Namespace = $"{microservice.Name}.RequestHandlers" });
            }

            foreach(var query in aggregate.Queries)
            {
                var queryHandlerModel = new ClassModel($"{query.Name}Handler");

                model.Files.Add(new CodeFileModel<ClassModel>(queryHandlerModel, queryHandlerModel.Name, requestHandlersDirectory, CSharp) { Namespace = $"{microservice.Name}.RequestHandlers" });
            }
        }

        return model;
    }

    public async Task<IEnumerable<FileModel>> AggregateCreateAsync(IDataContext context, Aggregate aggregate, string directory, CancellationToken cancellationToken)
    {
        var files = new List<FileModel>();

        var aggregateDirectory = _fileSystem.Path.Combine(directory, aggregate.Name);

        var aggregateClassModel = new ClassModel(aggregate.Name);

        var aggregateDtoModel = new ClassModel($"{aggregate.Name}Dto");

        foreach (var property in aggregate.Properties)
        {
            aggregateClassModel.Properties.Add(new(aggregateClassModel, AccessModifier.Public, property.Kind.ToType(), property.Name, PropertyAccessorModel.GetSet));

            aggregateDtoModel.Properties.Add(new(aggregateClassModel, AccessModifier.Public, property.Kind.ToType(), property.Name, PropertyAccessorModel.GetSet));
        }

        files.Add(new CodeFileModel<ClassModel>(aggregateClassModel, aggregateClassModel.Name, aggregateDirectory, CSharp) { Namespace = $"{context.ProductName}.Models.{aggregate.Name}" });

        files.Add(new CodeFileModel<ClassModel>(aggregateDtoModel, aggregateDtoModel.Name, aggregateDirectory, CSharp) { Namespace = $"{context.ProductName}.Models.{aggregate.Name}" });


        foreach(var command in aggregate.Commands)
        {
            var commandRequestClassModel = new ClassModel($"{command.Name}Request") {  Usings = [new ("MediatR")] };

            commandRequestClassModel.Implements.Add(new TypeModel($"IRequest<{command.Name}Response>"));

            files.Add(new CodeFileModel<ClassModel>(commandRequestClassModel, commandRequestClassModel.Name, aggregateDirectory, CSharp) { Namespace = $"{context.ProductName}.Models.{aggregate.Name}" });

            var commandResponseClassModel = new ClassModel($"{command.Name}Response");

            files.Add(new CodeFileModel<ClassModel>(commandResponseClassModel, commandResponseClassModel.Name, aggregateDirectory, CSharp) { Namespace = $"{context.ProductName}.Models.{aggregate.Name}" });
        }

        foreach (var query in aggregate.Queries)
        {
            var queryRequestClassModel = new ClassModel($"{query.Name}Request") { Usings = [new("MediatR")] }; ;

            queryRequestClassModel.Implements.Add(new TypeModel($"IRequest<{query.Name}Response>"));

            files.Add(new CodeFileModel<ClassModel>(queryRequestClassModel, queryRequestClassModel.Name, aggregateDirectory, CSharp) { Namespace = $"{context.ProductName}.Models.{aggregate.Name}" });

            var queryResponseClassModel = new ClassModel($"{query.Name}Response");

            files.Add(new CodeFileModel<ClassModel>(queryResponseClassModel, queryResponseClassModel.Name, aggregateDirectory, CSharp) { Namespace = $"{context.ProductName}.Models.{aggregate.Name}" });
        }

        return files;
    }

    public async Task<FileModel> DbContextCreateAsync(IDataContext context, Microservice microservice, BoundedContext boundedContext, string directory)
    {
        var model = new ClassModel("DataContext");

        model.Implements.Add(new("DbContext"));

        model.Implements.Add(new("IDataContext"));

        model.Usings.Add(new("Microsoft.EntityFrameworkCore"));

        foreach (var aggregate in boundedContext.Aggregates)
        {
            model.Usings.Add(new($"{context.ProductName}.Models.{aggregate.Name.Pluralize()}"));

            model.Properties.Add(new PropertyModel(model, AccessModifier.Public, TypeModel.DbSetOf(aggregate.Name), aggregate.Name.Pluralize(), PropertyAccessorModel.GetPrivateSet));
        }

        return new CodeFileModel<ClassModel>(model, model.Name, directory, CSharp) { Namespace = $"{microservice.Name}" };
    }

    public async Task<FileModel> DbContextInterfaceCreateAsync(IDataContext context, Microservice microservice, BoundedContext boundedContext, string directory)
    {
        var model = new InterfaceModel("IDataContext");

        model.Usings.Add(new("Microsoft.EntityFrameworkCore"));

        foreach (var aggregate in boundedContext.Aggregates)
        {
            model.Usings.Add(new($"{context.ProductName}.Models.{aggregate.Name.Pluralize()}"));

            model.Properties.Add(new PropertyModel(model, AccessModifier.Public, TypeModel.DbSetOf(aggregate.Name), aggregate.Name.Pluralize(), PropertyAccessorModel.GetPrivateSet));
        }

        return new CodeFileModel<InterfaceModel>(model, model.Name, directory, CSharp) { Namespace = $"{microservice.Name}" };
    }
}

