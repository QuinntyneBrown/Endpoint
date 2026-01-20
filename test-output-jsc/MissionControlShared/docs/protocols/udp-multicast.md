# UDP Multicast Protocol

## Overview

UDP Multicast provides high-throughput, low-latency messaging using IP multicast groups. It's ideal for real-time data distribution where some message loss is acceptable (e.g., telemetry, market data).

## Configuration

- **Default Multicast Group**: `239.0.0.1`
- **Default Port**: `5000`
- **Default TTL**: `32`

## Usage

### Service Registration

```csharp
services.AddUdpMulticastEventBus(options =>
{
    options.MulticastGroup = "239.0.0.1";
    options.Port = 5000;
    options.Ttl = 32;
});
```

## Network Considerations

- **TTL (Time To Live)**: Controls how many network hops the packets can traverse
  - TTL=1: Local subnet only
  - TTL=32: Reasonable for enterprise networks
  - TTL=255: Maximum (unrestricted)

- **Multicast Groups**: Use addresses in the `239.0.0.0/8` range for organization-local scope

- **Message Size**: UDP has a maximum datagram size. Large messages should be fragmented or use a different protocol.
