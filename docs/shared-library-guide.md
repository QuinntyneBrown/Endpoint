# Shared Library Generator User Guide

The `shared-library-create` command generates a complete shared library solution for microservices, including messaging abstractions, domain primitives, service contracts, and protocol-specific implementations.

## Table of Contents

- [Quick Start](#quick-start)
- [Command Options](#command-options)
- [Configuration File Format](#configuration-file-format)
- [Generated Structure](#generated-structure)
- [Protocol Support](#protocol-support)
- [CCSDS Support](#ccsds-support)
- [Examples](#examples)

## Quick Start

1. Create a YAML configuration file:

```yaml
solution:
  name: MySharedLibrary
  namespace: MyCompany.Shared
  outputPath: ./output
  targetFramework: net9.0

protocols:
  redis:
    enabled: true

services:
  - name: Orders
    events:
      - name: OrderCreated
        properties:
          - name: OrderId
            type: Guid
            key: 0
          - name: CustomerId
            type: Guid
            key: 1
          - name: TotalAmount
            type: decimal
            key: 2
```

2. Generate the shared library:

```bash
endpoint shared-library-create -c ./my-config.yaml
```

3. Build the generated solution:

```bash
cd ./output/MySharedLibrary
dotnet build
```

## Command Options

| Option | Short | Required | Description |
|--------|-------|----------|-------------|
| `--config` | `-c` | Yes | Path to the YAML configuration file |
| `--output` | `-o` | No | Output directory (overrides config) |
| `--library-name` | `-n` | No | Name prefix for generated projects (default: 'Shared'). Use 'Common' to generate Common.Domain, Common.Contracts, etc. |
| `--protocols` | | No | Comma-separated protocols to enable (redis,ccsds,udp,azureservicebus) |
| `--serializers` | | No | Comma-separated serializers to enable (messagepack,json,ccsds) |
| `--dry-run` | | No | Preview generated structure without creating files |
| `--verbose` | `-v` | No | Enable verbose output |

### Examples

```bash
# Basic usage
endpoint shared-library-create -c ./config.yaml

# Preview mode
endpoint shared-library-create -c ./config.yaml --dry-run

# Override output directory
endpoint shared-library-create -c ./config.yaml -o ./my-output

# Enable only specific protocols
endpoint shared-library-create -c ./config.yaml --protocols redis,udp

# Use a custom library name (generates Common.Domain, Common.Contracts, etc.)
endpoint shared-library-create -c ./config.yaml --library-name Common
```

## Configuration File Format

### Root Structure

```yaml
solution:           # Solution-level settings
protocols:          # Protocol configurations
serializers:        # Serializer configurations
services:           # Service and event definitions
domain:             # Domain type definitions
ccsdsPackets:       # CCSDS packet definitions (optional)
```

### Solution Configuration

```yaml
solution:
  name: MyLibrary              # Solution name (required)
  namespace: MyCompany.Shared  # Root namespace
  outputPath: ./output         # Output directory
  targetFramework: net9.0      # Target framework
  libraryName: Shared          # Project name prefix (default: 'Shared')
                               # Use 'Common' to generate Common.Domain, etc.
```

### Protocols Configuration

```yaml
protocols:
  redis:
    enabled: true
    channelPrefix: events        # Channel prefix for pub/sub

  udpMulticast:
    enabled: true
    defaultMulticastGroup: 239.0.0.1
    defaultPort: 5000
    defaultTtl: 32

  azureServiceBus:
    enabled: true
    defaultTopic: events
    useSessions: false

  ccsds:
    enabled: true
    includeSecondaryHeader: true
    secondaryHeaderFormat: CUC   # CUC or CDS
    spacecraftId: 42
```

### Serializers Configuration

```yaml
serializers:
  messagePack:
    enabled: true
    useLz4Compression: true
    useContractless: false

  json:
    enabled: false
    useCamelCase: true
    writeIndented: false

  ccsdsBinary:
    enabled: true
    bigEndian: true
```

### Services Configuration

```yaml
services:
  - name: Orders                    # Service name
    description: Order management   # Optional description
    events:
      - name: OrderCreated
        description: Fired when order is created
        routingKey: orders.created  # Optional routing key
        properties:
          - name: OrderId
            type: Guid
            key: 0                  # MessagePack key
            required: true
          - name: Items
            type: List<string>
            key: 1
    commands:
      - name: CreateOrder
        properties:
          - name: CustomerId
            type: Guid
            key: 0
```

### Domain Configuration

```yaml
domain:
  stronglyTypedIds:
    - name: OrderId
      underlyingType: Guid          # Guid (default), int, long, string

    - name: CustomerId
      underlyingType: Guid

  valueObjects:
    - name: Address
      properties:
        - name: Street
          type: string
        - name: City
          type: string
        - name: ZipCode
          type: string
```

### CCSDS Packets Configuration

```yaml
ccsdsPackets:
  - name: TelemetryPacket
    description: Main telemetry packet
    apid: 100                       # Application Process ID (0-2047)
    packetType: 0                   # 0=TM, 1=TC
    hasSecondaryHeader: true
    fields:
      - name: Temperature
        description: Sensor temperature
        dataType: int16             # Data type
        bitSize: 16                 # Bit width
        unit: degC                  # Engineering unit
        scale: 0.1                  # Calibration scale
        offset: -40                 # Calibration offset

      - name: StatusFlags
        dataType: bitfield
        bitSize: 8

      - name: Reserved
        dataType: uint8
        bitSize: 8
        isSpare: true               # Mark as spare/padding
```

#### Supported CCSDS Data Types

| Type | Description | Typical Bit Sizes |
|------|-------------|-------------------|
| `uint8` | Unsigned 8-bit integer | 8 |
| `uint16` | Unsigned 16-bit integer | 16 |
| `uint32` | Unsigned 32-bit integer | 24, 32 |
| `int8` | Signed 8-bit integer | 8 |
| `int16` | Signed 16-bit integer | 16 |
| `int32` | Signed 32-bit integer | 24, 32 |
| `float32` | 32-bit floating point | 32 |
| `float64` | 64-bit floating point | 64 |
| `bool` | Boolean | 1 |
| `bitfield` | Arbitrary bit field | Any |

### JSC Protocol Configuration (JSC-35199)

```yaml
protocols:
  jsc:
    enabled: true
    sourceMccId: 1                    # Source MCC identifier (2 bytes)
    defaultDestinationMccId: 2        # Default destination MCC ID
    protocolVersion: 1                # Protocol version
    includeCrc32: true                # Include CRC-32 checksum
    messageTypes:
      - name: TelemetryData
        typeCode: 0x10                # Message type code (0x01-0xFF)
        description: Real-time telemetry from spacecraft
        secondaryHeaderType: Telemetry  # Common, Command, Telemetry, FileTransfer, Heartbeat
        userDataFields:
          - name: Temperature
            type: ushort              # byte, ushort, uint, ulong, string, bytes
            description: Temperature reading
          - name: Pressure
            type: uint
          - name: StatusMessage
            type: string
            length: 64                # Required for string/bytes types

      - name: CommandAck
        typeCode: 0x20
        secondaryHeaderType: Command
        userDataFields:
          - name: CommandId
            type: uint
          - name: Status
            type: byte
```

### Messaging Infrastructure Configuration

```yaml
messagingInfrastructure:
  includeRetryPolicies: true          # Retry with exponential backoff
  includeCircuitBreaker: true         # Circuit breaker pattern
  includeDeadLetterQueue: true        # Failed message handling
  includeMessageValidation: true      # Fluent validation framework
  includeDistributedTracing: true     # Trace context propagation
  includeMessageVersioning: true      # Schema versioning support
  includeSerializationHelpers: true   # BigEndian, JSON helpers
  includeRepositoryInterfaces: true   # IRepository, IUnitOfWork
  includeEntityBaseClasses: true      # Entity, AuditableEntity
```

### Documentation Configuration

```yaml
documentation:
  enabled: true                       # Generate documentation
  outputFolder: docs                  # Output folder name
  generateReadme: true                # Main README.md
  generateArchitectureGuide: true     # Architecture documentation
  generateProtocolDocs: true          # Protocol-specific docs
  generateExtensionGuides: true       # How to extend the library
  generateApiReference: true          # API interface docs
```

## Generated Structure

The library name prefix can be customized using `--library-name` or `solution.libraryName`. Default is "Shared".

```
{SolutionName}/
├── {SolutionName}.sln
└── src/
    └── {LibraryName}/                          # e.g., "Shared" or "Common"
        ├── {SolutionName}.{LibraryName}/       # Aggregator project
        │   └── {LibraryName}Exports.cs
        ├── {LibraryName}.Messaging.Abstractions/  # Core interfaces
        │   ├── IEvent.cs
        │   ├── EventBase.cs
        │   ├── IEventBus.cs
        │   ├── IMessageSerializer.cs
        │   └── ServiceCollectionExtensions.cs
        ├── {LibraryName}.Domain/               # Domain primitives
        │   ├── Result.cs
        │   ├── Error.cs
        │   ├── StronglyTypedId.cs
        │   ├── ValueObject.cs
        │   ├── Ids/
        │   │   └── {StronglyTypedId}.cs
        │   └── ValueObjects/
        │       └── {ValueObject}.cs
        ├── {LibraryName}.Contracts/            # Service contracts
        │   └── {ServiceName}/
        │       ├── {Event}.cs
        │       └── {Command}.cs
        ├── {LibraryName}.Messaging.Redis/      # If Redis enabled
        │   ├── RedisEventBusOptions.cs
        │   ├── RedisEventBus.cs
        │   ├── MessagePackMessageSerializer.cs
        │   └── ServiceCollectionExtensions.cs
        ├── {LibraryName}.Messaging.UdpMulticast/  # If UDP enabled
        │   ├── UdpMulticastOptions.cs
        │   ├── UdpMulticastEventBus.cs
        │   ├── MessagePackMessageSerializer.cs
        │   └── ServiceCollectionExtensions.cs
        ├── {LibraryName}.Messaging.AzureServiceBus/  # If Azure SB enabled
        │   ├── AzureServiceBusOptions.cs
        │   ├── AzureServiceBusEventBus.cs
        │   ├── MessagePackMessageSerializer.cs
        │   └── ServiceCollectionExtensions.cs
        ├── {LibraryName}.Messaging.Ccsds/      # If CCSDS enabled
        │   ├── BitPacker.cs
        │   ├── BitUnpacker.cs
        │   ├── CcsdsPacket.cs
        │   ├── CcsdsPrimaryHeader.cs
        │   ├── CcsdsSerializer.cs
        │   ├── PacketRegistry.cs
        │   ├── ServiceCollectionExtensions.cs
        │   └── Packets/
        │       └── {PacketName}.cs
        ├── {LibraryName}.Messaging.Jsc/        # If JSC enabled
        │   ├── BigEndianConverter.cs
        │   ├── Crc32.cs
        │   ├── JscMessageEnums.cs
        │   ├── JscPrimaryHeader.cs
        │   ├── JscSecondaryHeaders.cs
        │   ├── JscMessage.cs
        │   ├── JscMessageSerializer.cs
        │   ├── ServiceCollectionExtensions.cs
        │   └── Messages/
        │       └── {MessageType}.cs
        ├── {LibraryName}.Messaging.Infrastructure/  # If infrastructure enabled
        │   ├── RetryPolicy.cs
        │   ├── CircuitBreaker.cs
        │   ├── DeadLetterQueue.cs
        │   ├── MessageValidation.cs
        │   ├── DistributedTracing.cs
        │   ├── SerializationHelpers.cs
        │   ├── MessageEnvelope.cs
        │   └── Interfaces/
        │       ├── IRepository.cs
        │       ├── IService.cs
        │       └── IUnitOfWork.cs
        └── docs/                               # If documentation enabled
            ├── README.md
            ├── architecture/
            ├── protocols/
            ├── extending/
            └── api/
```

## Protocol Support

### Redis Pub/Sub

Uses StackExchange.Redis for pub/sub messaging with MessagePack serialization.

**Usage in your service:**

```csharp
services.AddRedisEventBus(options =>
{
    options.ConnectionString = "localhost:6379";
    options.ChannelPrefix = "myapp";
});
```

**Publishing events:**

```csharp
public class OrderService
{
    private readonly IEventBus _eventBus;

    public async Task CreateOrderAsync(Order order)
    {
        // ... create order logic ...

        await _eventBus.PublishAsync(new OrderCreated
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.Total
        });
    }
}
```

### UDP Multicast

Uses UDP multicast for low-latency, high-throughput messaging.

**Usage:**

```csharp
services.AddUdpMulticastEventBus(options =>
{
    options.MulticastGroup = "239.0.0.1";
    options.Port = 5000;
    options.Ttl = 32;
});
```

### Azure Service Bus

Uses Azure Service Bus for reliable cloud messaging.

**Usage:**

```csharp
services.AddAzureServiceBusEventBus(options =>
{
    options.ConnectionString = "your-connection-string";
    options.TopicName = "events";
    options.SubscriptionName = "my-service";
});
```

### CCSDS Space Packets

Generates CCSDS (Consultative Committee for Space Data Systems) packet handlers with bit-level serialization.

**Usage:**

```csharp
services.AddCcsdsSerializer(registry =>
{
    registry.Register<TelemetryPacket>();
    registry.Register<CommandPacket>();
});
```

**Creating and serializing packets:**

```csharp
var packet = new TelemetryPacket
{
    Temperature = 2500,  // Raw value
    StatusFlags = 0x0F,
    SequenceCount = 42,
    Timestamp = DateTimeOffset.UtcNow
};

var serializer = serviceProvider.GetRequiredService<IMessageSerializer>();
byte[] bytes = serializer.Serialize(packet);
```

### JSC Protocol (JSC-35199)

Implements the JSC-35199 specification for inter-MCC (Mission Control Center) communication with big-endian serialization and CRC-32 checksums.

**Usage:**

```csharp
services.AddJscMessaging(options =>
{
    options.SourceMccId = 1;
    options.DefaultDestinationMccId = 2;
    options.IncludeCrc32 = true;
});
```

**Creating and sending messages:**

```csharp
var serializer = serviceProvider.GetRequiredService<JscMessageSerializer>();

// Create a telemetry message
var message = JscMessage.Create(
    JscMessageType.Telemetry,
    telemetryData,
    new JscTelemetrySecondaryHeader
    {
        Timestamp = DateTime.UtcNow.Ticks,
        SampleRate = 100,
        QualityIndicator = 0xFFFF
    });

byte[] bytes = message.Serialize();
```

**Deserializing messages:**

```csharp
var message = JscMessage.Deserialize(receivedBytes);
// Checksum is verified automatically during deserialization
```

## Messaging Infrastructure

When enabled, the messaging infrastructure project provides enterprise patterns:

### Retry Policies

```csharp
var policy = new RetryPolicy(maxRetries: 3, baseDelayMs: 100, useJitter: true);
var executor = new RetryExecutor(policy, logger);
await executor.ExecuteAsync(async () => await SendMessageAsync());
```

### Circuit Breaker

```csharp
var breaker = new CircuitBreaker(failureThreshold: 5, openDurationMs: 30000);
await breaker.ExecuteAsync(async () => await CallExternalServiceAsync());
```

### Dead Letter Queue

```csharp
var dlq = serviceProvider.GetRequiredService<IDeadLetterQueue>();
await dlq.EnqueueAsync(failedMessage, exception, metadata);
```

### Message Validation

```csharp
var validator = new MessageValidator<OrderCreated>()
    .RuleFor(x => x.OrderId, id => id != Guid.Empty, "OrderId is required")
    .RuleFor(x => x.Amount, amt => amt > 0, "Amount must be positive");

var result = validator.Validate(message);
if (!result.IsValid)
{
    // Handle validation errors
}
```

## Examples

### Minimal Configuration

```yaml
solution:
  name: SimpleLib

services:
  - name: Notifications
    events:
      - name: NotificationSent
        properties:
          - name: Message
            type: string
            key: 0
```

### E-Commerce Example

```yaml
solution:
  name: ECommerceShared
  namespace: ECommerce.Shared

protocols:
  redis:
    enabled: true

services:
  - name: Orders
    events:
      - name: OrderPlaced
        properties:
          - name: OrderId
            type: Guid
            key: 0
          - name: CustomerId
            type: Guid
            key: 1
          - name: Items
            type: List<OrderItem>
            key: 2
          - name: TotalAmount
            type: decimal
            key: 3

      - name: OrderShipped
        properties:
          - name: OrderId
            type: Guid
            key: 0
          - name: TrackingNumber
            type: string
            key: 1

  - name: Inventory
    events:
      - name: StockUpdated
        properties:
          - name: ProductId
            type: Guid
            key: 0
          - name: Quantity
            type: int
            key: 1

domain:
  stronglyTypedIds:
    - name: OrderId
    - name: CustomerId
    - name: ProductId

  valueObjects:
    - name: Money
      properties:
        - name: Amount
          type: decimal
        - name: Currency
          type: string
```

### Flight Simulator with CCSDS

```yaml
solution:
  name: FlightSimShared

protocols:
  udpMulticast:
    enabled: true
    defaultMulticastGroup: 239.0.0.1
    defaultPort: 5000

  ccsds:
    enabled: true

ccsdsPackets:
  - name: AircraftStatePacket
    apid: 100
    packetType: 0
    hasSecondaryHeader: true
    fields:
      - name: AircraftId
        dataType: uint32
        bitSize: 32

      - name: Latitude
        dataType: int32
        bitSize: 32
        scale: 0.0000001
        unit: degrees

      - name: Longitude
        dataType: int32
        bitSize: 32
        scale: 0.0000001
        unit: degrees

      - name: Altitude
        dataType: uint32
        bitSize: 24
        unit: feet

      - name: Heading
        dataType: uint16
        bitSize: 16
        scale: 0.01
        unit: degrees
```

### Mission Control with JSC Protocol

```yaml
solution:
  name: MissionControlShared
  namespace: MCC.Shared

protocols:
  jsc:
    enabled: true
    sourceMccId: 1
    defaultDestinationMccId: 2
    includeCrc32: true
    messageTypes:
      - name: TelemetryStream
        typeCode: 0x10
        secondaryHeaderType: Telemetry
        userDataFields:
          - name: SensorId
            type: uint
          - name: Value
            type: ulong
          - name: Quality
            type: byte

      - name: CommandResponse
        typeCode: 0x20
        secondaryHeaderType: Command
        userDataFields:
          - name: CommandSequence
            type: uint
          - name: ResultCode
            type: ushort

  udpMulticast:
    enabled: true
    defaultMulticastGroup: 239.1.1.1
    defaultPort: 6000

messagingInfrastructure:
  includeRetryPolicies: true
  includeCircuitBreaker: true
  includeDistributedTracing: true
  includeSerializationHelpers: true

documentation:
  enabled: true
  generateArchitectureGuide: true
  generateProtocolDocs: true
```

## Troubleshooting

### Common Issues

**"Configuration file not found"**
- Ensure the path to your YAML file is correct
- Use absolute paths if relative paths aren't working

**"Solution name is required"**
- Add a `solution.name` field to your YAML configuration

**"Duplicate APID found"**
- Each CCSDS packet must have a unique APID value

**Build errors in generated code**
- Ensure all referenced types exist (e.g., custom types like `OrderItem`)
- Check that MessagePack keys are unique within each event/command

**"Duplicate type code found" (JSC)**
- Each JSC message type must have a unique typeCode value (0x01-0xFF)

**"Invalid secondary header type" (JSC)**
- Valid secondary header types are: Common, Command, Telemetry, FileTransfer, Heartbeat

**"Length required for string/bytes type" (JSC)**
- JSC fields with `type: string` or `type: bytes` require a `length` property

**"Checksum mismatch" at runtime (JSC)**
- Ensure both sender and receiver have matching CRC-32 settings
- Verify the message was not corrupted in transit

### Getting Help

- Use `--dry-run` to preview what will be generated
- Use `--verbose` for detailed logging
- Check the generated code for any issues before building
