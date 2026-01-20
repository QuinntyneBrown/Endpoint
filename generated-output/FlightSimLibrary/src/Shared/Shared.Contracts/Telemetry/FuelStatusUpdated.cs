// Auto-generated code
using MessagePack;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Contracts.Telemetry;

/// <summary>
/// Fired when fuel status changes
/// </summary>
[MessagePackObject]
public sealed class FuelStatusUpdated : EventBase
{
    [Key(0)]
    public Guid AircraftId { get; init; }

    [Key(1)]
    public double LeftTankQuantity { get; init; }

    [Key(2)]
    public double RightTankQuantity { get; init; }

    [Key(3)]
    public double TotalFuel { get; init; }

    [Key(4)]
    public double FuelFlowRate { get; init; }

    [Key(5)]
    public TimeSpan EstimatedEndurance { get; init; }

}
