# Project Structure

## Directory Layout

```
MissionControlShared/
├── MissionControlShared.sln
├── docs/                                    # This documentation
└── src/
    └── Shared/
        ├── MissionControlShared.Shared/         # Aggregator
        ├── Shared.Messaging.Abstractions/    # Core interfaces
        ├── Shared.Domain/                    # Domain primitives
        ├── Shared.Contracts/                 # Event definitions
        ├── Shared.Messaging.UdpMulticast/    # UDP implementation
        ├── Shared.Messaging.Jsc/             # JSC implementation
        └── Shared.Messaging.Infrastructure/  # Advanced features
```

## Project Descriptions

### MissionControlShared.Shared (Aggregator)

The aggregator project provides a single entry point for consuming applications. It re-exports all types from the underlying projects.

**Usage:**
```csharp
using MCC.Shared;
// All types are available through this single namespace
```

### Shared.Messaging.Abstractions

Contains core interfaces that define the messaging contracts:

| Interface | Purpose |
|-----------|---------|
| `IEvent` | Base interface for all events |
| `IEventBus` | Publish/subscribe operations |
| `IMessageSerializer` | Serialization/deserialization |
| `IEventHandler<T>` | Event handler interface |

### Shared.Domain

Contains domain primitives including:

- **Strongly-Typed IDs**: Type-safe identifiers that prevent mixing different ID types
- **Value Objects**: Immutable objects defined by their values
- **Result Pattern**: Railway-oriented programming for error handling

### Shared.Contracts

Contains event and command definitions organized by service:

```
Shared.Contracts/
└── Telemetry/
    ├── TelemetryReceived.cs
└── Commands/
    ├── CommandIssued.cs
```

