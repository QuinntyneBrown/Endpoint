# Shared Library Demo

This playground demonstrates the `shared-library-create` command with configurations ranging from simple to complex.

## Demo Configurations

### 1. Simple Library (`configs/simple-library.yaml`)

A minimal shared library with:
- Basic messaging abstractions
- Single service with events
- Simple domain types

**Generate:**
```bash
endpoint shared-library-create -c configs/simple-library.yaml
```

### 2. Intermediate Library (`configs/intermediate-library.yaml`)

An e-commerce shared library with:
- Redis pub/sub enabled
- Multiple services (Orders, Inventory, Customers)
- Multiple events and commands
- Domain types (Money, Address, EmailAddress)

**Generate:**
```bash
endpoint shared-library-create -c configs/intermediate-library.yaml
```

### 3. Complex Library (`configs/complex-library.yaml`)

A flight simulation telemetry system with:
- All protocols enabled (Redis, UDP Multicast, Azure Service Bus, CCSDS)
- Multiple serializers (MessagePack, JSON, CCSDS Binary)
- CCSDS space packets with bit-level fields
- Comprehensive domain model

**Generate:**
```bash
endpoint shared-library-create -c configs/complex-library.yaml
```

## Running the Demos

### Prerequisites

1. Build the Endpoint CLI:
```bash
cd C:/projects/Endpoint
dotnet build src/Endpoint.Engineering.Cli/Endpoint.Engineering.Cli.csproj
```

### Generate All Libraries

```bash
# From the Endpoint root directory

# Simple Library
dotnet run --project src/Endpoint.Engineering.Cli/Endpoint.Engineering.Cli.csproj -- \
  shared-library-create -c playground/SharedLibraryDemo/configs/simple-library.yaml

# Intermediate Library
dotnet run --project src/Endpoint.Engineering.Cli/Endpoint.Engineering.Cli.csproj -- \
  shared-library-create -c playground/SharedLibraryDemo/configs/intermediate-library.yaml

# Complex Library
dotnet run --project src/Endpoint.Engineering.Cli/Endpoint.Engineering.Cli.csproj -- \
  shared-library-create -c playground/SharedLibraryDemo/configs/complex-library.yaml
```

### Preview Mode

To see what would be generated without creating files:

```bash
dotnet run --project src/Endpoint.Engineering.Cli/Endpoint.Engineering.Cli.csproj -- \
  shared-library-create -c playground/SharedLibraryDemo/configs/simple-library.yaml --dry-run
```

### Build Generated Libraries

```bash
# Build Simple Library
cd C:/projects/Endpoint/generated-output/SimpleLibrary
dotnet build

# Build Intermediate Library
cd C:/projects/Endpoint/generated-output/IntermediateLibrary
dotnet build

# Build Complex Library
cd C:/projects/Endpoint/generated-output/FlightSimLibrary
dotnet build
```

## Generated Structure

Each generated library follows this structure:

```
{SolutionName}/
├── {SolutionName}.sln
└── src/
    └── Shared/
        ├── {SolutionName}.Shared/              # Aggregator
        ├── Shared.Messaging.Abstractions/      # Core interfaces
        ├── Shared.Domain/                      # Domain primitives
        ├── Shared.Contracts/                   # Events & commands
        ├── Shared.Messaging.Redis/             # (if enabled)
        ├── Shared.Messaging.UdpMulticast/      # (if enabled)
        ├── Shared.Messaging.AzureServiceBus/   # (if enabled)
        └── Shared.Messaging.Ccsds/             # (if enabled)
```

## Key Features Demonstrated

### Simple Library
- Basic IEvent and IEventBus interfaces
- EventBase class with correlation support
- Strongly-typed ID pattern
- Value object pattern

### Intermediate Library
- Redis pub/sub event bus
- MessagePack serialization with LZ4 compression
- Multiple services with events and commands
- Complex domain value objects

### Complex Library
- Multi-protocol support
- CCSDS space packet serialization
- Bit-level field packing/unpacking
- Complete telemetry system example
