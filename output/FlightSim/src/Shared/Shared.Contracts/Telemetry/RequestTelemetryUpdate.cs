// Auto-generated code
using MessagePack;

namespace FlightSim.Shared.Contracts.Telemetry;

/// <summary>
/// Request immediate telemetry update
/// </summary>
[MessagePackObject]
public sealed class RequestTelemetryUpdate
{
    [Key(0)]
    public Guid AircraftId { get; init; }

    [Key(1)]
    public List<string> Parameters { get; init; } = new();

}
