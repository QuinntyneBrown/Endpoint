# Design 4: .NET Aspire Orchestration Design

## Overview

This design defines the architecture for generating .NET Aspire-based solutions that provide cloud-native orchestration, service discovery, and observability out of the box. The system creates production-ready distributed applications from PlantUML diagrams.

## Goals

1. Generate .NET Aspire AppHost project
2. Create ServiceDefaults for consistent configuration
3. Configure service discovery and communication
4. Set up observability (OpenTelemetry, logging, metrics)
5. Integrate with Azure/cloud resources
6. Support local development with containers

## .NET Aspire Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           Aspire AppHost                                 │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                    Distributed Application                       │    │
│  │                                                                   │    │
│  │   ┌─────────────┐    ┌─────────────┐    ┌─────────────┐         │    │
│  │   │  API Host   │    │   Angular   │    │   Worker    │         │    │
│  │   │   (5000)    │◄──►│   (4200)    │    │   Service   │         │    │
│  │   └──────┬──────┘    └─────────────┘    └──────┬──────┘         │    │
│  │          │                                      │                │    │
│  │          │      Service Discovery               │                │    │
│  │          │      ◄─────────────────►             │                │    │
│  │          │                                      │                │    │
│  │   ┌──────┴──────┐    ┌─────────────┐    ┌──────┴──────┐         │    │
│  │   │  SQL Server │    │    Redis    │    │  RabbitMQ   │         │    │
│  │   │   (1433)    │    │   (6379)    │    │   (5672)    │         │    │
│  │   └─────────────┘    └─────────────┘    └─────────────┘         │    │
│  └─────────────────────────────────────────────────────────────────┘    │
│                                                                          │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │                     Observability                                │    │
│  │   OpenTelemetry  │  Structured Logging  │  Health Checks        │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

## Aspire Solution Structure

```
ECommerce.Aspire/
├── src/
│   ├── ECommerce.AppHost/                    # Orchestration project
│   │   ├── Program.cs
│   │   ├── ECommerce.AppHost.csproj
│   │   └── appsettings.json
│   │
│   ├── ECommerce.ServiceDefaults/            # Shared configuration
│   │   ├── Extensions.cs
│   │   ├── ECommerce.ServiceDefaults.csproj
│   │   └── OpenTelemetryExtensions.cs
│   │
│   ├── ECommerce.Domain/                     # Domain layer
│   ├── ECommerce.Application/                # Application layer
│   ├── ECommerce.Infrastructure/             # Infrastructure layer
│   │
│   ├── ECommerce.Api/                        # API service
│   │   ├── Program.cs
│   │   ├── ECommerce.Api.csproj
│   │   └── ...
│   │
│   ├── ECommerce.Worker/                     # Background worker
│   │   ├── Program.cs
│   │   ├── Worker.cs
│   │   └── ECommerce.Worker.csproj
│   │
│   └── ECommerce.Angular/                    # Angular frontend
│       └── ...
│
├── tests/
│   └── ...
│
└── ECommerce.sln
```

## AppHost Generation

### Program.cs

```csharp
// ECommerce.AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure Resources
var sqlServer = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("ecommerce-db");

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var rabbitMq = builder.AddRabbitMQ("messaging")
    .WithDataVolume()
    .WithManagementPlugin();

// API Service
var api = builder.AddProject<Projects.ECommerce_Api>("api")
    .WithReference(sqlServer)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WithExternalHttpEndpoints();

// Worker Service
var worker = builder.AddProject<Projects.ECommerce_Worker>("worker")
    .WithReference(sqlServer)
    .WithReference(rabbitMq);

// Angular Frontend
var angular = builder.AddNpmApp("angular", "../ECommerce.Angular")
    .WithReference(api)
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
```

### AppHost Generator

