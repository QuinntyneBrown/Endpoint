# API Reference

## Core Interfaces

### IEvent

Base interface for all events.

```csharp
public interface IEvent
{
    Guid EventId { get; }
    DateTime Timestamp { get; }
    string EventType { get; }
}
```

### IEventBus

Interface for publish/subscribe operations.

```csharp
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent;

    Task SubscribeAsync<TEvent>(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}
```

### IMessageSerializer

Interface for message serialization.

```csharp
public interface IMessageSerializer
{
    byte[] Serialize<T>(T message);
    T Deserialize<T>(byte[] data);
    object Deserialize(byte[] data, Type type);
}
```

### IEventHandler<TEvent>

Interface for event handlers.

```csharp
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
```

## Domain Types

### Result<T>

Railway-oriented result type for error handling.

```csharp
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public Error Error { get; }

    public static Result<T> Success(T value);
    public static Result<T> Failure(Error error);
}
```

### Strongly-Typed IDs

#### SpacecraftId

Underlying type: `Guid`

#### MissionId

Underlying type: `Guid`

#### CommandId

Underlying type: `Guid`

### Value Objects

#### GeoLocation

Properties:
- `Latitude`: double
- `Longitude`: double
- `Altitude`: double

