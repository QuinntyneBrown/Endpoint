# Design 2: N-Tier Solution Template System

## Overview

This design defines a comprehensive template system for generating n-tier .NET solutions. The system supports multiple architectural patterns and allows customization at every layer.

## Goals

1. Support multiple n-tier architectural patterns
2. Provide configurable project templates
3. Enable layer-specific code generation
4. Support dependency injection and cross-cutting concerns
5. Allow template customization and extension

## N-Tier Architecture Patterns

### Pattern 1: Clean Architecture (Default)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          Presentation Layer                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │   Web API   │  │   Angular   │  │   gRPC      │  │   GraphQL   │    │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         Application Layer                                │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │  Commands   │  │   Queries   │  │  Validators │  │   DTOs      │    │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                           Domain Layer                                   │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │  Entities   │  │ Value Objs  │  │   Events    │  │ Interfaces  │    │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                       Infrastructure Layer                               │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐    │
│  │    EF Core  │  │ Repositories│  │  Services   │  │  External   │    │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

### Pattern 2: Vertical Slice Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Features                                    │
├─────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────┐  ┌─────────────────────┐  ┌────────────────┐  │
│  │   Orders Feature    │  │  Products Feature   │  │ Users Feature  │  │
│  │  ├── CreateOrder    │  │  ├── CreateProduct  │  │ ├── Register   │  │
│  │  │   ├── Command    │  │  │   ├── Command    │  │ │   ├── Cmd    │  │
│  │  │   ├── Handler    │  │  │   ├── Handler    │  │ │   ├── Handler│  │
│  │  │   └── Validator  │  │  │   └── Validator  │  │ │   └── Valid  │  │
│  │  ├── GetOrders      │  │  ├── GetProducts    │  │ ├── GetUsers   │  │
│  │  └── ...            │  │  └── ...            │  │ └── ...        │  │
│  └─────────────────────┘  └─────────────────────┘  └────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

### Pattern 3: Modular Monolith

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           Host Application                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐       │
│  │   Orders Module  │  │  Catalog Module  │  │  Identity Module │       │
│  │  ┌────────────┐  │  │  ┌────────────┐  │  │  ┌────────────┐  │       │
│  │  │   Domain   │  │  │  │   Domain   │  │  │  │   Domain   │  │       │
│  │  ├────────────┤  │  │  ├────────────┤  │  │  ├────────────┤  │       │
│  │  │Application │  │  │  │Application │  │  │  │Application │  │       │
│  │  ├────────────┤  │  │  ├────────────┤  │  │  ├────────────┤  │       │
│  │  │ Infra      │  │  │  │ Infra      │  │  │  │ Infra      │  │       │
│  │  └────────────┘  │  │  └────────────┘  │  │  └────────────┘  │       │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘       │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                     Shared Kernel / Common                       │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

### Pattern 4: Microservices

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           API Gateway                                    │
└───────────────────────────────┬─────────────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        │                       │                       │
        ▼                       ▼                       ▼
┌───────────────┐      ┌───────────────┐      ┌───────────────┐
│ Orders Service│      │Catalog Service│      │Identity Service│
│  ┌─────────┐  │      │  ┌─────────┐  │      │  ┌─────────┐  │
│  │   API   │  │      │  │   API   │  │      │  │   API   │  │
│  ├─────────┤  │      │  ├─────────┤  │      │  ├─────────┤  │
│  │  Core   │  │      │  │  Core   │  │      │  │  Core   │  │
│  ├─────────┤  │      │  ├─────────┤  │      │  ├─────────┤  │
│  │  Infra  │  │      │  │  Infra  │  │      │  │  Infra  │  │
│  └─────────┘  │      │  └─────────┘  │      │  └─────────┘  │
│       │       │      │       │       │      │       │       │
│   ┌───┴───┐   │      │   ┌───┴───┐   │      │   ┌───┴───┐   │
│   │  DB   │   │      │   │  DB   │   │      │   │  DB   │   │
│   └───────┘   │      │   └───────┘   │      │   └───────┘   │
└───────────────┘      └───────────────┘      └───────────────┘
```

## Template Configuration Schema

```csharp
namespace Endpoint.PlantUml.Templates
{
    public class SolutionTemplateConfiguration
    {
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public ArchitecturePattern Pattern { get; set; }
        public List<LayerConfiguration> Layers { get; set; } = new();
        public CrossCuttingConfiguration CrossCutting { get; set; }
        public TestingConfiguration Testing { get; set; }
        public DocumentationConfiguration Documentation { get; set; }
    }

