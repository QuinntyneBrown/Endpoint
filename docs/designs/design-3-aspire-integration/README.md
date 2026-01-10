# Design 3: Aspire Integration - Cloud-Native Orchestration

## Overview

This design extends the microservices generation to include .NET Aspire orchestration. Aspire provides a cohesive, opinionated stack for building cloud-native applications with built-in service discovery, configuration management, and observability.

## Goals

1. Generate .NET Aspire AppHost for service orchestration
2. Configure service discovery and communication
3. Add observability (OpenTelemetry, dashboards)
4. Support containerized infrastructure (Redis, PostgreSQL, RabbitMQ)
5. Generate environment-specific configurations

## Key Aspire Concepts

| Concept | Description |
|---------|-------------|
| AppHost | Orchestrator project that manages all services |
| ServiceDefaults | Shared defaults for telemetry, health checks |
| Service Discovery | Automatic endpoint resolution between services |
| Resource | External dependencies (databases, caches, message brokers) |

## PlantUML Input Format

```plantuml
@startuml Aspire Microservices
!define MICROSERVICE(name) package name <<microservice>>
!define APPHOST(name) package name <<apphost>>
!define RESOURCE(name) database name <<resource>>
!define CACHE(name) database name <<cache>>
!define MESSAGEBUS(name) queue name <<messagebus>>

' Aspire Configuration
APPHOST(MyApp.AppHost) {
    note "Orchestrates all services\nand resources" as N1
}

' Infrastructure Resources
RESOURCE(PostgreSQL) <<postgres>> {
    database OrdersDb
    database InventoryDb
    database CustomersDb
}

CACHE(Redis) <<redis>> {
    database DistributedCache
    database SessionStore
}

MESSAGEBUS(RabbitMQ) <<rabbitmq>> {
}

' Microservices
MICROSERVICE(OrderService) {
    class Order <<aggregate>>
    note "Depends on:\n- OrdersDb\n- Redis\n- RabbitMQ" as OrderDeps
}

MICROSERVICE(InventoryService) {
    class Product <<aggregate>>
    note "Depends on:\n- InventoryDb\n- Redis\n- RabbitMQ" as InvDeps
}

MICROSERVICE(ApiGateway) <<gateway>> {
    note "Routes to all services\nYARP-based" as GatewayNote
}

' Dependencies
OrderService --> PostgreSQL : uses
OrderService --> Redis : uses
OrderService --> RabbitMQ : publishes/subscribes
InventoryService --> PostgreSQL : uses
InventoryService --> Redis : uses
InventoryService --> RabbitMQ : publishes/subscribes
ApiGateway --> OrderService : routes
ApiGateway --> InventoryService : routes
@enduml
```

## Generated Solution Structure

```
MySolution/
├── src/
│   ├── MySolution.AppHost/                    # Aspire Orchestrator
│   │   ├── Program.cs
│   │   ├── MySolution.AppHost.csproj
│   │   └── appsettings.json
│   ├── MySolution.ServiceDefaults/            # Shared Defaults
│   │   ├── Extensions.cs
│   │   └── MySolution.ServiceDefaults.csproj
│   ├── BuildingBlocks/
│   │   ├── Messaging/
│   │   └── ...
│   └── Services/
│       ├── OrderService/
│       │   ├── OrderService.Domain/
│       │   ├── OrderService.Application/
│       │   ├── OrderService.Infrastructure/
│       │   └── OrderService.Api/
│       │       ├── Program.cs              # Uses ServiceDefaults
│       │       └── appsettings.json
│       └── InventoryService/
│           └── ...
├── tests/
└── MySolution.sln
```

## Generated Aspire AppHost

```csharp
// MySolution.AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure Resources
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume();

var ordersDb = postgres.AddDatabase("ordersdb");
var inventoryDb = postgres.AddDatabase("inventorydb");

var redis = builder.AddRedis("redis")
    .WithRedisCommander()
    .WithDataVolume();

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume();

// Microservices
var orderService = builder.AddProject<Projects.OrderService_Api>("orderservice")
    .WithReference(ordersDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithExternalHttpEndpoints();

var inventoryService = builder.AddProject<Projects.InventoryService_Api>("inventoryservice")
    .WithReference(inventoryDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithExternalHttpEndpoints();

// API Gateway
var gateway = builder.AddProject<Projects.ApiGateway>("gateway")
    .WithReference(orderService)
    .WithReference(inventoryService)
    .WithExternalHttpEndpoints();

builder.Build().Run();
```

## Generated Service Defaults

```csharp
// MySolution.ServiceDefaults/Extensions.cs
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

## CLI Command

```bash
endpoint aspire-microservices-create-from-plantuml \
    --file ./microservices.puml \
    --name MySolution \
    --directory ./output \
    --aspire-version 9.0 \
    --include-dashboard true \
    --container-registry myregistry.azurecr.io
