# package-add

Add NuGet packages to a .NET project.

## Synopsis

```bash
endpoint package-add [options]
```

## Description

The `package-add` command adds NuGet package references to a .NET project. This is equivalent to running `dotnet add package` but integrated into the Endpoint CLI workflow.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the NuGet package | No | - |
| `--directory` | `-d` | Directory containing the project | No | Current directory |

## Examples

### Add a package

```bash
endpoint package-add -n Newtonsoft.Json
```

### Add package to specific project

```bash
endpoint package-add -n MediatR -d ./src/MyApp.Core
```

### Add multiple packages

```bash
endpoint package-add -n "Microsoft.EntityFrameworkCore"
endpoint package-add -n "Microsoft.EntityFrameworkCore.SqlServer"
endpoint package-add -n "Microsoft.EntityFrameworkCore.Tools"
```

## Common Packages

### Web Development

| Package | Purpose |
|---------|---------|
| `Swashbuckle.AspNetCore` | Swagger/OpenAPI documentation |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT authentication |
| `FluentValidation.AspNetCore` | Request validation |

### Data Access

| Package | Purpose |
|---------|---------|
| `Microsoft.EntityFrameworkCore` | ORM framework |
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL Server provider |
| `Dapper` | Micro-ORM |

### CQRS & Messaging

| Package | Purpose |
|---------|---------|
| `MediatR` | CQRS mediator pattern |
| `MassTransit` | Message bus |
| `RabbitMQ.Client` | RabbitMQ messaging |

### Serialization

| Package | Purpose |
|---------|---------|
| `Newtonsoft.Json` | JSON serialization |
| `MessagePack` | Binary serialization |
| `System.Text.Json` | Built-in JSON (included) |

### Testing

| Package | Purpose |
|---------|---------|
| `xunit` | Unit testing framework |
| `Moq` | Mocking library |
| `FluentAssertions` | Assertion library |
| `Bogus` | Fake data generation |

### Logging

| Package | Purpose |
|---------|---------|
| `Serilog` | Structured logging |
| `Serilog.Sinks.Console` | Console output |
| `Serilog.Sinks.File` | File output |

## How It Works

The command:
1. Locates the .csproj file in the specified directory
2. Runs `dotnet add package <package-name>`
3. Updates the project with the package reference

## Generated Reference

The command adds to the .csproj file:

```xml
<ItemGroup>
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
</ItemGroup>
```

## Common Use Cases

1. **New Projects**: Add required dependencies
2. **Feature Development**: Add packages for new features
3. **Upgrades**: Update to newer package versions
4. **Testing**: Add test frameworks and utilities

## Best Practices

- Pin to specific versions for production
- Review package dependencies before adding
- Keep packages updated for security
- Remove unused packages

## Related Commands

- [reference-add](./reference-add.user-guide.md) - Add project references
- [project-add](./project-add.user-guide.md) - Add new projects

[Back to Project Management](./index.md) | [Back to Index](../index.md)
