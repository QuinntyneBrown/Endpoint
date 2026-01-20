// Auto-generated code
using MessagePack;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Contracts.FlightPlan;

/// <summary>
/// Fired when a flight plan is filed
/// </summary>
[MessagePackObject]
public sealed class FlightPlanFiled : EventBase
{
    [Key(0)]
    public Guid FlightPlanId { get; init; }

    [Key(1)]
    public Guid AircraftId { get; init; }

    [Key(2)]
    public string DepartureAirport { get; init; } = string.Empty;

    [Key(3)]
    public string ArrivalAirport { get; init; } = string.Empty;

    [Key(4)]
    public List<string> Route { get; init; } = new();

    [Key(5)]
    public int CruiseAltitude { get; init; }

    [Key(6)]
    public DateTimeOffset EstimatedDepartureTime { get; init; }

    [Key(7)]
    public TimeSpan EstimatedEnrouteTime { get; init; }

}