    public class LayerConfiguration
    {
        public string Name { get; set; }
        public LayerType Type { get; set; }
        public string ProjectSuffix { get; set; }
        public DotNetProjectType ProjectType { get; set; }
        public List<string> DefaultPackages { get; set; } = new();
        public List<string> DefaultReferences { get; set; } = new();
        public List<FolderConfiguration> Folders { get; set; } = new();
        public List<FileConfiguration> DefaultFiles { get; set; } = new();
    }

    public enum LayerType
    {
        Domain,
        Application,
        Infrastructure,
        Presentation,
        Shared,
        Testing
    }

    public class FolderConfiguration
    {
        public string Name { get; set; }
        public string Purpose { get; set; }
        public List<string> FilePatterns { get; set; } = new();
    }

    public class CrossCuttingConfiguration
    {
        public bool IncludeLogging { get; set; } = true;
        public string LoggingProvider { get; set; } = "Serilog";
        public bool IncludeCaching { get; set; }
        public string CachingProvider { get; set; } = "Memory";
        public bool IncludeHealthChecks { get; set; } = true;
        public bool IncludeOpenTelemetry { get; set; }
        public bool IncludeExceptionHandling { get; set; } = true;
    }

    public class TestingConfiguration
    {
        public bool IncludeUnitTests { get; set; } = true;
        public bool IncludeIntegrationTests { get; set; }
        public bool IncludeArchitectureTests { get; set; }
        public string TestFramework { get; set; } = "xUnit";
        public string MockingFramework { get; set; } = "NSubstitute";
    }
}
```

## Template Definitions

### Clean Architecture Template

```csharp
public static SolutionTemplateConfiguration CleanArchitectureTemplate => new()
{
    TemplateName = "CleanArchitecture",
    Description = "Clean Architecture with Domain-Driven Design principles",
    Pattern = ArchitecturePattern.CleanArchitecture,
    Layers = new List<LayerConfiguration>
    {
        new LayerConfiguration
        {
            Name = "Domain",
            Type = LayerType.Domain,
            ProjectSuffix = ".Domain",
            ProjectType = DotNetProjectType.ClassLib,
            DefaultPackages = new List<string>(),
            Folders = new List<FolderConfiguration>
            {
                new() { Name = "Entities", Purpose = "Domain entities" },
                new() { Name = "ValueObjects", Purpose = "Value objects" },
                new() { Name = "Events", Purpose = "Domain events" },
                new() { Name = "Interfaces", Purpose = "Repository interfaces" },
                new() { Name = "Exceptions", Purpose = "Domain exceptions" },
                new() { Name = "Enums", Purpose = "Domain enumerations" }
            }
        },
        new LayerConfiguration
        {
            Name = "Application",
            Type = LayerType.Application,
            ProjectSuffix = ".Application",
            ProjectType = DotNetProjectType.ClassLib,
            DefaultPackages = new List<string>
            {
                "MediatR",
                "FluentValidation",
                "AutoMapper",
                "FluentValidation.DependencyInjectionExtensions"
            },
            DefaultReferences = new List<string> { "Domain" },
            Folders = new List<FolderConfiguration>
            {
                new() { Name = "Common", Purpose = "Shared application components" },
                new() { Name = "Common/Behaviors", Purpose = "MediatR pipeline behaviors" },
                new() { Name = "Common/Interfaces", Purpose = "Application service interfaces" },
                new() { Name = "Common/Mappings", Purpose = "AutoMapper profiles" },
                new() { Name = "{Feature}/Commands", Purpose = "Feature commands" },
                new() { Name = "{Feature}/Queries", Purpose = "Feature queries" },
                new() { Name = "{Feature}/EventHandlers", Purpose = "Domain event handlers" }
            }
        },
        new LayerConfiguration
        {
            Name = "Infrastructure",
            Type = LayerType.Infrastructure,
            ProjectSuffix = ".Infrastructure",
            ProjectType = DotNetProjectType.ClassLib,
            DefaultPackages = new List<string>
            {
                "Microsoft.EntityFrameworkCore",
                "Microsoft.EntityFrameworkCore.SqlServer",
                "Microsoft.EntityFrameworkCore.Tools"
            },
            DefaultReferences = new List<string> { "Application" },
            Folders = new List<FolderConfiguration>
            {
                new() { Name = "Persistence", Purpose = "Database context and configurations" },
                new() { Name = "Persistence/Configurations", Purpose = "Entity type configurations" },
                new() { Name = "Persistence/Migrations", Purpose = "EF Core migrations" },
                new() { Name = "Repositories", Purpose = "Repository implementations" },
                new() { Name = "Services", Purpose = "Infrastructure services" },
                new() { Name = "Identity", Purpose = "Authentication/Authorization" }
            }
        },
        new LayerConfiguration
        {
            Name = "Api",
            Type = LayerType.Presentation,
            ProjectSuffix = ".Api",
            ProjectType = DotNetProjectType.WebApi,
            DefaultPackages = new List<string>
            {
                "Swashbuckle.AspNetCore",
                "Serilog.AspNetCore",
                "Microsoft.AspNetCore.Authentication.JwtBearer"
            },
            DefaultReferences = new List<string> { "Application", "Infrastructure" },
            Folders = new List<FolderConfiguration>
            {
                new() { Name = "Controllers", Purpose = "API controllers" },
                new() { Name = "Filters", Purpose = "Action filters" },
                new() { Name = "Middleware", Purpose = "Custom middleware" },
                new() { Name = "Models", Purpose = "API request/response models" }
            }
        }
    },
    CrossCutting = new CrossCuttingConfiguration
    {
        IncludeLogging = true,
        LoggingProvider = "Serilog",
        IncludeExceptionHandling = true,
        IncludeHealthChecks = true
    },
    Testing = new TestingConfiguration
    {
        IncludeUnitTests = true,
        IncludeIntegrationTests = true,
        IncludeArchitectureTests = true,
        TestFramework = "xUnit"
    }
};
```

## Project Factory Implementation

```csharp
namespace Endpoint.PlantUml.Generation
{
    public interface IProjectTemplateFactory
    {
        ProjectModel CreateDomainProject(DomainLayerModel domain, LayerConfiguration config);
        ProjectModel CreateApplicationProject(ApplicationLayerModel app, LayerConfiguration config);
        ProjectModel CreateInfrastructureProject(InfrastructureLayerModel infra, LayerConfiguration config);
        ProjectModel CreateApiProject(PresentationLayerModel api, LayerConfiguration config);
        ProjectModel CreateTestProject(TestLayerModel tests, TestingConfiguration config);
    }