```csharp
namespace Endpoint.Aspire.Generation
{
    public interface IAspireAppHostGenerator
    {
        AspireProjectModel GenerateAppHost(AspireSolutionModel solution);
    }

    public class AspireSolutionModel
    {
        public string SolutionName { get; set; }
        public List<AspireServiceModel> Services { get; set; } = new();
        public List<AspireResourceModel> Resources { get; set; } = new();
        public AspireFrontendModel Frontend { get; set; }
        public ObservabilityConfiguration Observability { get; set; }
    }

    public class AspireServiceModel
    {
        public string Name { get; set; }
        public ServiceType Type { get; set; }
        public string ProjectPath { get; set; }
        public List<string> ResourceReferences { get; set; } = new();
        public List<string> ServiceReferences { get; set; } = new();
        public bool HasExternalEndpoints { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
    }

    public enum ServiceType
    {
        WebApi,
        Worker,
        BlazorServer,
        BlazorWasm,
        Grpc
    }

    public class AspireResourceModel
    {
        public string Name { get; set; }
        public ResourceType Type { get; set; }
        public Dictionary<string, object> Configuration { get; set; } = new();
        public bool WithDataVolume { get; set; }
        public List<string> Plugins { get; set; } = new();
    }

    public enum ResourceType
    {
        SqlServer,
        PostgreSql,
        MySql,
        Redis,
        RabbitMq,
        Kafka,
        MongoDB,
        CosmosDb,
        AzureStorage,
        AzureServiceBus,
        AzureKeyVault
    }

    public class AspireAppHostGenerator : IAspireAppHostGenerator
    {
        public AspireProjectModel GenerateAppHost(AspireSolutionModel solution)
        {
            var project = new AspireProjectModel
            {
                Name = $"{solution.SolutionName}.AppHost",
                ProjectType = DotNetProjectType.AspireAppHost
            };

            // Generate Program.cs
            project.Files.Add(GenerateProgramCs(solution));

            // Generate appsettings.json
            project.Files.Add(GenerateAppSettings(solution));

            // Generate launchSettings.json
            project.Files.Add(GenerateLaunchSettings(solution));

            return project;
        }

        private FileModel GenerateProgramCs(AspireSolutionModel solution)
        {
            var sb = new StringBuilder();

            sb.AppendLine("var builder = DistributedApplication.CreateBuilder(args);");
            sb.AppendLine();

            // Add resources
            sb.AppendLine("// Infrastructure Resources");
            foreach (var resource in solution.Resources)
            {
                sb.AppendLine(GenerateResourceCode(resource));
            }

            sb.AppendLine();

            // Add services
            sb.AppendLine("// Application Services");
            foreach (var service in solution.Services)
            {
                sb.AppendLine(GenerateServiceCode(service, solution.Resources));
            }

            // Add frontend if present
            if (solution.Frontend != null)
            {
                sb.AppendLine();
                sb.AppendLine("// Frontend");
                sb.AppendLine(GenerateFrontendCode(solution.Frontend, solution.Services));
            }

            sb.AppendLine();
            sb.AppendLine("builder.Build().Run();");

            return new FileModel
            {
                Name = "Program.cs",
                Content = sb.ToString()
            };
        }

        private string GenerateResourceCode(AspireResourceModel resource)
        {
            var sb = new StringBuilder();
            var varName = resource.Name.ToCamelCase();

            sb.Append($"var {varName} = builder.");

            // Resource type-specific generation
            switch (resource.Type)
            {
                case ResourceType.SqlServer:
                    sb.Append($"AddSqlServer(\"{resource.Name}\")");
                    if (resource.WithDataVolume) sb.Append(".WithDataVolume()");
                    if (resource.Configuration.ContainsKey("database"))
                        sb.Append($".AddDatabase(\"{resource.Configuration["database"]}\")");
                    break;

                case ResourceType.Redis:
                    sb.Append($"AddRedis(\"{resource.Name}\")");
                    if (resource.WithDataVolume) sb.Append(".WithDataVolume()");
                    break;

                case ResourceType.RabbitMq:
                    sb.Append($"AddRabbitMQ(\"{resource.Name}\")");
                    if (resource.WithDataVolume) sb.Append(".WithDataVolume()");
                    if (resource.Plugins.Contains("management"))
                        sb.Append(".WithManagementPlugin()");
                    break;

                case ResourceType.PostgreSql:
                    sb.Append($"AddPostgres(\"{resource.Name}\")");
                    if (resource.WithDataVolume) sb.Append(".WithDataVolume()");
                    if (resource.Configuration.ContainsKey("database"))
                        sb.Append($".AddDatabase(\"{resource.Configuration["database"]}\")");
                    break;
            }

            sb.Append(";");
            return sb.ToString();
        }
    }
}
```

## ServiceDefaults Generation

### Extensions.cs

```csharp
// ECommerce.ServiceDefaults/Extensions.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return app;
    }
}
```

### ServiceDefaults Generator

