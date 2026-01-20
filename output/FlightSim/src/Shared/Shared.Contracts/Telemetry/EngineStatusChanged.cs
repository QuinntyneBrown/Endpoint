// Auto-generated code
using MessagePack;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Contracts.Telemetry;

/// <summary>
/// Fired when engine status changes
/// </summary>
[MessagePackObject]
public sealed class EngineStatusChanged : EventBase
{
    [Key(0)]
    public Guid AircraftId { get; init; }

    [Key(1)]
    public int EngineNumber { get; init; }

    [Key(2)]
    public double Rpm { get; init; }

    [Key(3)]
    public double FuelFlow { get; init; }

    [Key(4)]
    public double OilPressure { get; init; }

    [Key(5)]
    public double OilTemperature { get; init; }

}
