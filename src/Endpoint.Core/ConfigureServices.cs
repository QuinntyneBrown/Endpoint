// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Endpoint.Core;
using Endpoint.Core.Artifacts;
using Endpoint.Core.Artifacts.AngularProjects;
using Endpoint.Core.Artifacts.Files.Factories;
using Endpoint.Core.Artifacts.Files.Services;
using Endpoint.Core.Artifacts.Folders.Factories;
using Endpoint.Core.Artifacts.Folders.Services;
using Endpoint.Core.Artifacts.FullStack;
using Endpoint.Core.Artifacts.Git;
using Endpoint.Core.Artifacts.Projects.Factories;
using Endpoint.Core.Artifacts.Projects.Services;
using Endpoint.Core.Artifacts.Services;
using Endpoint.Core.Artifacts.Solutions.Factories;
using Endpoint.Core.Artifacts.Solutions.Services;
using Endpoint.Core.Artifacts.SpecFlow;
using Endpoint.Core.Artifacts.Units;
using Endpoint.Core.DataModel;
using Endpoint.Core.Events;
using Endpoint.Core.Services;
using Endpoint.Core.Syntax.Classes.Factories;
using Endpoint.Core.Syntax.Classes.Services;
using Endpoint.Core.Syntax.Controllers;
using Endpoint.Core.Syntax.Entities;
using Endpoint.Core.Syntax.Expressions;
using Endpoint.Core.Syntax.Methods.Factories;
using Endpoint.Core.Syntax.Namespaces.Factories;
using Endpoint.Core.Syntax.Properties.Factories;
using Endpoint.Core.Syntax.RouteHandlers;
using Endpoint.Core.Syntax.Types;
using Endpoint.Core.Syntax.Units.Factories;
using Endpoint.Core.Syntax.Units.Services;
using Endpoint.Core.SystemModels;

namespace Microsoft.Extensions.DependencyInjection;

using Type = System.Type;

