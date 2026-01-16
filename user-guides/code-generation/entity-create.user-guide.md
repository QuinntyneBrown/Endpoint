# entity-create

Create a database entity class along with its corresponding DTO (Data Transfer Object).

## Synopsis

```bash
endpoint entity-create [options]
```

## Description

The `entity-create` command generates both an entity class and a DTO class based on the specified name and properties. This is useful for creating domain entities that map to database tables along with their transfer objects.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the entity to create | No | - |
| `--properties` | `-p` | Properties to add (format: `Name:Type,Name:Type`) | No | - |
| `--directory` | `-d` | Target directory for the generated files | No | Current directory |

## Property Format

Properties are specified as comma-separated key-value pairs:

```
PropertyName:PropertyType
```

## Examples

### Create a simple entity

```bash
endpoint entity-create -n Product -p "Name:string,Price:decimal,Description:string"
```

**Output: `Product.cs`**
```csharp
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}
```

**Output: `ProductDto.cs`**
```csharp
public class ProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}
```

### Create an entity with various property types

```bash
endpoint entity-create -n Customer -p "CustomerId:Guid,Name:string,Email:string,CreatedAt:DateTime,IsActive:bool"
```

### Specify output directory

```bash
endpoint entity-create -n Order -p "OrderId:int,Total:decimal,Status:string" -d ./src/Domain/Entities
```

## Generated Files

For an entity named `Product`, the command generates:

1. `Product.cs` - The entity class
2. `ProductDto.cs` - The Data Transfer Object

## Common Property Types

| Type | Description |
|------|-------------|
| `string` | Text data |
| `int` | 32-bit integer |
| `long` | 64-bit integer |
| `decimal` | Decimal numbers (money, precise calculations) |
| `double` | Double-precision floating point |
| `bool` | Boolean (true/false) |
| `DateTime` | Date and time |
| `Guid` | Globally unique identifier |
| `byte[]` | Binary data |

## Common Use Cases

1. **Database Modeling**: Create entities for Entity Framework
2. **API Development**: Create entities with DTOs for API responses
3. **Domain Modeling**: Create domain entities for DDD applications

## Related Commands

- [class-create](./class-create.user-guide.md) - Create a simple class
- [aggregate-create](./aggregate-create.user-guide.md) - Create DDD aggregates
- [feature](./feature.user-guide.md) - Create complete CRUD features

[Back to Code Generation](./index.md) | [Back to Index](../index.md)
