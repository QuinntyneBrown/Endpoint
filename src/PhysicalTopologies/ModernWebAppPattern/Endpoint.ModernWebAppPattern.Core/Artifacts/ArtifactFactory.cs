// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Factories;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Types;
using Endpoint.ModernWebAppPattern.Core.Extensions;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.ModernWebAppPattern.Core.Artifacts;

using Aggregate = Endpoint.DomainDrivenDesign.Core.Models.Aggregate;
using Microservice = Endpoint.ModernWebAppPattern.Core.Models.Microservice;
using ISyntaxFactory = Endpoint.ModernWebAppPattern.Core.Syntax.ISyntaxFactory;
public class ArtifactFactory : IArtifactFactory
{
    private readonly ILogger<ArtifactFactory> _logger;
    private readonly IDataContextProvider _dataContextProvider;
    private readonly IFileSystem _fileSystem;
    private readonly IClassFactory _classFactory;
    private readonly ISyntaxFactory _syntaxFactory;

    public ArtifactFactory(ILogger<ArtifactFactory> logger, IDataContextProvider dataContextProvider, IFileSystem fileSystem, IClassFactory classFactory, ISyntaxFactory syntaxFactory)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dataContextProvider);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(classFactory);
        ArgumentNullException.ThrowIfNull(syntaxFactory);

        _logger = logger;
        _dataContextProvider = dataContextProvider;
        _fileSystem = fileSystem;
        _classFactory = classFactory;
        _syntaxFactory = syntaxFactory;
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

        var validationProject = await ValidationProjectCreateAsync(context.ProductName, model.SrcDirectory);

        model.Projects.Add(validationProject);

        var modelsProject = await ModelsProjectCreateAsync(model.SrcDirectory, cancellationToken);

        model.DependOns.Add(new DependsOnModel(modelsProject, validationProject));

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

        model.Packages.Add(new ("Microsoft.EntityFrameworkCore", "8.0.10"));

        foreach (var aggregate in boundedContext.Aggregates)
        {
            var aggregateNamespace = $"{context.ProductName}.Models.{aggregate.Name}";

            model.Files.Add(await ControllerCreateAsync(microservice, aggregate, controllersDirectory));
            
            foreach(var command in aggregate.Commands)
            {
                var commandHandlerModel = new ClassModel($"{command.Name}Handler");

                commandHandlerModel.Usings.Add(UsingModel.MediatR);

                commandHandlerModel.Usings.Add(new(aggregateNamespace));

                model.Files.Add(new CodeFileModel<ClassModel>(commandHandlerModel, commandHandlerModel.Name, requestHandlersDirectory, CSharp) { Namespace = $"{microservice.Name}.RequestHandlers" });
            }

            foreach(var query in aggregate.Queries)
            {
                var queryHandlerModel = new ClassModel($"{query.Name}Handler");

                queryHandlerModel.Usings.Add(UsingModel.MediatR);

                queryHandlerModel.Usings.Add(new(aggregateNamespace));

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

        var aggregateNamespace = $"{context.ProductName}.Models.{aggregate.Name}";

        foreach (var property in aggregate.Properties)
        {
            aggregateClassModel.Properties.Add(new(aggregateClassModel, AccessModifier.Public, property.Kind.ToType(), property.Name, PropertyAccessorModel.GetSet));

            aggregateDtoModel.Properties.Add(new(aggregateClassModel, AccessModifier.Public, property.Kind.ToType(), property.Name, PropertyAccessorModel.GetSet));
        }

        files.Add(new CodeFileModel<ClassModel>(aggregateClassModel, aggregateClassModel.Name, aggregateDirectory, CSharp) { Namespace = aggregateNamespace });

        files.Add(new CodeFileModel<ClassModel>(aggregateDtoModel, aggregateDtoModel.Name, aggregateDirectory, CSharp) { Namespace = aggregateNamespace });


        foreach(var command in aggregate.Commands)
        {
            files.Add(await CommandValidatorCreateAsync(command, aggregateDirectory));

            var commandRequestClassModel = new ClassModel($"{command.Name}Request") {  Usings = [new ("MediatR")] };

            commandRequestClassModel.Implements.Add(new ($"IRequest<{command.Name}Response>"));

            switch(command.Kind)
            {
                case RequestKind.Create:
                    foreach (var property in aggregate.Properties.Where(x => !x.Key))
                    {
                        commandRequestClassModel.Properties.Add(new(commandRequestClassModel, AccessModifier.Public, property.Kind.ToType(), property.Name, PropertyAccessorModel.GetSet));
                    }
                    break;

                case RequestKind.Update:
                    foreach(var property in aggregate.Properties)
                    {
                        commandRequestClassModel.Properties.Add(new(commandRequestClassModel, AccessModifier.Public, property.Kind.ToType(), property.Name, PropertyAccessorModel.GetSet));
                    }
                    
                    break;

                case RequestKind.Delete:

                    var keyProperty = aggregate.Properties.Single(x => x.Key);

                    commandRequestClassModel.Properties.Add(new (commandRequestClassModel, AccessModifier.Public, keyProperty.Kind.ToType(), keyProperty.Name, PropertyAccessorModel.GetSet));

                    break;
            }

            files.Add(new CodeFileModel<ClassModel>(commandRequestClassModel, commandRequestClassModel.Name, aggregateDirectory, CSharp) { Namespace = aggregateNamespace });

            var commandResponseClassModel = new ClassModel($"{command.Name}Response");

            switch (command.Kind)
            {
                case RequestKind.Create:    
                case RequestKind.Update:
                    commandResponseClassModel.Properties.Add(new(commandRequestClassModel, AccessModifier.Public, new($"{command.Aggregate.Name}Dto"), command.Aggregate.Name, PropertyAccessorModel.GetSet));
                    break;
            }

            files.Add(new CodeFileModel<ClassModel>(commandResponseClassModel, commandResponseClassModel.Name, aggregateDirectory, CSharp) { Namespace = aggregateNamespace });
        }

        foreach (var query in aggregate.Queries)
        {
            var queryRequestClassModel = new ClassModel($"{query.Name}Request") { Usings = [new("MediatR")] }; ;

            queryRequestClassModel.Implements.Add(new ($"IRequest<{query.Name}Response>"));

            switch(query.Kind)
            {
                case RequestKind.GetById:
                    var keyProperty = aggregate.Properties.Single(x => x.Key);

                    queryRequestClassModel.Properties.Add(new(queryRequestClassModel, AccessModifier.Public, keyProperty.Kind.ToType(), keyProperty.Name, PropertyAccessorModel.GetSet));
                    break;
            }

            files.Add(new CodeFileModel<ClassModel>(queryRequestClassModel, queryRequestClassModel.Name, aggregateDirectory, CSharp) { Namespace = aggregateNamespace });

            var queryResponseClassModel = new ClassModel($"{query.Name}Response");

            switch (query.Kind)
            {
                case RequestKind.GetById:
                    queryResponseClassModel.Properties.Add(new(queryRequestClassModel, AccessModifier.Public, new($"{aggregate.Name}Dto"), aggregate.Name, PropertyAccessorModel.GetSet));
                    break;

                case RequestKind.Get:
                    queryResponseClassModel.Properties.Add(new(queryRequestClassModel, AccessModifier.Public, TypeModel.ListOf($"{aggregate.Name}Dto"), aggregate.Name.Pluralize(), PropertyAccessorModel.GetSet));
                    break;
            }

            files.Add(new CodeFileModel<ClassModel>(queryResponseClassModel, queryResponseClassModel.Name, aggregateDirectory, CSharp) { Namespace = aggregateNamespace });
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
            var aggregateNamespace = $"{context.ProductName}.Models.{aggregate.Name}";

            model.Usings.Add(new(aggregateNamespace));

            model.Properties.Add(new (model, AccessModifier.Public, TypeModel.DbSetOf(aggregate.Name), aggregate.Name.Pluralize(), PropertyAccessorModel.GetPrivateSet));
        }

        return new CodeFileModel<ClassModel>(model, model.Name, directory, CSharp) { Namespace = $"{microservice.Name}" };
    }

    public async Task<FileModel> DbContextInterfaceCreateAsync(IDataContext context, Microservice microservice, BoundedContext boundedContext, string directory)
    {
        var model = new InterfaceModel("IDataContext");

        model.Usings.Add(new("Microsoft.EntityFrameworkCore"));

        foreach (var aggregate in boundedContext.Aggregates)
        {
            var aggregateNamespace = $"{context.ProductName}.Models.{aggregate.Name}";

            model.Usings.Add(new(aggregateNamespace));

            model.Properties.Add(new (model, AccessModifier.Public, TypeModel.DbSetOf(aggregate.Name), aggregate.Name.Pluralize(), PropertyAccessorModel.GetPrivateSet));
        }

        return new CodeFileModel<InterfaceModel>(model, model.Name, directory, CSharp) { Namespace = $"{microservice.Name}" };
    }

    public async Task<FileModel> ControllerCreateAsync(Microservice microservice, Aggregate aggregate, string directory)
    {
        var model = new ClassModel($"{aggregate.Name.Pluralize()}Controller");

        model.Implements.Add(new("Controller"));

        model.Usings.Add(new("MediatR"));

        model.Usings.Add(new("Microsoft.AspNetCore.Mvc"));

        model.Usings.Add(new("System.Net.Mime"));

        model.Usings.Add(new($"{microservice.ProductName}.Models.{aggregate.Name}"));

        model.Fields.Add(FieldModel.LoggerOf(model.Name));

        model.Fields.Add(FieldModel.Mediator);

        var constructor = new ConstructorModel(model, model.Name)
        {
            Params = [
                ParamModel.LoggerOf(model.Name),
                ParamModel.Mediator,
            ],
        };

        model.Constructors.Add(constructor);

        model.Attributes.Add(new (AttributeType.ApiController, "ApiController", []));

        model.Attributes.Add(new (AttributeType.Route, $"Route(\"api/{aggregate.Name.ToLower().Pluralize()}\")", []));

        foreach(var command in aggregate.Commands)
        {
            model.Methods.Add(command.Kind switch
            {

                RequestKind.Create => await _syntaxFactory.ControllerCreateMethodCreateAsync(model, command),
                RequestKind.Update => await _syntaxFactory.ControllerUpdateMethodCreateAsync(model, command),
                RequestKind.Delete => await _syntaxFactory.ControllerDeleteMethodCreateAsync(model, command)
            });
        }

        foreach(var query in aggregate.Queries)
        {
            model.Methods.Add(query.Kind switch
            {

                RequestKind.Get => await _syntaxFactory.ControllerGetMethodCreateAsync(model, query),
                RequestKind.GetById => await _syntaxFactory.ControllerGetByIdMethodCreateAsync(model, query),
            });
        }

        return new CodeFileModel<ClassModel>(model, model.Name, directory, CSharp) { Namespace = $"{microservice.Name}.Controllers" };
    }

    public async Task<FileModel> CommandValidatorCreateAsync(Command command, string directory)
    {
        var classModel = new ClassModel($"{command.Name}RequestValidator");

        classModel.Implements.Add(new($"AbstractValidator<{command.Name}Request>"));

        classModel.Usings.Add(new("FluentValidation"));

        classModel.Constructors.Add(new ConstructorModel(classModel, classModel.Name)
        {
            Body = new CommandRequestValidatorConstructorExpressionModel(command)
        });

        var file = new CodeFileModel<ClassModel>(classModel, classModel.Name, directory, CSharp) { Namespace = $"{command.ProductName}.Models.{command.Aggregate.Name}" };

        return file;
    }

    public async Task<ProjectModel> ValidationProjectCreateAsync(string productName, string directory)
    {
        var model = new ProjectModel($"{productName}.Validation", directory);

        model.Packages.Add(new("FluentValidation", "11.10.0"));

        model.Packages.Add(new PackageModel("MediatR", "12.4.1"));

        model.Packages.Add(new PackageModel("FluentValidation.DependencyInjectionExtensions", "11.10.0"));

        model.Files.Add(new FileModel("ConfigureServices", model.Directory, CSharp)
        {
            Body = $$"""
            using FluentValidation;
            using MediatR;
            using {{productName}}.Validation;

            namespace Microsoft.Extensions.DependencyInjection;

            public static class ConfigureServices
            {
                public static void AddValidation(this IServiceCollection services, Type type)
                {
                    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

                    services.AddValidatorsFromAssemblyContaining(type);
                }
            }
            """
        });

        model.Files.Add(new FileModel("ConfigureServices", model.Directory, CSharp)
        {
            Body = $$"""
            using FluentValidation;
            using MediatR;
            using {{productName}}.Validation;

            namespace Microsoft.Extensions.DependencyInjection;

            public static class ConfigureServices
            {
                public static void AddValidation(this IServiceCollection services, Type type)
                {
                    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

                    services.AddValidatorsFromAssemblyContaining(type);
                }
            }
            """
        });

        model.Files.Add(new FileModel("ValidationBehavior", model.Directory, CSharp)
        {
            Body = $$"""
            using FluentValidation;
            using MediatR;

            namespace {{productName}}.Validation;

            public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
                where TRequest : IRequest<TResponse>
                where TResponse : ResponseBase, new()
            {
                private readonly IEnumerable<IValidator<TRequest>> _validators;

                public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
                    => _validators = validators;

                public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
                {
                    var context = new ValidationContext<TRequest>(request);
                    var failures = _validators
                        .Select(v => v.Validate(context))
                        .SelectMany(result => result.Errors)
                        .Where(validationFailure => validationFailure != null)
                        .ToList();

                    if (failures.Any())
                    {
                        var response = new TResponse();

                        foreach (var failure in failures)
                        {
                            response.Errors.Add(failure.ErrorMessage);
                        }

                        return response;
                    }


                    return await next();
                }
            }
            """
        });

        model.Files.Add(new FileModel("ResponseBase", model.Directory, CSharp)
        {
            Body = $$"""
            namespace {{productName}}.Validation;

            public class ResponseBase
            {
                public ResponseBase()
                {
                    Errors = new List<string>();
                }
                public List<string> Errors { get; set; }
            }
            """
        });

        return model;
    }
}

