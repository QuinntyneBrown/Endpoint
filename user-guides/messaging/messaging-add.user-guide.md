# messaging-add

Add messaging infrastructure to an existing solution.

## Synopsis

```bash
endpoint messaging-add [options]
```

## Description

The `messaging-add` command adds a messaging project to an existing .NET solution. It sets up the infrastructure for event-driven communication between services using Redis Pub/Sub with MessagePack serialization.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--name` | `-n` | Solution name (auto-detected from .sln if not provided) | No | Auto-detect |
| `--directory` | `-d` | Directory containing the solution file | No | Current directory |
| `--lz4` | - | Use LZ4 compression with MessagePack | No | `true` |

## Examples

### Add messaging to current solution

```bash
endpoint messaging-add
```

### Add messaging with explicit name

```bash
endpoint messaging-add -n MyApp
```

### Add messaging to specific directory

```bash
endpoint messaging-add -d ./backend
```

### Add messaging without compression

```bash
endpoint messaging-add --lz4 false
```

## Generated Structure

For a solution named `MyApp`:

```
src/
└── MyApp.Messaging/
    ├── MyApp.Messaging.csproj
    ├── Messages/
    │   ├── IMessage.cs
    │   └── MessageBase.cs
    ├── Publishers/
    │   └── IMessagePublisher.cs
    ├── Consumers/
    │   └── IMessageConsumer.cs
    └── ConfigureServices.cs
```

## Generated Project Features

- **MessagePack Serialization**: High-performance binary serialization
- **LZ4 Compression**: Optional compression for reduced message size
- **Redis Pub/Sub**: Redis-based message broker integration
- **Type-Safe Messages**: Strongly-typed message contracts

## Dependencies Added

| Package | Purpose |
|---------|---------|
| `MessagePack` | Binary serialization |
| `MessagePack.Lz4Compression` | Optional compression |
| `StackExchange.Redis` | Redis client |
| `Microsoft.Extensions.DependencyInjection` | DI integration |

## Usage Example

After adding messaging:

### Define a Message

```csharp
[MessagePackObject]
public class OrderCreatedMessage : MessageBase
{
    [Key(0)]
    public Guid OrderId { get; set; }

    [Key(1)]
    public decimal Total { get; set; }
}
```

### Publish a Message

```csharp
public class OrderService
{
    private readonly IMessagePublisher _publisher;

    public async Task CreateOrderAsync(Order order)
    {
        // Create order...

        await _publisher.PublishAsync(new OrderCreatedMessage
        {
            OrderId = order.Id,
            Total = order.Total
        });
    }
}
```

### Consume a Message

```csharp
public class InventoryMessageConsumer : IMessageConsumer<OrderCreatedMessage>
{
    public async Task HandleAsync(OrderCreatedMessage message)
    {
        // Update inventory based on order...
    }
}
```

## Service Registration

```csharp
// In Program.cs or Startup.cs
services.AddMessaging(Configuration);
```

## Configuration

Add to `appsettings.json`:

```json
{
  "Messaging": {
    "Redis": {
      "ConnectionString": "localhost:6379",
      "ChannelPrefix": "myapp"
    },
    "UseLz4Compression": true
  }
}
```

## Common Use Cases

1. **Microservices Communication**: Event-driven messaging between services
2. **Background Processing**: Publish events for async processing
3. **Event Sourcing**: Publish domain events
4. **Integration**: Integrate with external systems

## Related Commands

- [messaging-project-add](./messaging-project-add.user-guide.md) - Add messaging project
- [event-driven-microservices-create](../application-scaffolding/event-driven-microservices-create.user-guide.md) - Create microservices

[Back to Messaging](./index.md) | [Back to Index](../index.md)
