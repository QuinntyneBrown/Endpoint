// Auto-generated code
using MessagePack;
using MCC.Shared.Shared.Messaging.Abstractions;

namespace MCC.Shared.Shared.Contracts.Telemetry;

[MessagePackObject]
public sealed class TelemetryReceived : EventBase
{
    [Key(0)]
    public Guid SourceId { get; init; }

    [Key(1)]
    public DateTime Timestamp { get; init; }

    [Key(2)]
    public byte[] Data { get; init; } = Array.Empty<byte>();

}