```csharp
namespace Endpoint.Aspire.Generation
{
    public interface IServiceDefaultsGenerator
    {
        AspireProjectModel GenerateServiceDefaults(AspireSolutionModel solution);
    }

    public class ServiceDefaultsGenerator : IServiceDefaultsGenerator
    {
        public AspireProjectModel GenerateServiceDefaults(AspireSolutionModel solution)
        {
            var project = new AspireProjectModel
            {
                Name = $"{solution.SolutionName}.ServiceDefaults",
                ProjectType = DotNetProjectType.AspireServiceDefaults,
                Packages = new List<PackageModel>
                {
                    new() { Name = "Microsoft.Extensions.Http.Resilience" },
                    new() { Name = "Microsoft.Extensions.ServiceDiscovery" },
                    new() { Name = "OpenTelemetry.Exporter.OpenTelemetryProtocol" },
                    new() { Name = "OpenTelemetry.Extensions.Hosting" },
                    new() { Name = "OpenTelemetry.Instrumentation.AspNetCore" },
                    new() { Name = "OpenTelemetry.Instrumentation.Http" },
                    new() { Name = "OpenTelemetry.Instrumentation.Runtime" }
                }
            };

            // Generate Extensions.cs
            project.Files.Add(GenerateExtensions(solution));

            return project;
        }

        private FileModel GenerateExtensions(AspireSolutionModel solution)
        {
            var template = LoadTemplate("ServiceDefaultsExtensions.liquid");
            var content = _templateProcessor.Process(template, new
            {
                SolutionName = solution.SolutionName,
                IncludeEfCore = solution.Services.Any(s => s.ResourceReferences.Any(r =>
                    r.Contains("sql", StringComparison.OrdinalIgnoreCase) ||
                    r.Contains("postgres", StringComparison.OrdinalIgnoreCase))),
                IncludeRedis = solution.Resources.Any(r => r.Type == ResourceType.Redis),
                IncludeMessaging = solution.Resources.Any(r =>
                    r.Type == ResourceType.RabbitMq || r.Type == ResourceType.Kafka)
            });

            return new FileModel
            {
                Name = "Extensions.cs",
                Content = content
            };
        }
    }
}
```

## API Service Integration

### Program.cs for API with Aspire

```csharp
// ECommerce.Api/Program.cs
using ECommerce.Application;
using ECommerce.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire ServiceDefaults
builder.AddServiceDefaults();

// Add Application Layer
builder.Services.AddApplication();

// Add Infrastructure Layer with Aspire connections
builder.AddSqlServerDbContext<ApplicationDbContext>("ecommerce-db");
builder.AddRedisDistributedCache("redis");

// Add API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Map Aspire endpoints (health checks)
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## Resource Configuration Models

```csharp
namespace Endpoint.Aspire.Models
{
    public class DatabaseResourceConfiguration
    {
        public ResourceType Type { get; set; } = ResourceType.SqlServer;
        public string Name { get; set; } = "sql";
        public string DatabaseName { get; set; }
        public bool WithDataVolume { get; set; } = true;
        public bool UseAzure { get; set; }
        public string AzureResourceGroup { get; set; }
    }

    public class CacheResourceConfiguration
    {
        public ResourceType Type { get; set; } = ResourceType.Redis;
        public string Name { get; set; } = "redis";
        public bool WithDataVolume { get; set; } = true;
        public bool UseAzure { get; set; }
    }

    public class MessagingResourceConfiguration
    {
        public ResourceType Type { get; set; } = ResourceType.RabbitMq;
        public string Name { get; set; } = "messaging";
        public bool WithDataVolume { get; set; } = true;
        public bool WithManagementPlugin { get; set; } = true;
        public bool UseAzure { get; set; }
    }

    public class ObservabilityConfiguration
    {
        public bool EnableOpenTelemetry { get; set; } = true;
        public bool EnableStructuredLogging { get; set; } = true;
        public bool EnableHealthChecks { get; set; } = true;
        public bool EnableMetrics { get; set; } = true;
        public bool EnableTracing { get; set; } = true;
        public string OtlpEndpoint { get; set; }
    }
}
```

## Cloud Resource Integration

### Azure Resources

```csharp
// For Azure deployment
var builder = DistributedApplication.CreateBuilder(args);

// Azure SQL Database
var sqlServer = builder.AddAzureSqlServer("sql")
    .AddDatabase("ecommerce-db");

// Azure Cache for Redis
var redis = builder.AddAzureRedis("redis");

// Azure Service Bus
var serviceBus = builder.AddAzureServiceBus("messaging")
    .AddQueue("orders")
    .AddTopic("events");

// Azure Storage
var storage = builder.AddAzureStorage("storage")
    .AddBlobs("blobs")
    .AddQueues("queues");

// Azure Key Vault
var keyVault = builder.AddAzureKeyVault("keyvault");

var api = builder.AddProject<Projects.ECommerce_Api>("api")
    .WithReference(sqlServer)
    .WithReference(redis)
    .WithReference(serviceBus)
    .WithReference(keyVault);
