// Auto-generated code
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MCC.Shared.Shared.Messaging.Abstractions;

namespace MCC.Shared.Shared.Messaging.UdpMulticast;

/// <summary>
/// UDP multicast-based event bus implementation.
/// </summary>
public class UdpMulticastEventBus : IEventBus, IAsyncDisposable
{
    private readonly ILogger<UdpMulticastEventBus> _logger;
    private readonly UdpMulticastOptions _options;
    private readonly IMessageSerializer _serializer;
    private readonly UdpClient _sender;
    private readonly UdpClient _receiver;
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _receiveTask;
    private bool _disposed;

    public UdpMulticastEventBus(
        ILogger<UdpMulticastEventBus> logger,
        IOptions<UdpMulticastOptions> options,
        IMessageSerializer serializer)
    {
        _logger = logger;
        _options = options.Value;
        _serializer = serializer;

        // Setup sender
        _sender = new UdpClient();
        _sender.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, _options.Ttl);

        // Setup receiver
        _receiver = new UdpClient();
        _receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _receiver.Client.Bind(new IPEndPoint(IPAddress.Any, _options.Port));
        _receiver.JoinMulticastGroup(_options.MulticastGroupAddress);

        // Start receiving
        _receiveTask = ReceiveLoop(_cts.Token);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        var typeName = typeof(TEvent).AssemblyQualifiedName ?? typeof(TEvent).FullName ?? typeof(TEvent).Name;
        var typeNameBytes = System.Text.Encoding.UTF8.GetBytes(typeName);
        var eventData = _serializer.Serialize(@event);

        // Format: [4 bytes type name length][type name][event data]
        var packet = new byte[4 + typeNameBytes.Length + eventData.Length];
        BitConverter.GetBytes(typeNameBytes.Length).CopyTo(packet, 0);
        typeNameBytes.CopyTo(packet, 4);
        eventData.CopyTo(packet, 4 + typeNameBytes.Length);

        var endpoint = new IPEndPoint(_options.MulticastGroupAddress, _options.Port);

        _logger.LogDebug("Publishing {EventType} to {Endpoint}", typeof(TEvent).Name, endpoint);

        await _sender.SendAsync(packet, endpoint, cancellationToken);
    }

    public Task SubscribeAsync<TEvent>(Func<TEvent, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        _handlers.AddOrUpdate(
            typeof(TEvent),
            _ => new List<Delegate> { handler },
            (_, list) => { list.Add(handler); return list; });

        _logger.LogInformation("Subscribed to {EventType}", typeof(TEvent).Name);

        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync<TEvent>()
        where TEvent : class, IEvent
    {
        _handlers.TryRemove(typeof(TEvent), out _);
        _logger.LogInformation("Unsubscribed from {EventType}", typeof(TEvent).Name);
        return Task.CompletedTask;
    }

    private async Task ReceiveLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _receiver.ReceiveAsync(cancellationToken);
                var packet = result.Buffer;

                if (packet.Length < 4) continue;

                // Parse packet
                var typeNameLength = BitConverter.ToInt32(packet, 0);
                if (packet.Length < 4 + typeNameLength) continue;

                var typeName = System.Text.Encoding.UTF8.GetString(packet, 4, typeNameLength);
                var eventData = new byte[packet.Length - 4 - typeNameLength];
                Array.Copy(packet, 4 + typeNameLength, eventData, 0, eventData.Length);

                // Find type and handlers
                var eventType = Type.GetType(typeName);
                if (eventType == null) continue;

                if (_handlers.TryGetValue(eventType, out var handlers))
                {
                    var @event = _serializer.Deserialize(eventData, eventType);
                    if (@event != null)
                    {
                        foreach (var handler in handlers)
                        {
                            try
                            {
                                var invokeMethod = handler.GetType().GetMethod("Invoke");
                                if (invokeMethod != null)
                                {
                                    var task = invokeMethod.Invoke(handler, new[] { @event, cancellationToken }) as Task;
                                    if (task != null)
                                    {
                                        await task;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error handling event {EventType}", eventType.Name);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UDP receive loop");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        _cts.Cancel();

        if (_receiveTask != null)
        {
            try { await _receiveTask; } catch { }
        }

        _receiver.DropMulticastGroup(_options.MulticastGroupAddress);
        _receiver.Close();
        _sender.Close();
        _cts.Dispose();
    }
}
