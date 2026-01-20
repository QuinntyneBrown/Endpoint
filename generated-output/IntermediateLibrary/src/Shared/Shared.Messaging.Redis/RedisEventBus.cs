// Auto-generated code
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ECommerce.Shared.Messaging.Abstractions;

namespace ECommerce.Shared.Messaging.Redis;

/// <summary>
/// Redis-based event bus implementation.
/// </summary>
public class RedisEventBus : IEventBus, IAsyncDisposable
{
    private readonly ILogger<RedisEventBus> _logger;
    private readonly RedisEventBusOptions _options;
    private readonly IMessageSerializer _serializer;
    private readonly ConnectionMultiplexer _connection;
    private readonly ISubscriber _subscriber;
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private bool _disposed;

    public RedisEventBus(
        ILogger<RedisEventBus> logger,
        IOptions<RedisEventBusOptions> options,
        IMessageSerializer serializer)
    {
        _logger = logger;
        _options = options.Value;
        _serializer = serializer;
        _connection = ConnectionMultiplexer.Connect(_options.ConnectionString);
        _subscriber = _connection.GetSubscriber();
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        var channel = GetChannel<TEvent>();
        var data = _serializer.Serialize(@event);

        _logger.LogDebug("Publishing {EventType} to channel {Channel}", typeof(TEvent).Name, channel);

        await _subscriber.PublishAsync(RedisChannel.Literal(channel), data);
    }

    public async Task SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        var channel = GetChannel<TEvent>();

        _handlers.AddOrUpdate(
            typeof(TEvent),
            _ => new List<Delegate> { handler },
            (_, list) => { list.Add(handler); return list; });

        _logger.LogInformation("Subscribing to {EventType} on channel {Channel}", typeof(TEvent).Name, channel);

        await _subscriber.SubscribeAsync(RedisChannel.Literal(channel), async (_, message) =>
        {
            try
            {
                var @event = _serializer.Deserialize<TEvent>(message!);
                if (@event != null)
                {
                    await handler(@event, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling event {EventType}", typeof(TEvent).Name);
            }
        });
    }

    public async Task UnsubscribeAsync<TEvent>()
        where TEvent : class, IEvent
    {
        var channel = GetChannel<TEvent>();
        _handlers.TryRemove(typeof(TEvent), out _);

        _logger.LogInformation("Unsubscribing from {EventType} on channel {Channel}", typeof(TEvent).Name, channel);

        await _subscriber.UnsubscribeAsync(RedisChannel.Literal(channel));
    }

    private string GetChannel<TEvent>() =>
        $"{_options.ChannelPrefix}:{typeof(TEvent).Name}";

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await _connection.CloseAsync();
        _connection.Dispose();
    }
}
