// Auto-generated code
using MessagePack;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Contracts.FlightPlan;

/// <summary>
/// Fired when aircraft reaches a waypoint
/// </summary>
[MessagePackObject]
public sealed class WaypointReached : EventBase
{
    [Key(0)]
    public Guid FlightPlanId { get; init; }

    [Key(1)]
    public Guid AircraftId { get; init; }

    [Key(2)]
    public string WaypointName { get; init; } = string.Empty;

    [Key(3)]
    public int WaypointIndex { get; init; }

    [Key(4)]
    public DateTimeOffset ArrivalTime { get; init; }

    [Key(5)]
    public string NextWaypointName { get; init; } = string.Empty;

    [Key(6)]
    public double DistanceToNext { get; init; }

}
