# interface-create

Create a C# interface file with optional base interfaces.

## Synopsis

```bash
endpoint interface-create [options]
```

## Description

The `interface-create` command generates a new C# interface file. You can specify base interfaces that the new interface should inherit from. Multiple interfaces can be created at once by providing comma-separated names.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the interface(s) to create (comma-separated for multiple) | No | - |
| `--implements` | `-i` | Base interfaces to inherit from (comma-separated) | No | - |
| `--directory` | `-d` | Target directory for the generated file | No | Current directory |

## Examples

### Create a simple interface

```bash
endpoint interface-create -n ICustomerService
```

**Output: `ICustomerService.cs`**
```csharp
public interface ICustomerService
{
    void Foo();
}
```

### Create an interface inheriting from other interfaces

```bash
endpoint interface-create -n IOrderRepository -i "IRepository,IDisposable"
```

**Output: `IOrderRepository.cs`**
```csharp
public interface IOrderRepository : IRepository, IDisposable
{
    void Foo();
}
```

### Create multiple interfaces at once

```bash
endpoint interface-create -n "IUserService,IProductService,IOrderService"
```

This creates three separate interface files.

### Specify output directory

```bash
endpoint interface-create -n IDataAccess -d ./src/Interfaces
```

## Common Use Cases

1. **Service Interfaces**: Define contracts for services
2. **Repository Pattern**: Create repository interfaces
3. **Dependency Injection**: Define interfaces for DI container registration
4. **Strategy Pattern**: Create strategy interfaces

## Best Practices

- Prefix interface names with `I` (e.g., `ICustomerService`)
- Keep interfaces focused on a single responsibility
- Use base interfaces for common functionality

## Related Commands

- [class-create](./class-create.user-guide.md) - Create classes implementing interfaces
- [service-create](./service-create.user-guide.md) - Create service classes
- [class-and-interface-create](./class-and-interface-create.user-guide.md) - Create both class and interface

[Back to Code Generation](./index.md) | [Back to Index](../index.md)
