// Auto-generated code
namespace Simple.Shared.Messaging.Abstractions;

/// <summary>
/// Marker interface for events.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Gets the unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Gets the correlation ID for distributed tracing.
    /// </summary>
    string? CorrelationId { get; }
}
