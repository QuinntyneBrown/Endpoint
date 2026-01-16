# mwa-create

Create a Modern Web Application with full-stack scaffolding.

## Synopsis

```bash
endpoint mwa-create [options]
```

## Description

The `mwa-create` command generates a complete Modern Web Application solution including backend API, domain models, data access layer, and infrastructure. It uses an interactive JSON editor to customize the application structure before generation.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the application | No | - |
| `--path` | `-p` | Path to JSON configuration file | No | - |
| `--directory` | `-d` | Target directory | No | Current directory |

## Examples

### Create with interactive editor

```bash
endpoint mwa-create -n MyWebApp
```

This opens an interactive JSON editor with a default template:

```json
{
  "productName": "MyWebApp",
  "boundedContexts": [
    {
      "name": "ToDos",
      "aggregates": [
        {
          "name": "ToDo",
          "properties": "ToDoId:Guid,Title:string,IsComplete:bool"
        }
      ]
    }
  ],
  "microservices": [
    {
      "name": "MyWebApp.ToDos.Api",
      "kind": "Api"
    }
  ]
}
```

### Create from configuration file

```bash
endpoint mwa-create -n MyApp -p ./config/app-definition.json
```

### Specify output directory

```bash
endpoint mwa-create -n ECommerce -d ./projects
```

## Generated Structure

```
MyWebApp/
├── MyWebApp.sln
├── endpoint.json                    # Configuration snapshot
├── src/
│   ├── MyWebApp.Core/              # Domain models
│   │   ├── AggregateModels/
│   │   │   └── ToDoAggregate/
│   │   │       ├── ToDo.cs
│   │   │       ├── Commands/
│   │   │       └── Queries/
│   │   └── MyWebApp.Core.csproj
│   ├── MyWebApp.Infrastructure/    # Data access
│   │   ├── Data/
│   │   │   └── MyWebAppDbContext.cs
│   │   └── MyWebApp.Infrastructure.csproj
│   └── MyWebApp.Api/               # Web API
│       ├── Controllers/
│       │   └── ToDosController.cs
│       ├── Program.cs
│       └── MyWebApp.Api.csproj
└── tests/
    └── MyWebApp.Tests/
```

## Configuration Schema

```json
{
  "productName": "string",
  "boundedContexts": [
    {
      "name": "string",
      "aggregates": [
        {
          "name": "string",
          "properties": "Property:Type,Property:Type",
          "commands": ["Create", "Update", "Delete"],
          "queries": ["GetAll", "GetById"]
        }
      ]
    }
  ],
  "microservices": [
    {
      "name": "string",
      "kind": "Api | Worker | Gateway"
    }
  ],
  "messages": []
}
```

## Features Generated

- **Domain Layer**: Entities, aggregates, value objects
- **Application Layer**: Commands, queries, handlers (CQRS)
- **Infrastructure**: DbContext, repositories, migrations
- **API Layer**: Controllers with CRUD endpoints
- **Validation**: FluentValidation validators
- **Mapping**: Extension methods for entity-DTO mapping

## Common Use Cases

1. **Full-Stack Applications**: Complete backend scaffolding
2. **Microservices**: Individual service creation
3. **Rapid Prototyping**: Quick application setup
4. **DDD Applications**: Domain-Driven Design implementations

## Best Practices

- Define your domain model before running
- Use meaningful bounded context names
- Start with a simple model and iterate
- Review and customize generated code

## Related Commands

- [ddd-app-create](./ddd-app-create.user-guide.md) - Simpler DDD application
- [aggregate-create](../code-generation/aggregate-create.user-guide.md) - Add aggregates
- [solution-create](../project-management/solution-create.user-guide.md) - Basic solution

[Back to Application Scaffolding](./index.md) | [Back to Index](../index.md)
