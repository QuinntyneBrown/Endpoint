// Auto-generated code
using MessagePack;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Contracts.Navigation;

/// <summary>
/// Fired when aircraft reaches a waypoint
/// </summary>
[MessagePackObject]
public sealed class WaypointReached : EventBase
{
    [Key(0)]
    public Guid AircraftId { get; init; }

    [Key(1)]
    public string WaypointId { get; init; } = string.Empty;

    [Key(2)]
    public string NextWaypointId { get; init; } = string.Empty;

    /// <summary>
    /// Distance in nautical miles
    /// </summary>
    [Key(3)]
    public double DistanceToNext { get; init; }

}