    public class ProjectTemplateFactory : IProjectTemplateFactory
    {
        private readonly ITemplateProcessor _templateProcessor;
        private readonly IPackageReferenceProvider _packageProvider;

        public ProjectModel CreateDomainProject(DomainLayerModel domain, LayerConfiguration config)
        {
            var project = new ProjectModel
            {
                Name = $"{domain.SolutionName}{config.ProjectSuffix}",
                ProjectType = config.ProjectType,
                Packages = config.DefaultPackages.Select(p => new PackageModel { Name = p }).ToList()
            };

            // Add base entity
            project.Files.Add(CreateBaseEntityFile(domain));

            // Add aggregate roots
            foreach (var aggregate in domain.Aggregates)
            {
                project.Files.Add(CreateAggregateFile(aggregate));
                project.Files.Add(CreateRepositoryInterfaceFile(aggregate));
            }

            // Add value objects
            foreach (var valueObject in domain.ValueObjects)
            {
                project.Files.Add(CreateValueObjectFile(valueObject));
            }

            // Add domain events
            foreach (var domainEvent in domain.Events)
            {
                project.Files.Add(CreateDomainEventFile(domainEvent));
            }

            // Add domain exceptions
            foreach (var exception in domain.Exceptions)
            {
                project.Files.Add(CreateDomainExceptionFile(exception));
            }

            return project;
        }

