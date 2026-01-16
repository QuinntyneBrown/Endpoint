# service-create

Create a service class for business logic encapsulation.

## Synopsis

```bash
endpoint service-create [options]
```

## Description

The `service-create` command generates a service class that can be used to encapsulate business logic. Services are typically registered with the dependency injection container and injected into controllers or other services.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the service to create | No | - |
| `--directory` | `-d` | Target directory for the generated file | No | Current directory |

## Examples

### Create a basic service

```bash
endpoint service-create -n EmailService
```

**Output: `EmailService.cs`**
```csharp
public class EmailService
{
}
```

### Specify output directory

```bash
endpoint service-create -n NotificationService -d ./src/Services
```

## Common Service Patterns

### Repository Service

```bash
endpoint service-create -n CustomerRepository
```

Use for data access logic.

### Business Logic Service

```bash
endpoint service-create -n OrderProcessingService
```

Use for complex business operations.

### External Integration Service

```bash
endpoint service-create -n PaymentGatewayService
```

Use for third-party integrations.

## Best Practices

1. **Single Responsibility**: Each service should have one clear purpose
2. **Interface First**: Create an interface alongside the service for DI
3. **Dependency Injection**: Accept dependencies through constructor
4. **Testability**: Design services to be easily unit tested

## Recommended Structure

```csharp
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        // Implementation
    }
}
```

## Common Use Cases

1. **Data Access**: Repository pattern implementations
2. **Business Logic**: Domain-specific operations
3. **External APIs**: Third-party service integrations
4. **Utilities**: Helper services (email, caching, etc.)
5. **Orchestration**: Coordinating multiple operations

## Related Commands

- [interface-create](./interface-create.user-guide.md) - Create service interfaces
- [class-create](./class-create.user-guide.md) - Create general classes

[Back to Code Generation](./index.md) | [Back to Index](../index.md)
