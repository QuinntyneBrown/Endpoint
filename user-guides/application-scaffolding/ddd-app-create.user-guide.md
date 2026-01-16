# ddd-app-create

Create a Domain-Driven Design application with a single aggregate.

## Synopsis

```bash
endpoint ddd-app-create [options]
```

## Description

The `ddd-app-create` command generates a complete DDD (Domain-Driven Design) application with a single aggregate. This is a streamlined alternative to `mwa-create` for simpler applications. The command creates a full solution with domain, infrastructure, and API layers.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--product-name` | `-n` | Name of the product/application | No | - |
| `--bounded-context` | `-b` | Name of the bounded context | No | `ToDos` |
| `--aggregate` | `-a` | Name of the aggregate | No | `ToDo` |
| `--properties` | `-p` | Properties for the aggregate | No | `ToDoId:Guid,Title:String,IsComplete:String` |
| `--directory` | `-d` | Target directory | No | Current directory |

## Property Format

Properties are specified as comma-separated key-value pairs:

```
PropertyName:PropertyType
```

If the aggregate ID property is not included, it's automatically added.

## Examples

### Create a default ToDo application

```bash
endpoint ddd-app-create -n MyTaskApp
```

### Create a custom aggregate application

```bash
endpoint ddd-app-create -n ECommerce -a Product -b Products -p "Name:string,Price:decimal,Description:string,Stock:int"
```

### Create a customer management application

```bash
endpoint ddd-app-create -n CRM -a Customer -b Customers -p "Name:string,Email:string,Phone:string,CreatedAt:DateTime"
```

### Specify output directory

```bash
endpoint ddd-app-create -n OrderSystem -a Order -d ./projects
```

## Generated Structure

For `endpoint ddd-app-create -n MyTaskApp -a ToDo -b ToDos`:

```
MyTaskApp/
├── MyTaskApp.sln
├── src/
│   ├── MyTaskApp.Core/
│   │   ├── AggregateModels/
│   │   │   └── ToDoAggregate/
│   │   │       ├── ToDo.cs
│   │   │       ├── ToDoDto.cs
│   │   │       ├── Commands/
│   │   │       │   ├── CreateToDoCommand.cs
│   │   │       │   ├── UpdateToDoCommand.cs
│   │   │       │   └── DeleteToDoCommand.cs
│   │   │       └── Queries/
│   │   │           ├── GetToDosQuery.cs
│   │   │           └── GetToDoByIdQuery.cs
│   │   └── MyTaskApp.Core.csproj
│   ├── MyTaskApp.Infrastructure/
│   │   ├── Data/
│   │   │   └── MyTaskAppDbContext.cs
│   │   └── MyTaskApp.Infrastructure.csproj
│   └── MyTaskApp.ToDos.Api/
│       ├── Controllers/
│       │   └── ToDosController.cs
│       ├── Program.cs
│       └── MyTaskApp.ToDos.Api.csproj
└── tests/
```

## Generated API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/todos` | Get all items |
| GET | `/api/todos/{id}` | Get item by ID |
| POST | `/api/todos` | Create new item |
| PUT | `/api/todos` | Update item |
| DELETE | `/api/todos/{id}` | Delete item |

## Common Property Types

| Type | C# Type |
|------|---------|
| `string` | `string` |
| `int` | `int` |
| `long` | `long` |
| `decimal` | `decimal` |
| `double` | `double` |
| `bool` | `bool` |
| `DateTime` | `DateTime` |
| `Guid` | `Guid` |

## Automatic Features

The command automatically:
1. Adds the aggregate ID property if not specified
2. Creates proper project references
3. Sets up Entity Framework DbContext
4. Configures dependency injection
5. Opens VS Code in the solution directory

## Common Use Cases

1. **Quick Prototypes**: Rapidly create working applications
2. **Learning DDD**: Understand DDD project structure
3. **Microservices**: Create individual bounded contexts
4. **CRUD Applications**: Standard data management apps

## Best Practices

- Use singular names for aggregates (e.g., `Product` not `Products`)
- Use plural names for bounded contexts (e.g., `Products` not `Product`)
- Include ID property with consistent naming (`{AggregateName}Id`)
- Use appropriate property types for your domain

## Related Commands

- [mwa-create](./mwa-create.user-guide.md) - More complex applications
- [aggregate-create](../code-generation/aggregate-create.user-guide.md) - Add more aggregates
- [solution-create](../project-management/solution-create.user-guide.md) - Basic solutions

[Back to Application Scaffolding](./index.md) | [Back to Index](../index.md)
