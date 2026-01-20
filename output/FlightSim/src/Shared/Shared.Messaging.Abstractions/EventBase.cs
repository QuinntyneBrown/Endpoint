// Auto-generated code
namespace FlightSim.Shared.Messaging.Abstractions;

/// <summary>
/// Base class for events providing common functionality.
/// </summary>
public abstract class EventBase : IEvent
{
    /// <inheritdoc />
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <inheritdoc />
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public string? CorrelationId { get; init; }
}
