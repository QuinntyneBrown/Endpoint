# Adding New Events

## Overview

Events are defined in the `Shared.Contracts` project. Each service has its own namespace and folder.

## Step-by-Step Guide

### 1. Identify the Service

Determine which service owns this event. Events should be grouped by the service that publishes them.

### 2. Create the Event Class

Create a new class in `Shared.Contracts/{ServiceName}/`:

```csharp
using MCC.Shared.Messaging.Abstractions;
using MessagePack;

namespace MCC.Shared.Contracts.MyService;

[MessagePackObject]
public class MyNewEvent : EventBase
{
    [Key(0)]
    public Guid EntityId { get; set; }

    [Key(1)]
    public string Description { get; set; } = string.Empty;

    [Key(2)]
    public DateTime OccurredAt { get; set; }
}
```

### 3. MessagePack Key Considerations

- **Keys must be unique** within the class
- **Keys are immutable** - once published, never change a key number
- **Start from 0** and increment sequentially
- **Add new properties at the end** with the next available key

### 4. Backward Compatibility

When modifying events:

| Action | Safe? | Notes |
|--------|-------|-------|
| Add new property | ✅ | Use next available key |
| Remove property | ⚠️ | Old messages may fail |
| Rename property | ✅ | Key stays the same |
| Change key number | ❌ | Breaks deserialization |
| Change property type | ❌ | Breaks deserialization |

### 5. Publishing the Event

```csharp
public class MyService
{
    private readonly IEventBus _eventBus;

    public async Task DoWorkAsync()
    {
        // ... business logic ...

        await _eventBus.PublishAsync(new MyNewEvent
        {
            EntityId = Guid.NewGuid(),
            Description = "Something happened",
            OccurredAt = DateTime.UtcNow
        });
    }
}
```

### 6. Handling the Event

Create a handler in your consuming service:

```csharp
public class MyNewEventHandler : IEventHandler<MyNewEvent>
{
    public Task HandleAsync(MyNewEvent @event, CancellationToken cancellationToken)
    {
        // Handle the event
        return Task.CompletedTask;
    }
}
```
