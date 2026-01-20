// Auto-generated code
using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Messaging.AzureServiceBus;

/// <summary>
/// Azure Service Bus-based event bus implementation.
/// </summary>
public class AzureServiceBusEventBus : IEventBus, IAsyncDisposable
{
    private readonly ILogger<AzureServiceBusEventBus> _logger;
    private readonly AzureServiceBusOptions _options;
    private readonly IMessageSerializer _serializer;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ConcurrentDictionary<Type, ServiceBusProcessor> _processors = new();
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private bool _disposed;

    public AzureServiceBusEventBus(
        ILogger<AzureServiceBusEventBus> logger,
        IOptions<AzureServiceBusOptions> options,
        IMessageSerializer serializer)
    {
        _logger = logger;
        _options = options.Value;
        _serializer = serializer;
        _client = new ServiceBusClient(_options.ConnectionString);
        _sender = _client.CreateSender(_options.TopicName);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        var data = _serializer.Serialize(@event);
        var message = new ServiceBusMessage(data)
        {
            ContentType = "application/x-msgpack",
            Subject = typeof(TEvent).Name,
            MessageId = @event.EventId.ToString(),
            CorrelationId = @event.CorrelationId ?? string.Empty,
        };

        _logger.LogDebug("Publishing {EventType} to topic {Topic}", typeof(TEvent).Name, _options.TopicName);

        await _sender.SendMessageAsync(message, cancellationToken);
    }

    public async Task SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        _handlers.AddOrUpdate(
            typeof(TEvent),
            _ => new List<Delegate> { handler },
            (_, list) => { list.Add(handler); return list; });

        if (!_processors.ContainsKey(typeof(TEvent)))
        {
            var processor = _client.CreateProcessor(
                _options.TopicName,
                _options.SubscriptionName,
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 10,
                });

            processor.ProcessMessageAsync += async args =>
            {
                if (args.Message.Subject == typeof(TEvent).Name)
                {
                    try
                    {
                        var @event = _serializer.Deserialize<TEvent>(args.Message.Body.ToArray());
                        if (@event != null && _handlers.TryGetValue(typeof(TEvent), out var handlers))
                        {
                            foreach (var h in handlers)
                            {
                                if (h is Func<TEvent, CancellationToken, Task> typedHandler)
                                {
                                    await typedHandler(@event, args.CancellationToken);
                                }
                            }
                        }

                        await args.CompleteMessageAsync(args.Message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling event {EventType}", typeof(TEvent).Name);
                        await args.AbandonMessageAsync(args.Message);
                    }
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Error processing Service Bus messages");
                return Task.CompletedTask;
            };

            _processors[typeof(TEvent)] = processor;
            await processor.StartProcessingAsync(cancellationToken);

            _logger.LogInformation("Subscribed to {EventType} on topic {Topic}", typeof(TEvent).Name, _options.TopicName);
        }
    }

    public async Task UnsubscribeAsync<TEvent>()
        where TEvent : class, IEvent
    {
        _handlers.TryRemove(typeof(TEvent), out _);

        if (_processors.TryRemove(typeof(TEvent), out var processor))
        {
            await processor.StopProcessingAsync();
            await processor.DisposeAsync();
        }

        _logger.LogInformation("Unsubscribed from {EventType}", typeof(TEvent).Name);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var processor in _processors.Values)
        {
            await processor.StopProcessingAsync();
            await processor.DisposeAsync();
        }

        _processors.Clear();
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}
