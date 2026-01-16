# class-create

Create a C# class file with optional properties and interface implementations.

## Synopsis

```bash
endpoint class-create [options]
```

## Description

The `class-create` command generates a new C# class file in the specified directory. You can define properties and interfaces to implement directly from the command line.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the class to create | No | - |
| `--properties` | `-p` | Properties to add (format: `Name:Type,Name:Type`) | No | - |
| `--implements` | `-i` | Interfaces to implement (comma-separated) | No | - |
| `--directory` | `-d` | Target directory for the generated file | No | Current directory |

## Property Format

Properties are specified as comma-separated key-value pairs:

```
PropertyName:PropertyType
```

## Examples

### Create a simple class

```bash
endpoint class-create -n Customer
```

**Output: `Customer.cs`**
```csharp
public class Customer
{
}
```

### Create a class with properties

```bash
endpoint class-create -n Customer -p "Id:int,Name:string,Email:string,CreatedAt:DateTime"
```

**Output: `Customer.cs`**
```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Create a class implementing interfaces

```bash
endpoint class-create -n CustomerService -i "ICustomerService,IDisposable"
```

**Output: `CustomerService.cs`**
```csharp
public class CustomerService : ICustomerService, IDisposable
{
}
```

### Create a class with properties and interfaces

```bash
endpoint class-create -n OrderEntity -p "OrderId:Guid,Total:decimal,Status:string" -i "IEntity"
```

### Specify output directory

```bash
endpoint class-create -n Product -p "Name:string,Price:decimal" -d ./src/Models
```

## Common Use Cases

1. **Quick Model Creation**: Create data models with properties
2. **Service Classes**: Create service classes implementing interfaces
3. **Entity Classes**: Create database entities with properties
4. **DTOs**: Create Data Transfer Objects

## Related Commands

- [interface-create](./interface-create.user-guide.md) - Create interfaces
- [entity-create](./entity-create.user-guide.md) - Create entities with DTOs
- [record-create](./record-create.user-guide.md) - Create C# records

[Back to Code Generation](./index.md) | [Back to Index](../index.md)
