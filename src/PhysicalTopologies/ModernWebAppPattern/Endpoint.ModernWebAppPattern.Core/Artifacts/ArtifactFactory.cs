// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Artifacts;
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
using Endpoint.ModernWebAppPattern.Core.Extensions;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.RequestHandlers;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Text.Json;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.ModernWebAppPattern.Core.Artifacts;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;
using AggregateModel = Endpoint.DomainDrivenDesign.Core.Models.AggregateModel;
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
    public async Task<SolutionModel> SolutionCreateAsync(JsonElement jsonElement, string name, string directory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SolutionCreateAsync");

        var model = new SolutionModel(name, directory);

        var context = await _dataContextProvider.GetAsync(jsonElement, cancellationToken);

        ProjectModel? messagingProject = null;

        if (context.Messages.Count != 0)
        {
            messagingProject = await MessagingProjectCreateAsync(model.SrcDirectory, cancellationToken);

            model.Projects.Add(messagingProject);
        }

        var validationProject = await ValidationProjectCreateAsync(context.ProductName, model.SrcDirectory);

        model.Projects.Add(validationProject);

        var modelsProject = await ModelsProjectCreateAsync(model.SrcDirectory, cancellationToken);

        model.DependOns.Add(new(modelsProject, validationProject));

        model.Projects.Add(modelsProject);

        foreach (var microservice in context.Microservices)
        {
            var microserviceProject = await ApiProjectCreateAsync(microservice, model.SrcDirectory, cancellationToken);

            model.Projects.Add(microserviceProject);

            model.DependOns.Add(new(microserviceProject, modelsProject));

            if (context.Messages.Count != 0)
            {
                model.DependOns.Add(new(microserviceProject, messagingProject));
            }
        }

        return model;

    }

    public async Task<SolutionModel> SolutionCreateAsync(string path, string name, string directory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SolutionCreateAsync");

        var model = new SolutionModel(name, directory);

        var context = await _dataContextProvider.GetAsync(path, cancellationToken);

        ProjectModel? messagingProject = null;

        if (context.Messages.Count != 0)
        {
            messagingProject = await MessagingProjectCreateAsync(model.SrcDirectory, cancellationToken);

            model.Projects.Add(messagingProject);
        }

        var validationProject = await ValidationProjectCreateAsync(context.ProductName, model.SrcDirectory);

        model.Projects.Add(validationProject);

        var modelsProject = await ModelsProjectCreateAsync(model.SrcDirectory, cancellationToken);

        model.DependOns.Add(new (modelsProject, validationProject));

        model.Projects.Add(modelsProject);

        foreach (var microservice in context.Microservices)
        {
            var microserviceProject = await ApiProjectCreateAsync(microservice, model.SrcDirectory, cancellationToken);
            
            model.Projects.Add(microserviceProject);

            model.DependOns.Add(new (microserviceProject, modelsProject));

            if(context.Messages.Count != 0)
            {
                model.DependOns.Add(new (microserviceProject, messagingProject));
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

        var model = new ProjectModel($"{context.ProductName}.Models", directory)
        {
            DotNetProjectType = DotNet.Artifacts.Projects.Enums.DotNetProjectType.ClassLib
        };

        foreach (var boundedContext in context.BoundedContexts)
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

        model.Files.Add(await ApiProgramCreateAsync(boundedContext, microservice, model.Directory));

        model.Files.Add(await ApiAppSettingsCreateAsync(boundedContext, model.Directory));

        model.Files.Add(await ApiConfigureServicesCreateAsync(microservice, model.Directory));

        model.Packages.Add(new("Microsoft.EntityFrameworkCore", "8.0.10"));

        model.Packages.Add(new("Microsoft.EntityFrameworkCore.Design", "8.0.10"));

        model.Packages.Add(new("Microsoft.EntityFrameworkCore.SqlServer", "8.0.10"));

        foreach (var aggregate in boundedContext.Aggregates)
        {
            var aggregateNamespace = $"{context.ProductName}.Models.{aggregate.Name}";

            model.Files.Add(await ControllerCreateAsync(microservice, aggregate, controllersDirectory));
            
            foreach(var command in aggregate.Commands)
            {
                var commandHandlerModel = new ClassModel($"{command.Name}Handler");

                commandHandlerModel.Implements.Add(new($"IRequestHandler<{command.Name}Request, {command.Name}Response>"));
                
                commandHandlerModel.Usings.Add(new ("MediatR"));

                commandHandlerModel.Usings.Add(new(aggregateNamespace));

                commandHandlerModel.Fields =
                    [
                        FieldModel.LoggerOf($"{command.Name}Handler"),
                        new () { Name = "_context", Type = new ($"I{boundedContext.Name}DbContext") }
                    ];

                commandHandlerModel.Constructors.Add(new (commandHandlerModel, commandHandlerModel.Name)
                {
                    Params =
                    [
                        ParamModel.LoggerOf($"{command.Name}Handler"),
                        new () {  Name = "context", Type = new ($"I{boundedContext.Name}DbContext")}
                    ]
                });

                commandHandlerModel.Methods.Add(new()
                {
                    Name = "Handle",
                    Params = [
                        new() { Name = "request", Type = new ($"{command.Name}Request") },
                        ParamModel.CancellationToken
                    ],
                    Async = true,
                    ReturnType = TypeModel.TaskOf($"{command.Name}Response"),
                    Body = command.Kind switch
                    {
                        RequestKind.Create => new CreateRequestHandlerExpressionModel(command),
                        RequestKind.Update => new UpdateRequestHandlerExpressionModel(command),
                        RequestKind.Delete => new DeleteRequestHandlerExpressionModel(command),
                        _ => throw new NotImplementedException()
                    }
                });

                model.Files.Add(new CodeFileModel<ClassModel>(commandHandlerModel, commandHandlerModel.Name, requestHandlersDirectory, CSharp) { Namespace = $"{microservice.Name}.RequestHandlers" });
            }

            foreach(var query in aggregate.Queries)
            {
                var queryHandlerModel = new ClassModel($"{query.Name}Handler");

                queryHandlerModel.Usings.Add(new("MediatR"));

                queryHandlerModel.Usings.Add(new(aggregateNamespace));

                queryHandlerModel.Implements.Add(new($"IRequestHandler<{query.Name}Request, {query.Name}Response>"));

                queryHandlerModel.Fields =
                    [
                        FieldModel.LoggerOf($"{query.Name}Handler"),
                        new () { Name = "_context", Type = new ($"I{boundedContext.Name}DbContext") }
                    ];

                queryHandlerModel.Constructors.Add(new(queryHandlerModel, queryHandlerModel.Name)
                {
                    Params =
                    [
                        ParamModel.LoggerOf($"{query.Name}Handler"),
                        new () { Name = "context", Type = new ($"I{boundedContext.Name}DbContext") }
                    ]
                });

                queryHandlerModel.Methods.Add(new()
                {
                    Name = "Handle",
                    Params = [
                        new() { Name = "request", Type = new ($"{query.Name}Request") },
                        ParamModel.CancellationToken
                    ],
                    Async = true,
                    ReturnType = TypeModel.TaskOf($"{query.Name}Response"),
                    Body = query.Kind switch
                    {
                        RequestKind.GetById => new GetByIdRequestHandlerExpressionModel(query),
                        RequestKind.Get => new GetRequestHandlerExpressionModel(query),
                        _ => throw new NotImplementedException()
                    }
                });

                queryHandlerModel.Usings.Add(new("MediatR"));

                queryHandlerModel.Usings.Add(new(aggregateNamespace));

                model.Files.Add(new CodeFileModel<ClassModel>(queryHandlerModel, queryHandlerModel.Name, requestHandlersDirectory, CSharp) { Namespace = $"{microservice.Name}.RequestHandlers" });
            }
        }

        return model;
    }

    public async Task<IEnumerable<FileModel>> AggregateCreateAsync(Endpoint.DomainDrivenDesign.Core.IDataContext context, AggregateModel aggregate, string directory, CancellationToken cancellationToken)
    {
        var files = new List<FileModel>();

        var aggregateDirectory = _fileSystem.Path.Combine(directory, aggregate.Name);

        var aggregateClassModel = new ClassModel(aggregate.Name);

        var aggregateDtoModel = new ClassModel($"{aggregate.Name}Dto");

        var aggregateNamespace = $"{context.ProductName}.Models.{aggregate.Name}";

        foreach (var property in aggregate.Properties)
        {
            aggregateClassModel.Properties.Add(new(aggregateClassModel, AccessModifier.Public, property.ToType(), property.Name, PropertyAccessorModel.GetSet));

            aggregateDtoModel.Properties.Add(new(aggregateClassModel, AccessModifier.Public, property.ToType(), property.Name, PropertyAccessorModel.GetSet));
        }

        files.Add(await AggregateExtenionCreateAsync(aggregate, aggregateDirectory));

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
                        commandRequestClassModel.Properties.Add(new(commandRequestClassModel, AccessModifier.Public, property.ToType(), property.Name, PropertyAccessorModel.GetSet));
                    }
                    break;

                case RequestKind.Update:
                    foreach(var property in aggregate.Properties)
                    {
                        commandRequestClassModel.Properties.Add(new(commandRequestClassModel, AccessModifier.Public, property.ToType(), property.Name, PropertyAccessorModel.GetSet));
                    }
                    
                    break;

                case RequestKind.Delete:

                    var keyProperty = aggregate.Properties.Single(x => x.Key);

                    commandRequestClassModel.Properties.Add(new (commandRequestClassModel, AccessModifier.Public, keyProperty.ToType(), keyProperty.Name, PropertyAccessorModel.GetSet));

                    break;
            }

            files.Add(new CodeFileModel<ClassModel>(commandRequestClassModel, commandRequestClassModel.Name, aggregateDirectory, CSharp) { Namespace = aggregateNamespace });

            var commandResponseClassModel = new ClassModel($"{command.Name}Response");

            switch (command.Kind)
            {
                case RequestKind.Create:    
                case RequestKind.Update:
                    commandResponseClassModel.Properties.Add(new(commandRequestClassModel, AccessModifier.Public, new($"{command.Aggregate.Name}Dto?"), command.Aggregate.Name, PropertyAccessorModel.GetSet));
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

                    queryRequestClassModel.Properties.Add(new(queryRequestClassModel, AccessModifier.Public, keyProperty.ToType(), keyProperty.Name, PropertyAccessorModel.GetSet));
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

        foreach (var entity in aggregate.Entities)
        {
            var entityClassModel = new ClassModel(entity.Name);

            foreach (var property in entity.Properties)
            {
                entityClassModel.Properties.Add(new(entityClassModel, AccessModifier.Public, property.ToType(), property.Name, PropertyAccessorModel.GetSet));
            }

            files.Add(new CodeFileModel<ClassModel>(entityClassModel, entityClassModel.Name, aggregateDirectory, CSharp) { Namespace = aggregateNamespace });
        }

        return files;
    }

    public async Task<FileModel> DbContextCreateAsync(IDataContext context, Microservice microservice, BoundedContext boundedContext, string directory)
    {
        var model = new ClassModel($"{boundedContext.Name}DbContext");

        model.Implements.Add(new("DbContext"));

        model.Implements.Add(new($"I{boundedContext.Name}DbContext"));

        model.Usings.Add(new("Microsoft.EntityFrameworkCore"));

        model.Constructors.Add(new (model, model.Name)
        {
            BaseParams =
            [
                "options",
            ],
            Params =
            [
                new() { Name = "options", Type = new($"DbContextOptions<{boundedContext.Name}DbContext>") }
            ]
        });

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
        var model = new InterfaceModel($"I{boundedContext.Name}DbContext");

        model.Usings.Add(new("Microsoft.EntityFrameworkCore"));

        foreach (var aggregate in boundedContext.Aggregates)
        {
            var aggregateNamespace = $"{context.ProductName}.Models.{aggregate.Name}";

            model.Usings.Add(new(aggregateNamespace));

            model.Properties.Add(new (model, AccessModifier.Public, TypeModel.DbSetOf(aggregate.Name), aggregate.Name.Pluralize(), PropertyAccessorModel.GetPrivateSet));
        }

        model.Methods.Add(new()
        {
            Name = "SaveChangesAsync",
            Interface = true,
            Params =
            [
                ParamModel.CancellationToken
            ],
            ReturnType = TypeModel.TaskOf("int")
        });

        return new CodeFileModel<InterfaceModel>(model, model.Name, directory, CSharp) { Namespace = $"{microservice.Name}" };
    }

    public async Task<FileModel> ControllerCreateAsync(Microservice microservice, AggregateModel aggregate, string directory)
    {
        var model = new ClassModel($"{aggregate.Name.Pluralize()}Controller");

        model.Implements.Add(new("Controller"));

        model.Usings.Add(new("MediatR"));

        model.Usings.Add(new("Microsoft.AspNetCore.Mvc"));

        model.Usings.Add(new("System.Net.Mime"));

        model.Usings.Add(new($"{microservice.ProductName}.Models.{aggregate.Name}"));

        model.Fields.Add(FieldModel.LoggerOf(model.Name));

        model.Fields.Add(FieldModel.Mediator);

        model.Constructors.Add(new(model, model.Name)
        {
            Params = [
                ParamModel.LoggerOf(model.Name),
                ParamModel.Mediator,
            ],
        });

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

        model.Packages.Add(new ("FluentValidation", "11.10.0"));

        model.Packages.Add(new ("MediatR", "12.4.1"));

        model.Packages.Add(new ("FluentValidation.DependencyInjectionExtensions", "11.10.0"));

        model.Files.Add(new ("ConfigureServices", model.Directory, CSharp)
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

        model.Files.Add(new ("ConfigureServices", model.Directory, CSharp)
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

        model.Files.Add(new ("ValidationBehavior", model.Directory, CSharp)
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

        model.Files.Add(new ("ResponseBase", model.Directory, CSharp)
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

    public async Task<FileModel> AggregateExtenionCreateAsync(AggregateModel aggregate, string directory)
    {
        var model = new ClassModel($"{aggregate.Name}Extensions")
        {
            Static = true
        };

        model.Methods.Add(new ()
        {
            Static = true,
            ReturnType = new ($"{aggregate.Name}Dto"),
            Name = "ToDto",
            Params =
            [
                new ()
                {
                    ExtensionMethodParam = true,
                    Type = new TypeModel(aggregate.Name),
                    Name = aggregate.Name.ToCamelCase()
                }
            ],
            Body = new ToDtoExpressionModel(aggregate)
        });

        var file = new CodeFileModel<ClassModel>(model, model.Name, directory, CSharp) { Namespace = $"{aggregate.BoundedContext!.ProductName}.Models.{aggregate.Name}" };

        return file;
    }

    public async Task<FileModel> ApiConfigureServicesCreateAsync(Microservice microservice, string directory)
    {
        return new FileModel("ConfigureServices", directory, CSharp)
        {
            Body = $$"""
            using {{microservice.Name}};
            using Microsoft.AspNetCore.Cors.Infrastructure;
            using Microsoft.EntityFrameworkCore;

            namespace Microsoft.Extensions.DependencyInjection;

            public static class ConfigureApiServices
            {
                public static void AddApiServices(this IServiceCollection services, Action<CorsPolicyBuilder> configureCorsPolicyBuilder, string connectionString)
                {
                    services.AddControllers();

                    services.AddCors(options => options.AddPolicy("CorsPolicy",
                        builder =>
                        {
                            configureCorsPolicyBuilder.Invoke(builder);

                            builder
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .SetIsOriginAllowed(isOriginAllowed: _ => true)
                            .AllowCredentials();
                        }));

                    services.AddTransient<I{{microservice.BoundedContextName}}DbContext, {{microservice.BoundedContextName}}DbContext>();

                    services.AddDbContextPool<{{microservice.BoundedContextName}}DbContext>(options =>
                    {
                        options.UseSqlServer(connectionString,
                            builder => builder.MigrationsAssembly("{{microservice.Name}}")
                                .EnableRetryOnFailure())
                        .EnableThreadSafetyChecks(false)
                        .LogTo(Console.WriteLine)
                        .EnableSensitiveDataLogging();
                    });

                    services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<Program>());
                }
            }
            """
        };
    }

    public async Task<FileModel> ApiProgramCreateAsync(BoundedContext boundedContext, Microservice microservice, string directory)
    {
        return new FileModel("Program", directory, CSharp)
        {
            Body = $$"""
            using {{microservice.Name}};
            {{string.Join(Environment.NewLine, boundedContext.Aggregates.Select(x => $"using {boundedContext.ProductName}.Models.{x.Name};"))}}
            using Microsoft.EntityFrameworkCore;

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddValidation(typeof({{boundedContext.Aggregates.First().Name}}));

            builder.Services.AddApiServices(corsPolicyBuilder =>
            {
                corsPolicyBuilder.WithOrigins(builder.Configuration["WithOrigins"]!.Split(','));
            }, builder.Configuration.GetConnectionString("DefaultConnection")!);

            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();

            app.UseHttpsRedirection();

            var services = (IServiceScopeFactory)app.Services.GetRequiredService(typeof(IServiceScopeFactory));

            using (var scope = services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<{{boundedContext.Name}}DbContext>();

                if (args.Contains("ci"))
                    args = new string[4] { "dropdb", "migratedb", "seeddb", "stop" };

                if (args.Contains("dropdb"))
                {

                }

                if (args.Contains("migratedb"))
                {
                    context.Database.Migrate();
                }

                if (args.Contains("seeddb"))
                {

                }

                if (args.Contains("stop"))
                    Environment.Exit(0);
            }

            app.Run();
            """
        };
    }

    public async Task<FileModel> ApiAppSettingsCreateAsync(BoundedContext boundedContext, string directory)
    {
        return new FileModel("appsettings.Development", directory, ".json")
        {
            Body = $$"""
            {
              "Logging": {
                "LogLevel": {
                  "Default": "Information",
                  "Microsoft.AspNetCore": "Warning"
                }
              },
              "ConnectionStrings": {
                "DefaultConnection": "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog={{boundedContext.Name}};Integrated Security=SSPI;"
              },
              "WithOrigins":  "https://localhost:4200/"
            }
            
            """
        };
    }
}