        public ProjectModel CreateApplicationProject(ApplicationLayerModel app, LayerConfiguration config)
        {
            var project = new ProjectModel
            {
                Name = $"{app.SolutionName}{config.ProjectSuffix}",
                ProjectType = config.ProjectType,
                Packages = config.DefaultPackages.Select(p => new PackageModel { Name = p }).ToList(),
                References = config.DefaultReferences.Select(r => $"{app.SolutionName}.{r}").ToList()
            };

            // Add common components
            project.Files.Add(CreateDependencyInjectionFile(app));
            project.Files.Add(CreateValidationBehaviorFile(app));
            project.Files.Add(CreateLoggingBehaviorFile(app));
            project.Files.Add(CreateMappingProfileFile(app));

            // Add feature-based CQRS
            foreach (var feature in app.Features)
            {
                // Commands
                foreach (var command in feature.Commands)
                {
                    project.Files.Add(CreateCommandFile(feature, command));
                    project.Files.Add(CreateCommandHandlerFile(feature, command));
                    project.Files.Add(CreateCommandValidatorFile(feature, command));
                }

                // Queries
                foreach (var query in feature.Queries)
                {
                    project.Files.Add(CreateQueryFile(feature, query));
                    project.Files.Add(CreateQueryHandlerFile(feature, query));
                }

                // DTOs
                foreach (var dto in feature.DTOs)
                {
                    project.Files.Add(CreateDtoFile(feature, dto));
                }
            }

            return project;
        }

        // Additional factory methods...
    }
}
```

## Layer Models

```csharp
namespace Endpoint.PlantUml.Models.Layers
{
    public class DomainLayerModel
    {
        public string SolutionName { get; set; }
        public string Namespace { get; set; }
        public List<AggregateModel> Aggregates { get; set; } = new();
        public List<EntityModel> Entities { get; set; } = new();
        public List<ValueObjectModel> ValueObjects { get; set; } = new();
        public List<DomainEventModel> Events { get; set; } = new();
        public List<EnumModel> Enums { get; set; } = new();
        public List<DomainExceptionModel> Exceptions { get; set; } = new();
    }

    public class ApplicationLayerModel
    {
        public string SolutionName { get; set; }
        public string Namespace { get; set; }
        public List<FeatureModel> Features { get; set; } = new();
        public List<ServiceInterfaceModel> ServiceInterfaces { get; set; } = new();
        public List<BehaviorModel> Behaviors { get; set; } = new();
    }

    public class InfrastructureLayerModel
    {
        public string SolutionName { get; set; }
        public string Namespace { get; set; }
        public DatabaseConfiguration Database { get; set; }
        public List<RepositoryModel> Repositories { get; set; } = new();
        public List<ServiceImplementationModel> Services { get; set; } = new();
        public List<EntityConfigurationModel> EntityConfigurations { get; set; } = new();
    }

    public class PresentationLayerModel
    {
        public string SolutionName { get; set; }
        public string Namespace { get; set; }
        public List<ControllerModel> Controllers { get; set; } = new();
        public List<EndpointModel> Endpoints { get; set; } = new();
        public AuthenticationConfiguration Authentication { get; set; }
        public SwaggerConfiguration Swagger { get; set; }
    }

    public class FeatureModel
    {
        public string Name { get; set; }
        public string EntityName { get; set; }
        public List<CommandModel> Commands { get; set; } = new();
        public List<QueryModel> Queries { get; set; } = new();
        public List<DtoModel> DTOs { get; set; } = new();
    }

    public class CommandModel
    {
        public string Name { get; set; }
        public CommandType Type { get; set; }
        public List<PropertyModel> Properties { get; set; } = new();
        public string ReturnType { get; set; }
    }

