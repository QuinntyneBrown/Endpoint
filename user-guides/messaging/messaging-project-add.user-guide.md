# messaging-project-add

Add a messaging project with message definitions.

## Synopsis

```bash
endpoint messaging-project-add [options]
```

## Description

The `messaging-project-add` command creates a messaging project with predefined message types. It can generate messages from a JSON definition file and optionally include Redis support and message compression.

## Options

| Option | Short | Description | Required | Default |
|--------|-------|-------------|----------|---------|
| `--file` | `-f` | JSON file with message definitions | No | - |
| `--include-redis` | - | Include Redis Pub/Sub support | No | `false` |
| `--use-compression` | - | Use LZ4 compression | No | `false` |
| `--generate-sample` | - | Generate a sample JSON file | No | `false` |

## Examples

### Generate sample message definition

```bash
endpoint messaging-project-add --generate-sample
```

Creates `messages.json`:

```json
{
  "namespace": "MyApp.Messaging",
  "messages": [
    {
      "name": "OrderCreated",
      "properties": [
        { "name": "OrderId", "type": "Guid" },
        { "name": "CustomerId", "type": "Guid" },
        { "name": "Total", "type": "decimal" },
        { "name": "CreatedAt", "type": "DateTime" }
      ]
    },
    {
      "name": "OrderShipped",
      "properties": [
        { "name": "OrderId", "type": "Guid" },
        { "name": "TrackingNumber", "type": "string" },
        { "name": "ShippedAt", "type": "DateTime" }
      ]
    }
  ]
}
```

### Create project from definition

```bash
endpoint messaging-project-add -f ./messages.json
```

### Create with Redis support

```bash
endpoint messaging-project-add -f ./messages.json --include-redis
```

### Create with compression

```bash
endpoint messaging-project-add -f ./messages.json --use-compression
```

### Full featured

```bash
endpoint messaging-project-add -f ./messages.json --include-redis --use-compression
```

## JSON Definition Schema

```json
{
  "namespace": "string",
  "messages": [
    {
      "name": "MessageName",
      "description": "Optional description",
      "properties": [
        {
          "name": "PropertyName",
          "type": "PropertyType",
          "required": true
        }
      ]
    }
  ]
}
```

## Supported Property Types

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
| `List<T>` | `List<T>` |
| `Dictionary<K,V>` | `Dictionary<K,V>` |

## Generated Message Example

For the definition above:

```csharp
[MessagePackObject]
public class OrderCreatedMessage
{
    [Key(0)]
    public Guid OrderId { get; set; }

    [Key(1)]
    public Guid CustomerId { get; set; }

    [Key(2)]
    public decimal Total { get; set; }

    [Key(3)]
    public DateTime CreatedAt { get; set; }
}
```

## Generated Project Structure

```
MyApp.Messaging/
├── MyApp.Messaging.csproj
├── Messages/
│   ├── OrderCreatedMessage.cs
│   ├── OrderShippedMessage.cs
│   └── ...
├── Publishers/
│   └── MessagePublisher.cs (with --include-redis)
├── Consumers/
│   └── MessageConsumerBase.cs (with --include-redis)
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

## Common Use Cases

1. **Contract-First Design**: Define messages before implementation
2. **Code Generation**: Generate message classes from JSON
3. **Multi-Service**: Share message definitions across services
4. **Documentation**: JSON serves as message documentation

## Related Commands

- [messaging-add](./messaging-add.user-guide.md) - Add messaging infrastructure
- [event-driven-microservices-create](../application-scaffolding/event-driven-microservices-create.user-guide.md) - Create microservices

[Back to Messaging](./index.md) | [Back to Index](../index.md)