public static class ConfigureServices
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IDataModelContextProvider<DataModelContext>,DataModelContextProvider<DataModelContext>>();
        services.AddSingleton<IDataModelContext,DataModelContext>();
        services.AddSingleton<IEndpointEventContainer,EndpointEventContainer>();
        services.AddSingleton<IFullStackFactory,FullStackFactory>();
        services.AddSingleton<ISystemContextFactory, SystemContextFactory>();
        services.AddSingleton<ISystemContext, SystemContext>();
        services.AddSingleton<IDocumentFactory, DocumentFactory>();
        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<ICodeFormatterService, DotnetCodeFormatterService>();
        services.AddSingleton<ICodeAnalysisService, CodeAnalysisService>();
        services.AddSingleton<ISyntaxUnitFactory, SyntaxUnitFactory>();
        services.AddSingleton<INamespaceFactory, NamespaceFactory>();
        services.AddSingleton<IPropertyFactory, PropertyFactory>();
        services.AddSingleton<ICqrsFactory, CqrsFactory>();
        services.AddSingleton<ISyntaxFactory, SyntaxFactory>();
        services.AddSingleton<IExpressionFactory, ExpressionFactory>();
        services.AddSingleton<IObjectCache, ObjectCache>();
        services.AddSingleton<ITypeFactory, TypeFactory>();
        services.AddSingleton<IPlaywrightService, PlaywrightService>();
        services.AddSingleton<IMethodFactory, MethodFactory>();
        services.AddSingleton<ISpecFlowService, SpecFlowService>();
        services.AddSingleton<IFolderFactory, FolderFactory>();
        services.AddSingleton<IFolderService, FolderService>();
        services.AddSingleton<IDomainDrivenDesignService, DomainDrivenDesignService>();

        services.AddSingleton<IClassService, ClassService>();
        services.AddSingleton<IUtlitityService, UtlitityService>();
        services.AddSingleton<ISignalRService, SignalRService>();
        services.AddSingleton<IReactService, ReactService>();
        services.AddSingleton<ICoreProjectService, CoreProjectService>();
        services.AddSingleton<ILitService, LitService>();
        services.AddSingleton<IAngularService, AngularService>();
        services.AddSingleton<IInfrastructureProjectService, InfrastructureProjectService>();
        services.AddSingleton<IApiProjectService, ApiProjectService>();
        services.AddSingleton<IAggregateService, AggregateService>();
        services.AddSingleton<ICommandService, CommandService>();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<ITemplateProcessor, LiquidTemplateProcessor>();
        services.AddSingleton<INamingConventionConverter, NamingConventionConverter>();
        services.AddSingleton<ISettingsProvider, SettingsProvider>();
        services.AddSingleton<ITenseConverter, TenseConverter>();
        services.AddSingleton<IContext, Context>();
        services.AddSingleton<IFileFactory, FileFactory>();
        services.AddSingleton<INamespaceProvider, NamespaceProvider>();
        services.AddSingleton<IDomainDrivenDesignFileService, DomainDrivenDesignFileService>();
        services.AddSingleton<IDependencyInjectionService, DependencyInjectionService>();
        services.AddSingleton<IEntityFactory, EntityFactory>();
        services.AddSingleton<IFileNamespaceProvider, FileNamespaceProvider>();
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<ISolutionService, SolutionService>();
        services.AddSingleton<IFileProvider, FileProvider>();
        services.AddSingleton<ISolutionNamespaceProvider, SolutionNamespaceProvider>();
        services.AddSingleton<ISolutionFactory, SolutionFactory>();
        services.AddSingleton<IControllerFactory, ControllerFactory>();

        services.AddSingleton<IMinimalApiService, MinimalApiService>();
        services.AddSingleton<IFileFactory, FileFactory>();
        services.AddSingleton<IAngularProjectFactory, AngularProjectFactory>();

        services.AddSingleton<IArtifactGenerator, ArtifactGenerator>();
        services.AddSingleton<ISyntaxGenerator, SyntaxGenerator>();

        services.AddSingleton<IClipboardService, ClipboardService>();

        services.AddSingleton<IEntityFileFactory, EntityFileFactory>();
        services.AddSingleton<IProjectFactory, ProjectFactory>();
        services.AddSingleton<IRouteHandlerFactory, RouteHandlerFactory>();
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(Constants).Assembly));

        services.AddSingleton<IClassFactory, ClassFactory>();

        services.AddSingleton<IArtifactParser, ArtifactParser>();
        services.AddSingleton<ISyntaxParser, SyntaxParser>();

        services.AddSingleton(typeof(IGenericSyntaxGenerationStrategy<>), typeof(Constants).Assembly);
        services.AddSingleton(typeof(IGenericArtifactGenerationStrategy<>), typeof(Constants).Assembly);
        services.AddSingleton(typeof(ISyntaxParsingStrategy<>), typeof(Constants).Assembly);
        services.AddSingleton(typeof(IArtifactParsingStrategy<>), typeof(Constants).Assembly);

        services.AddSingleton<ITemplateLocator, EmbeddedResourceTemplateLocatorBase<CodeGeneratorApplication>>();
    }

    public static void AddSingleton(this IServiceCollection services, Type @interface, Assembly assembly)
    {
        var implementations = assembly.GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                type.GetInterfaces().Any(interfaceType =>
                    interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == @interface))
            .ToList();

        foreach (var implementation in implementations)
        {
            foreach (var implementedInterface in implementation.GetInterfaces())
            {
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == @interface)
                {
                    services.AddSingleton(implementedInterface, implementation);
                }
            }
        }
    }

    public static void AddCoreServices<T>(this IServiceCollection services)
        where T : class
    {
        services.AddSingleton<ITemplateLocator, EmbeddedResourceTemplateLocatorBase<T>>();
        AddCoreServices(services);
    }
}




