# JSC Protocol (JSC-35199)

## Overview

The JSC (Johnson Space Center) protocol follows the JSC-35199 specification for inter-MCC (Mission Control Center) communication. It provides a standardized message format for reliable data exchange between ground systems.

## Configuration

- **Source MCC ID**: 1
- **Default Destination MCC ID**: 2
- **Protocol Version**: 1
- **CRC-32 Enabled**: True

## Message Structure

### Primary Header (16 bytes)

| Field | Size | Description |
|-------|------|-------------|
| Sync Word | 4 bytes | `0x1ACFFC1D` - Magic synchronization pattern |
| Version | 1 byte | Protocol version |
| Message Type | 1 byte | Type code for message routing |
| Message ID | 2 bytes | Unique message identifier |
| Source MCC ID | 2 bytes | Originating MCC identifier |
| Destination MCC ID | 2 bytes | Target MCC identifier |
| Priority | 1 byte | Message priority (0-255) |
| Flags | 1 byte | Control flags |
| Secondary Header Length | 2 bytes | Length of secondary header |

### Secondary Headers

The secondary header type is determined by the message category:

#### Common Secondary Header (8 bytes)
| Field | Size | Description |
|-------|------|-------------|
| Timestamp | 8 bytes | UTC timestamp in ticks |

#### Command Secondary Header (24 bytes)
| Field | Size | Description |
|-------|------|-------------|
| Timestamp | 8 bytes | UTC timestamp |
| Command Sequence | 4 bytes | Command sequence number |
| Execution Time | 8 bytes | Scheduled execution time |
| Target System | 2 bytes | Target system identifier |
| Reserved | 2 bytes | Reserved for future use |

#### Telemetry Secondary Header (16 bytes)
| Field | Size | Description |
|-------|------|-------------|
| Timestamp | 8 bytes | Acquisition timestamp |
| Sample Rate | 4 bytes | Samples per second |
| Quality Indicator | 2 bytes | Data quality flags |
| Reserved | 2 bytes | Reserved for future use |

### CRC-32 Checksum

When enabled, a 4-byte CRC-32 checksum is appended to each message using the IEEE 802.3 polynomial (`0xEDB88320`).

## Usage

### Registering JSC Messaging

```csharp
services.AddJscMessaging(options =>
{
    options.SourceMccId = 1;
    options.DefaultDestinationMccId = 2;
    options.IncludeCrc32 = true;
});
```

### Creating and Sending Messages

```csharp
var serializer = serviceProvider.GetRequiredService<JscMessageSerializer>();

var message = new JscMessage
{
    MessageType = JscMessageType.Telemetry,
    Priority = 128,
    UserData = telemetryData
};

byte[] bytes = serializer.Serialize(message);
```

## Configured Message Types

| Name | Type Code | Secondary Header | Description |
|------|-----------|------------------|-------------|
| TelemetryData | 0x10 | Telemetry | Real-time telemetry from spacecraft |
| CommandAck | 0x20 | Command | Command acknowledgment |
| SystemHeartbeat | 0x01 | Heartbeat | System heartbeat message |

