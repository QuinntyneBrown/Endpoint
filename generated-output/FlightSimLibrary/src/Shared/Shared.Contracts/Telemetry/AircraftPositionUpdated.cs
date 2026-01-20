// Auto-generated code
using MessagePack;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Contracts.Telemetry;

/// <summary>
/// Fired when aircraft position changes
/// </summary>
[MessagePackObject]
public sealed class AircraftPositionUpdated : EventBase
{
    [Key(0)]
    public Guid AircraftId { get; init; }

    [Key(1)]
    public double Latitude { get; init; }

    [Key(2)]
    public double Longitude { get; init; }

    /// <summary>
    /// Altitude in feet MSL
    /// </summary>
    [Key(3)]
    public double Altitude { get; init; }

    /// <summary>
    /// Heading in degrees (0-360)
    /// </summary>
    [Key(4)]
    public double Heading { get; init; }

    /// <summary>
    /// Ground speed in knots
    /// </summary>
    [Key(5)]
    public double GroundSpeed { get; init; }

    /// <summary>
    /// Vertical speed in feet per minute
    /// </summary>
    [Key(6)]
    public double VerticalSpeed { get; init; }

    [Key(7)]
    public DateTimeOffset Timestamp { get; init; }

}
