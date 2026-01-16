# event-driven-microservices-create

Create an event-driven microservices architecture with multiple services.

## Synopsis

```bash
endpoint event-driven-microservices-create [options]
```

## Description

The `event-driven-microservices-create` command generates a complete event-driven microservices solution with multiple services that communicate via events. This is ideal for building distributed systems with loose coupling between services.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Name of the solution | No | - |
| `--services` | `-s` | Comma-separated list of service names | No | - |
| `--directory` | `-d` | Target directory | No | Current directory |

## Examples

### Create an e-commerce microservices solution

```bash
endpoint event-driven-microservices-create -n ECommerce -s "Orders,Inventory,Shipping,Payments"
```

### Create a simple two-service system

```bash
endpoint event-driven-microservices-create -n NotificationSystem -s "Users,Notifications"
```

### Specify output directory

```bash
endpoint event-driven-microservices-create -n MyPlatform -s "Auth,Products,Orders" -d ./microservices
```

## Generated Structure

For `endpoint event-driven-microservices-create -n ECommerce -s "Orders,Inventory"`:

```
ECommerce/
├── ECommerce.sln
├── src/
│   ├── ECommerce.Orders/
│   │   ├── ECommerce.Orders.Api/
│   │   │   ├── Controllers/
│   │   │   ├── Program.cs
│   │   │   └── ECommerce.Orders.Api.csproj
│   │   ├── ECommerce.Orders.Core/
│   │   │   └── ECommerce.Orders.Core.csproj
│   │   └── ECommerce.Orders.Infrastructure/
│   │       └── ECommerce.Orders.Infrastructure.csproj
│   ├── ECommerce.Inventory/
│   │   ├── ECommerce.Inventory.Api/
│   │   ├── ECommerce.Inventory.Core/
│   │   └── ECommerce.Inventory.Infrastructure/
│   └── ECommerce.Messaging/
│       ├── Messages/
│       │   ├── OrderCreatedEvent.cs
│       │   └── InventoryUpdatedEvent.cs
│       └── ECommerce.Messaging.csproj
└── tests/
```

## Event-Driven Architecture

The generated services use:

- **Message Bus**: For asynchronous communication
- **Events**: For publishing domain events
- **Handlers**: For consuming events from other services

### Example Events

```csharp
// Published by Orders service
public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public List<OrderItem> Items { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Consumed by Inventory service
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent @event)
    {
        // Reserve inventory
    }
}
```

## Service Communication Patterns

| Pattern | Description |
|---------|-------------|
| **Event Sourcing** | Persist events as source of truth |
| **Pub/Sub** | Publish events, multiple subscribers |
| **Request/Reply** | Synchronous communication when needed |
| **Saga** | Distributed transactions via events |

## Common Microservices Setups

### E-Commerce

```bash
endpoint event-driven-microservices-create -n ECommerce \
  -s "Products,Orders,Inventory,Shipping,Payments,Notifications"
```

### SaaS Platform

```bash
endpoint event-driven-microservices-create -n SaaS \
  -s "Auth,Tenants,Billing,Users,Analytics"
```

### IoT Platform

```bash
endpoint event-driven-microservices-create -n IoT \
  -s "Devices,Telemetry,Alerts,Dashboard"
```

## Benefits

1. **Loose Coupling**: Services are independent
2. **Scalability**: Scale individual services
3. **Resilience**: Failure isolation
4. **Flexibility**: Technology diversity per service
5. **Maintainability**: Smaller, focused codebases

## Best Practices

- Keep services small and focused
- Define clear bounded contexts
- Use shared messaging contracts
- Implement eventual consistency
- Add proper error handling and retries

## Related Commands

- [microservice-add](./microservice-add.user-guide.md) - Add more microservices
- [messaging-add](../messaging/messaging-add.user-guide.md) - Add messaging infrastructure
- [ddd-app-create](./ddd-app-create.user-guide.md) - Create single service

[Back to Application Scaffolding](./index.md) | [Back to Index](../index.md)
