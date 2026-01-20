# Adding New Protocols

## Overview

To add a new messaging protocol, you need to implement the `IEventBus` interface and provide a message serializer.

## Implementation Steps

### 1. Create a New Project

Create a new class library: `Shared.Messaging.MyProtocol`

Add project references:

```xml
<ItemGroup>
  <ProjectReference Include="..\Shared.Messaging.Abstractions\Shared.Messaging.Abstractions.csproj" />
</ItemGroup>
```

### 2. Create Options Class

```csharp
public class MyProtocolOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30000;
    // Add protocol-specific options
}
```

### 3. Implement IEventBus

```csharp
using MCC.Shared.Messaging.Abstractions;

public class MyProtocolEventBus : IEventBus
{
    private readonly MyProtocolOptions _options;
    private readonly IMessageSerializer _serializer;

    public MyProtocolEventBus(
        IOptions<MyProtocolOptions> options,
        IMessageSerializer serializer)
    {
        _options = options.Value;
        _serializer = serializer;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IEvent
    {
        var bytes = _serializer.Serialize(@event);
        // Send bytes using your protocol
    }

    public Task SubscribeAsync<TEvent>(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken ct = default)
        where TEvent : IEvent
    {
        // Implement subscription logic
        // Call handler when messages arrive
        return Task.CompletedTask;
    }
}
```

### 4. Create Extension Method

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyProtocolEventBus(
        this IServiceCollection services,
        Action<MyProtocolOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IEventBus, MyProtocolEventBus>();
        return services;
    }
}
```

### 5. Update the Aggregator Project

Add a reference to your new project in `MissionControlShared.Shared.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\Shared.Messaging.MyProtocol\Shared.Messaging.MyProtocol.csproj" />
</ItemGroup>
```
