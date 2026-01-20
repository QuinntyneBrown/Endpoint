// Auto-generated code
namespace Simple.Shared.Messaging.Abstractions;

/// <summary>
/// Interface for publishing and subscribing to events.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    /// <summary>
    /// Subscribes to events of a specific type.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="handler">The handler to invoke when an event is received.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent;

    /// <summary>
    /// Unsubscribes from events of a specific type.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    Task UnsubscribeAsync<TEvent>()
        where TEvent : class, IEvent;
}
