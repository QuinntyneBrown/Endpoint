// Auto-generated code
using MessagePack;

namespace FlightSim.Shared.Contracts.Telemetry;

[MessagePackObject]
public sealed class RequestTelemetrySnapshot
{
    [Key(0)]
    public Guid AircraftId { get; init; }

    [Key(1)]
    public List<string> Parameters { get; init; } = new();

}
