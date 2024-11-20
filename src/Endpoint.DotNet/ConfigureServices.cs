// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.AngularProjects;
using Endpoint.DotNet.Artifacts.Files.Factories;
using Endpoint.DotNet.Artifacts.Files.Services;
using Endpoint.DotNet.Artifacts.Folders.Factories;
using Endpoint.DotNet.Artifacts.Folders.Services;
using Endpoint.DotNet.Artifacts.FullStack;
using Endpoint.DotNet.Artifacts.Git;
using Endpoint.DotNet.Artifacts.Projects.Factories;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.DotNet.Artifacts.Services;
using Endpoint.DotNet.Artifacts.Solutions.Factories;
using Endpoint.DotNet.Artifacts.Solutions.Services;
using Endpoint.DotNet.Artifacts.SpecFlow;
using Endpoint.DotNet.Artifacts.Units;
using Endpoint.DotNet.DataModel;
using Endpoint.DotNet.Events;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes.Factories;
using Endpoint.DotNet.Syntax.Classes.Services;
using Endpoint.DotNet.Syntax.Controllers;
using Endpoint.DotNet.Syntax.Entities;
using Endpoint.DotNet.Syntax.Expressions;
using Endpoint.DotNet.Syntax.Methods.Factories;
using Endpoint.DotNet.Syntax.Namespaces.Factories;
using Endpoint.DotNet.Syntax.Properties.Factories;
using Endpoint.DotNet.Syntax.RouteHandlers;
using Endpoint.DotNet.Syntax.Types;
using Endpoint.DotNet.Syntax.Units.Factories;
using Endpoint.DotNet.Syntax.Units.Services;
using Endpoint.DotNet.SystemModels;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static void AddDotNetServices(this IServiceCollection services)
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
        services.AddSingleton<ITypeFactory, TypeFactory>();
        services.AddSingleton<IPlaywrightService, PlaywrightService>();
        services.AddSingleton<IMethodFactory, MethodFactory>();
        services.AddSingleton<ISpecFlowService, SpecFlowService>();
        services.AddSingleton<IFolderFactory, FolderFactory>();
        services.AddSingleton<IFolderService, FolderService>();
        services.AddSingleton<IDomainDrivenDesignService, DomainDrivenDesignService>();

        services.AddSingleton<IClassService, ClassService>();
        services.AddSingleton<IUtilityService, UtlitityService>();
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

        services.AddSingleton<IClipboardService, ClipboardService>();

        services.AddSingleton<IEntityFileFactory, EntityFileFactory>();
        services.AddSingleton<IProjectFactory, ProjectFactory>();
        services.AddSingleton<IRouteHandlerFactory, RouteHandlerFactory>();
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(Constants).Assembly));

        services.AddSingleton<IClassFactory, ClassFactory>();

        services.AddSingleton<IArtifactParser, ArtifactParser>();
        services.AddSingleton<ISyntaxParser, SyntaxParser>();

        services.AddSingleton<ITemplateLocator, EmbeddedResourceTemplateLocatorBase<CodeGeneratorApplication>>();
    }


}