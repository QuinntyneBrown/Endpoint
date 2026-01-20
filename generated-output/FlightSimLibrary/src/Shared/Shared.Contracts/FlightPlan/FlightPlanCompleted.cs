// Auto-generated code
using MessagePack;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Contracts.FlightPlan;

/// <summary>
/// Fired when flight plan is completed
/// </summary>
[MessagePackObject]
public sealed class FlightPlanCompleted : EventBase
{
    [Key(0)]
    public Guid FlightPlanId { get; init; }

    [Key(1)]
    public Guid AircraftId { get; init; }

    [Key(2)]
    public DateTimeOffset CompletedAt { get; init; }

    [Key(3)]
    public TimeSpan ActualEnrouteTime { get; init; }

}
