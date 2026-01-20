// Auto-generated code
using MessagePack;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Contracts.Telemetry;

/// <summary>
/// Fired when engine parameters change
/// </summary>
[MessagePackObject]
public sealed class EngineParametersUpdated : EventBase
{
    [Key(0)]
    public Guid AircraftId { get; init; }

    [Key(1)]
    public int EngineNumber { get; init; }

    [Key(2)]
    public double Rpm { get; init; }

    [Key(3)]
    public double ManifoldPressure { get; init; }

    [Key(4)]
    public double FuelFlow { get; init; }

    [Key(5)]
    public double OilPressure { get; init; }

    [Key(6)]
    public double OilTemperature { get; init; }

    [Key(7)]
    public double CylinderHeadTemp { get; init; }

    [Key(8)]
    public double ExhaustGasTemp { get; init; }

}