    public enum CommandType
    {
        Create,
        Update,
        Delete,
        Custom
    }
}
```

## Generated Solution Structure

For a solution named "ECommerce" with Orders and Products aggregates:

```
ECommerce/
├── src/
│   ├── ECommerce.Domain/
│   │   ├── Common/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── AggregateRoot.cs
│   │   │   └── ValueObject.cs
│   │   ├── Entities/
│   │   │   ├── Order.cs
│   │   │   ├── OrderItem.cs
│   │   │   └── Product.cs
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs
│   │   │   └── Address.cs
│   │   ├── Events/
│   │   │   ├── OrderCreatedEvent.cs
│   │   │   └── OrderCompletedEvent.cs
│   │   ├── Interfaces/
│   │   │   ├── IOrderRepository.cs
│   │   │   └── IProductRepository.cs
│   │   ├── Enums/
│   │   │   └── OrderStatus.cs
│   │   └── Exceptions/
│   │       └── DomainException.cs
│   │
│   ├── ECommerce.Application/
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   └── LoggingBehavior.cs
│   │   │   ├── Interfaces/
│   │   │   │   └── IApplicationDbContext.cs
│   │   │   └── Mappings/
│   │   │       └── MappingProfile.cs
│   │   ├── Orders/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateOrder/
│   │   │   │   │   ├── CreateOrderCommand.cs
│   │   │   │   │   ├── CreateOrderCommandHandler.cs
│   │   │   │   │   └── CreateOrderCommandValidator.cs
│   │   │   │   └── UpdateOrder/
│   │   │   │       └── ...
│   │   │   ├── Queries/
│   │   │   │   ├── GetOrders/
│   │   │   │   │   ├── GetOrdersQuery.cs
│   │   │   │   │   └── GetOrdersQueryHandler.cs
│   │   │   │   └── GetOrderById/
│   │   │   │       └── ...
│   │   │   └── DTOs/
│   │   │       └── OrderDto.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── ECommerce.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── OrderConfiguration.cs
│   │   │   │   └── ProductConfiguration.cs
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   │   ├── OrderRepository.cs
│   │   │   └── ProductRepository.cs
│   │   └── DependencyInjection.cs
│   │
│   └── ECommerce.Api/
│       ├── Controllers/
│       │   ├── OrdersController.cs
│       │   └── ProductsController.cs
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Program.cs
│       └── appsettings.json
│
├── tests/
│   ├── ECommerce.Domain.Tests/
│   ├── ECommerce.Application.Tests/
│   └── ECommerce.Api.IntegrationTests/
│
├── ECommerce.sln
└── Directory.Build.props
```

## Template Extensibility

### Custom Template Registration

```csharp
public interface ITemplateRegistry
{
    void RegisterTemplate(string name, SolutionTemplateConfiguration template);
    SolutionTemplateConfiguration GetTemplate(string name);
    IEnumerable<string> GetAvailableTemplates();
}

public class TemplateRegistry : ITemplateRegistry
{
    private readonly Dictionary<string, SolutionTemplateConfiguration> _templates = new();

    public TemplateRegistry()
    {
        // Register built-in templates
        RegisterTemplate("CleanArchitecture", BuiltInTemplates.CleanArchitecture);
        RegisterTemplate("VerticalSlice", BuiltInTemplates.VerticalSlice);
        RegisterTemplate("Modular", BuiltInTemplates.Modular);
        RegisterTemplate("Microservices", BuiltInTemplates.Microservices);
        RegisterTemplate("MinimalApi", BuiltInTemplates.MinimalApi);
    }

    // Load custom templates from JSON files
    public void LoadCustomTemplates(string directory)
    {
        foreach (var file in Directory.GetFiles(directory, "*.template.json"))
        {
            var json = File.ReadAllText(file);
            var template = JsonSerializer.Deserialize<SolutionTemplateConfiguration>(json);
            RegisterTemplate(template.TemplateName, template);
        }
    }
}
```

### Template Override Points

```csharp
public interface ILayerCustomizer
{
    void CustomizeDomain(DomainLayerModel domain);
    void CustomizeApplication(ApplicationLayerModel application);
    void CustomizeInfrastructure(InfrastructureLayerModel infrastructure);
    void CustomizePresentation(PresentationLayerModel presentation);
}
```

## Configuration File Support

Templates can be customized via `plantuml-solution.json`:

```json
{
  "solutionName": "ECommerce",
  "template": "CleanArchitecture",
  "options": {
    "includeAngularFrontend": true,
    "useAspire": true,
    "database": {
      "provider": "PostgreSql",
      "connectionStringName": "DefaultConnection"
    },
    "authentication": {
      "scheme": "JWT",
      "includeIdentity": true
    },
    "features": {
      "includeLogging": true,
      "loggingProvider": "Serilog",
      "includeCaching": true,
      "cachingProvider": "Redis",
      "includeHealthChecks": true,
      "includeOpenTelemetry": true
    },
    "testing": {
      "includeUnitTests": true,
      "includeIntegrationTests": true,
      "includeArchitectureTests": true,
      "testFramework": "xUnit",
      "mockingFramework": "NSubstitute"
    }
  },
  "customPackages": [
    { "project": "Domain", "packages": ["Ardalis.SmartEnum"] },
    { "project": "Infrastructure", "packages": ["Npgsql.EntityFrameworkCore.PostgreSQL"] }
  ]
}
```