```

### Command Options

| Option | Description | Default |
|--------|-------------|---------|
| `--file` | Path to PlantUML file | Required |
| `--name` | Solution name | Required |
| `--directory` | Output directory | Current dir |
| `--aspire-version` | Target Aspire version | Latest |
| `--include-dashboard` | Include Aspire dashboard | true |
| `--container-registry` | Container registry for deployment | None |
| `--include-tests` | Generate integration tests | true |

## Aspire Resource Mapping

| PlantUML Stereotype | Aspire Resource |
|---------------------|-----------------|
| `<<postgres>>` | `AddPostgres()` |
| `<<sqlserver>>` | `AddSqlServer()` |
| `<<mysql>>` | `AddMySql()` |
| `<<mongodb>>` | `AddMongoDB()` |
| `<<redis>>` | `AddRedis()` |
| `<<rabbitmq>>` | `AddRabbitMQ()` |
| `<<kafka>>` | `AddKafka()` |
| `<<azureservicebus>>` | `AddAzureServiceBus()` |
| `<<azurestorage>>` | `AddAzureStorage()` |
| `<<gateway>>` | YARP-based gateway project |

## Implementation Components

### 1. Resource Parsing Strategy

```csharp
public class AspireResourcePlantUmlParsingStrategy : IArtifactParsingStrategy<AspireResourceModel>
{
    private static readonly Dictionary<string, AspireResourceType> ResourceTypes = new()
    {
        ["<<postgres>>"] = AspireResourceType.PostgreSQL,
        ["<<redis>>"] = AspireResourceType.Redis,
        ["<<rabbitmq>>"] = AspireResourceType.RabbitMQ,
        ["<<mongodb>>"] = AspireResourceType.MongoDB,
        ["<<sqlserver>>"] = AspireResourceType.SqlServer,
    };

    public async Task<AspireResourceModel> ParseAsync(IArtifactParser parser, string value)
    {
        // Parse resource declarations and map to Aspire types
    }
}
```

### 2. AppHost Generation Strategy

```csharp
public class AppHostGenerationStrategy : IArtifactGenerationStrategy<AppHostModel>
{
    public async Task GenerateAsync(AppHostModel model, GenerationContext context)
    {
        // Generate Program.cs with all resource and service references
        // Generate csproj with Aspire dependencies
        // Generate appsettings.json
    }
}
```

### 3. Service Defaults Generation

```csharp
public class ServiceDefaultsGenerationStrategy : IArtifactGenerationStrategy<ServiceDefaultsModel>
{
    public async Task GenerateAsync(ServiceDefaultsModel model, GenerationContext context)
    {
        // Generate Extensions.cs with OpenTelemetry, health checks
        // Generate csproj with proper package references
    }
}
```

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           Aspire Dashboard                               │
│                     (Observability & Monitoring)                         │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    │ Metrics/Traces/Logs
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                              AppHost                                     │
│                    (Orchestration & Configuration)                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐            │
│  │  API Gateway   │  │ Order Service  │  │Inventory Svc   │            │
│  │   (YARP)       │──│    (API)       │──│    (API)       │            │
│  └────────────────┘  └───────┬────────┘  └───────┬────────┘            │
│                              │                    │                      │
│              ┌───────────────┴────────────────────┘                      │
│              │                                                           │
│              ▼                                                           │
│  ┌────────────────────────────────────────────────────────────────┐     │
│  │                    ServiceDefaults                              │     │
│  │  (OpenTelemetry, HealthChecks, ServiceDiscovery, Resilience)   │     │
│  └────────────────────────────────────────────────────────────────┘     │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
            ┌───────────────────────┼───────────────────────┐
            │                       │                       │
            ▼                       ▼                       ▼
    ┌───────────────┐       ┌───────────────┐       ┌───────────────┐
    │   PostgreSQL  │       │     Redis     │       │   RabbitMQ    │
    │  (Databases)  │       │    (Cache)    │       │  (Messaging)  │
    └───────────────┘       └───────────────┘       └───────────────┘
```

## Deployment Support

### Development (Local)

```bash
# Run with Aspire dashboard
dotnet run --project src/MySolution.AppHost
```

### Production (Azure Container Apps)

```bash
# Deploy to Azure
azd init
azd provision
azd deploy
```

### Generated Azure Infrastructure

```bicep
// infra/main.bicep (generated)
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: 'cae-${resourceToken}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
    }
  }
}

resource orderService 'Microsoft.App/containerApps@2023-05-01' = {
  name: 'orderservice'
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: {
        external: false
        targetPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: 'orderservice'
          image: '${containerRegistry}/orderservice:latest'
        }
      ]
    }
  }
}
```

## Pros and Cons

### Pros
- Unified development experience
- Built-in observability (OpenTelemetry)
- Service discovery out of the box
- Local development with production parity
- Easy Azure deployment with AZD
- Dashboard for debugging

### Cons
- .NET 8+ required
- Aspire is relatively new (evolving API)
- Opinionated stack may not fit all scenarios
- Container runtime required for resources
- Learning curve for Aspire concepts

## Implementation Effort

| Component | Estimated Complexity |
|-----------|---------------------|
| Resource Parsing | Medium |
| AppHost Generation | Medium |
| ServiceDefaults Generation | Low |
| Service Discovery Integration | Medium |
| Deployment Scripts | Medium |
| Dashboard Integration | Low |

## See Also

- [Architecture Diagram](./architecture.puml)
- [Service Topology Diagram](./service-topology.drawio)
- [Example Input](./example-input.puml)
