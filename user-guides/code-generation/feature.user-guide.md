# feature

Create a complete CRUD feature with commands, queries, DTOs, validators, and extensions.

## Synopsis

```bash
endpoint feature <entity-name> [options]
```

## Description

The `feature` command is a powerful scaffolding tool that generates a complete CRUD (Create, Read, Update, Delete) feature for an entity. It creates multiple files including commands, queries, DTOs, validators, and extension methods, following CQRS (Command Query Responsibility Segregation) patterns.

## Arguments

| Argument | Description |
|----------|-------------|
| `entity-name` | The name of the entity to create the feature for |

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--directory` | `-d` | Target directory for the generated files | No | Current directory |

## Examples

### Create a Product feature

```bash
endpoint feature Product
```

This generates:
- `CreateProductCommand.cs`
- `UpdateProductCommand.cs`
- `DeleteProductCommand.cs`
- `GetProductsQuery.cs`
- `GetProductByIdQuery.cs`
- `ProductDto.cs`
- `ProductValidator.cs`
- `ProductExtensions.cs`

### Create feature in a specific directory

```bash
endpoint feature Order -d ./Features
```

If the directory ends with "Features", a subdirectory named after the plural entity will be created:
- `./Features/Orders/CreateOrderCommand.cs`
- `./Features/Orders/UpdateOrderCommand.cs`
- etc.

## Generated Files

For an entity named `Product`, the following files are generated:

### Commands

| File | Description |
|------|-------------|
| `CreateProductCommand.cs` | Command to create a new product |
| `UpdateProductCommand.cs` | Command to update an existing product |
| `DeleteProductCommand.cs` | Command to delete a product |

### Queries

| File | Description |
|------|-------------|
| `GetProductsQuery.cs` | Query to retrieve all products |
| `GetProductByIdQuery.cs` | Query to retrieve a product by ID |

### Supporting Files

| File | Description |
|------|-------------|
| `ProductDto.cs` | Data Transfer Object for the product |
| `ProductValidator.cs` | FluentValidation validator |
| `ProductExtensions.cs` | Extension methods (e.g., mapping) |

## Directory Structure

When using `-d ./Features`:

```
Features/
└── Products/
    ├── Commands/
    │   ├── CreateProductCommand.cs
    │   ├── UpdateProductCommand.cs
    │   └── DeleteProductCommand.cs
    ├── Queries/
    │   ├── GetProductsQuery.cs
    │   └── GetProductByIdQuery.cs
    ├── ProductDto.cs
    ├── ProductValidator.cs
    └── ProductExtensions.cs
```

## Common Use Cases

1. **Rapid Prototyping**: Quickly scaffold CRUD operations
2. **Consistent Structure**: Ensure all features follow the same pattern
3. **CQRS Implementation**: Generate command/query separation
4. **API Development**: Create all necessary files for REST endpoints

## Generated Code Patterns

### Command Example

```csharp
public class CreateProductRequest : IRequest<ProductDto>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class CreateProductRequestHandler : IRequestHandler<CreateProductRequest, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Query Example

```csharp
public class GetProductsRequest : IRequest<IEnumerable<ProductDto>>
{
}

public class GetProductsRequestHandler : IRequestHandler<GetProductsRequest, IEnumerable<ProductDto>>
{
    public async Task<IEnumerable<ProductDto>> Handle(GetProductsRequest request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

## Best Practices

1. **Feature Folders**: Organize by feature, not by type
2. **Single Responsibility**: Each command/query does one thing
3. **Validation**: Always validate incoming commands
4. **Mapping**: Use extension methods for entity-to-DTO mapping

## Related Commands

- [entity-create](./entity-create.user-guide.md) - Create just the entity
- [controller-create](./controller-create.user-guide.md) - Create API controllers
- [aggregate-create](./aggregate-create.user-guide.md) - Create DDD aggregates

[Back to Code Generation](./index.md) | [Back to Index](../index.md)