```

## Aspire Dashboard Integration

```csharp
namespace Endpoint.Aspire.Generation
{
    public class AspireDashboardConfiguration
    {
        public bool Enabled { get; set; } = true;
        public int Port { get; set; } = 18888;
        public bool EnableAuthentication { get; set; }
        public string AuthenticationType { get; set; } = "BrowserToken";
    }
}
```

## Command Integration

```csharp
[Verb("aspire-solution-from-plantuml", HelpText = "Generate .NET Aspire solution from PlantUML")]
public class AspireSolutionFromPlantUmlRequest : IRequest
{
    [Option('f', "file", Required = true, HelpText = "Path to PlantUML file")]
    public string FilePath { get; set; }

    [Option('n', "name", Required = true, HelpText = "Solution name")]
    public string SolutionName { get; set; }

    [Option('o', "output", HelpText = "Output directory")]
    public string OutputDirectory { get; set; } = Environment.CurrentDirectory;

    [Option("database", Default = "SqlServer", HelpText = "Database provider")]
    public string DatabaseProvider { get; set; }

    [Option("cache", Default = "Redis", HelpText = "Cache provider")]
    public string CacheProvider { get; set; }

    [Option("messaging", Default = "RabbitMq", HelpText = "Messaging provider")]
    public string MessagingProvider { get; set; }

    [Option("azure", HelpText = "Use Azure resources")]
    public bool UseAzure { get; set; }

    [Option("angular", HelpText = "Include Angular frontend")]
    public bool IncludeAngular { get; set; }

    [Option("worker", HelpText = "Include background worker")]
    public bool IncludeWorker { get; set; }
}

public class AspireSolutionFromPlantUmlRequestHandler : IRequestHandler<AspireSolutionFromPlantUmlRequest>
{
    private readonly IPlantUmlParser _parser;
    private readonly ISemanticAnalyzer _analyzer;
    private readonly IAspireSolutionFactory _solutionFactory;
    private readonly IArtifactGenerator _generator;

    public async Task Handle(AspireSolutionFromPlantUmlRequest request, CancellationToken ct)
    {
        // 1. Parse PlantUML
        var document = _parser.ParseFile(request.FilePath);
        var semanticModel = _analyzer.Analyze(document);

        // 2. Build Aspire solution model
        var aspireModel = _solutionFactory.CreateAspireSolution(semanticModel, new AspireOptions
        {
            SolutionName = request.SolutionName,
            DatabaseProvider = Enum.Parse<ResourceType>(request.DatabaseProvider),
            CacheProvider = request.CacheProvider == "None" ? null : ResourceType.Redis,
            MessagingProvider = request.MessagingProvider == "None" ? null : ResourceType.RabbitMq,
            UseAzureResources = request.UseAzure,
            IncludeAngularFrontend = request.IncludeAngular,
            IncludeBackgroundWorker = request.IncludeWorker
        });

        // 3. Generate solution
        await _generator.GenerateAsync(aspireModel);
    }
}
```

## Generated Project References

```xml
<!-- ECommerce.Api.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\ECommerce.ServiceDefaults\ECommerce.ServiceDefaults.csproj" />
    <ProjectReference Include="..\ECommerce.Application\ECommerce.Application.csproj" />
    <ProjectReference Include="..\ECommerce.Infrastructure\ECommerce.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Microsoft.EntityFrameworkCore.SqlServer" />
    <PackageReference Include="Aspire.StackExchange.Redis.DistributedCaching" />
  </ItemGroup>
</Project>
```

```xml
<!-- ECommerce.AppHost.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <Sdk Name="Aspire.AppHost.Sdk" Version="9.0.0" />

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <IsAspireHost>true</IsAspireHost>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\ECommerce.Api\ECommerce.Api.csproj" />
    <ProjectReference Include="..\ECommerce.Worker\ECommerce.Worker.csproj" />
  </ItemGroup>
</Project>
```

## Deployment Manifests

### Aspire Manifest Generation

```csharp
public interface IManifestGenerator
{
    Task<string> GenerateManifestAsync(AspireSolutionModel solution);
}

// Generated aspire-manifest.json
{
  "resources": {
    "sql": {
      "type": "container.v0",
      "image": "mcr.microsoft.com/mssql/server:2022-latest",
      "env": {
        "ACCEPT_EULA": "Y",
        "MSSQL_SA_PASSWORD": "{sql.inputs.password}"
      },
      "bindings": {
        "tcp": {
          "scheme": "tcp",
          "protocol": "tcp",
          "transport": "tcp",
          "targetPort": 1433
        }
      }
    },
    "api": {
      "type": "project.v0",
      "path": "../ECommerce.Api/ECommerce.Api.csproj",
      "env": {
        "ConnectionStrings__ecommerce-db": "{sql.connectionString}"
      },
      "bindings": {
        "http": {
          "scheme": "http",
          "protocol": "tcp",
          "transport": "http"
        },
        "https": {
          "scheme": "https",
          "protocol": "tcp",
          "transport": "http"
        }
      }
    }
  }
}
```
