# Efficient Message Routing for Selective Subscription in Redis Pub/Sub

## Problem Statement

With 3 microservices, 100 message types, and each service only needing ~10 messages, you need a routing strategy that:
- Avoids subscribing to unnecessary channels
- Minimizes deserialization of irrelevant messages
- Handles cyclical event patterns efficiently
- Provides clean worker service integration

---

## 1. Routing Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Message Flow Architecture                          │
└─────────────────────────────────────────────────────────────────────────────┘

                              Redis Pub/Sub
                                    │
        ┌───────────────────────────┼───────────────────────────┐
        │                           │                           │
        ▼                           ▼                           ▼
┌───────────────────┐   ┌───────────────────┐   ┌───────────────────┐
│  Inventory Svc    │   │  Inspection Svc   │   │  Notifications    │
│                   │   │                   │   │                   │
│  Subscribes to:   │   │  Subscribes to:   │   │  Subscribes to:   │
│  • vehicles.*     │   │  • inspections.*  │   │  • vehicles.listed│
│  • dealers.*      │   │  • vehicles.listed│   │  • inspections.*  │
│                   │   │  • scheduling.*   │   │  • customers.*    │
│  ~12 messages     │   │  ~10 messages     │   │  ~15 messages     │
└───────────────────┘   └───────────────────┘   └───────────────────┘

Strategy: Channel-per-domain with selective pattern subscriptions
```

---

## 2. Channel Naming Convention

### 2.1 Hierarchical Channel Structure

```csharp
// Channel naming follows: {domain}.{aggregate}.{event-type}.v{version}
// Examples:
//   vehicles.listing.created.v1
//   vehicles.listing.updated.v1
//   vehicles.listing.sold.v1
//   inspections.report.completed.v1
//   inspections.report.failed.v1
//   dealers.account.verified.v1

public static class ChannelNaming
{
    // Pattern: {domain}.{aggregate}.{action}.v{version}
    public static string GetChannel<T>() where T : IMessage
    {
        var attr = typeof(T).GetCustomAttribute<MessageChannelAttribute>();
        if (attr != null) return attr.Channel;
        
        // Convention-based fallback
        var name = typeof(T).Name;
        
        // VehicleListedEvent -> vehicles.listing.listed.v1
        // InspectionCompletedEvent -> inspections.report.completed.v1
        return ConvertToChannel(name);
    }
    
    public static string GetDomainPattern(string domain) => $"{domain}.*";
    
    public static string GetAggregatePattern(string domain, string aggregate) 
        => $"{domain}.{aggregate}.*";
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class MessageChannelAttribute : Attribute
{
    public string Channel { get; }
    public string Domain { get; }
    public string Aggregate { get; }
    
    public MessageChannelAttribute(string domain, string aggregate, string action, int version = 1)
    {
        Domain = domain;
        Aggregate = aggregate;
        Channel = $"{domain}.{aggregate}.{action}.v{version}";
    }
}

// Usage in generated messages
[MessagePackObject]
[MessageChannel("vehicles", "listing", "created", version: 1)]
public sealed class VehicleListedEvent : IDomainEvent { /* ... */ }

[MessagePackObject]
[MessageChannel("inspections", "report", "completed", version: 1)]
public sealed class InspectionCompletedEvent : IDomainEvent { /* ... */ }
```

---

## 3. Subscription Registry Pattern

### 3.1 Declarative Subscription Configuration

```csharp
// Each microservice declares its subscriptions at startup
public interface ISubscriptionRegistry
{
    IReadOnlyList<SubscriptionDescriptor> GetSubscriptions();
    bool ShouldHandle(string messageType);
    Type? GetHandlerType(string messageType);
}

public sealed class SubscriptionDescriptor
{
    public required string ChannelPattern { get; init; }
    public required Type MessageType { get; init; }
    public required Type HandlerType { get; init; }
    public int MaxConcurrency { get; init; } = 1;
    public RetryPolicy RetryPolicy { get; init; } = RetryPolicy.Default;
}

public sealed class RetryPolicy
{
    public int MaxRetries { get; init; } = 3;
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromSeconds(1);
    public double BackoffMultiplier { get; init; } = 2.0;
    
    public static RetryPolicy Default => new();
    public static RetryPolicy None => new() { MaxRetries = 0 };
}

// Fluent builder for service-specific subscriptions
public sealed class SubscriptionRegistryBuilder
{
    private readonly List<SubscriptionDescriptor> _subscriptions = new();
    private readonly string _serviceName;

    public SubscriptionRegistryBuilder(string serviceName)
    {
        _serviceName = serviceName;
    }

    public SubscriptionRegistryBuilder Subscribe<TMessage, THandler>(
        Action<SubscriptionOptions>? configure = null)
        where TMessage : IMessage
        where THandler : IMessageHandler<TMessage>
    {
        var options = new SubscriptionOptions();
        configure?.Invoke(options);
        
        var channel = ChannelNaming.GetChannel<TMessage>();
        
        _subscriptions.Add(new SubscriptionDescriptor
        {
            ChannelPattern = channel,
            MessageType = typeof(TMessage),
            HandlerType = typeof(THandler),
            MaxConcurrency = options.MaxConcurrency,
            RetryPolicy = options.RetryPolicy
        });
        
        return this;
    }

    public SubscriptionRegistryBuilder SubscribeToDomain<THandler>(
        string domain,
        params Type[] messageTypes)
        where THandler : IMessageHandler
    {
        foreach (var messageType in messageTypes)
        {
            var channel = ChannelNaming.GetChannel(messageType);
            _subscriptions.Add(new SubscriptionDescriptor
            {
                ChannelPattern = channel,
                MessageType = messageType,
                HandlerType = typeof(THandler)
            });
        }
        return this;
    }

    public ISubscriptionRegistry Build() => new SubscriptionRegistry(_subscriptions);
}

public sealed class SubscriptionOptions
{
    public int MaxConcurrency { get; set; } = 1;
    public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.Default;
}
```

### 3.2 Service-Specific Registration Examples

```csharp
// Inventory Service - subscribes to vehicle and dealer events
public static class InventoryServiceSubscriptions
{
    public static ISubscriptionRegistry Configure()
    {
        return new SubscriptionRegistryBuilder("inventory-service")
            // Vehicle domain events
            .Subscribe<VehicleListedEvent, VehicleListedHandler>(opt => 
            {
                opt.MaxConcurrency = 4;  // High volume expected
            })
            .Subscribe<VehicleUpdatedEvent, VehicleUpdatedHandler>()
            .Subscribe<VehicleSoldEvent, VehicleSoldHandler>()
            .Subscribe<VehicleArchivedEvent, VehicleArchivedHandler>()
            
            // Dealer domain events
            .Subscribe<DealerVerifiedEvent, DealerVerifiedHandler>()
            .Subscribe<DealerSuspendedEvent, DealerSuspendedHandler>()
            .Subscribe<DealerInventoryLimitChangedEvent, InventoryLimitHandler>()
            
            // Pricing events
            .Subscribe<PriceDropEvent, PriceDropHandler>()
            .Subscribe<PriceIncreaseEvent, PriceIncreaseHandler>()
            
            // Photo events
            .Subscribe<PhotosUploadedEvent, PhotosUploadedHandler>()
            .Subscribe<PhotosApprovedEvent, PhotosApprovedHandler>()
            .Subscribe<PhotosRejectedEvent, PhotosRejectedHandler>()
            
            .Build();
    }
}

// Inspection Service - subscribes to inspection scheduling and vehicle events
public static class InspectionServiceSubscriptions
{
    public static ISubscriptionRegistry Configure()
    {
        return new SubscriptionRegistryBuilder("inspection-service")
            // Core inspection events
            .Subscribe<InspectionRequestedEvent, InspectionRequestedHandler>(opt =>
            {
                opt.MaxConcurrency = 2;
                opt.RetryPolicy = new RetryPolicy { MaxRetries = 5 };
            })
            .Subscribe<InspectionScheduledEvent, InspectionScheduledHandler>()
            .Subscribe<InspectionStartedEvent, InspectionStartedHandler>()
            .Subscribe<InspectionCompletedEvent, InspectionCompletedHandler>()
            .Subscribe<InspectionCancelledEvent, InspectionCancelledHandler>()
            
            // Vehicle events needed for inspection context
            .Subscribe<VehicleListedEvent, VehicleContextHandler>()
            .Subscribe<VehicleUpdatedEvent, VehicleContextHandler>()
            
            // Inspector events
            .Subscribe<InspectorAssignedEvent, InspectorAssignedHandler>()
            .Subscribe<InspectorAvailabilityChangedEvent, AvailabilityHandler>()
            .Subscribe<InspectorLocationUpdatedEvent, LocationHandler>()
            
            .Build();
    }
}

// Notification Service - subscribes across domains for notification triggers
public static class NotificationServiceSubscriptions
{
    public static ISubscriptionRegistry Configure()
    {
        return new SubscriptionRegistryBuilder("notification-service")
            // Vehicle notifications
            .Subscribe<VehicleListedEvent, NewListingNotificationHandler>()
            .Subscribe<VehicleSoldEvent, SaleNotificationHandler>()
            .Subscribe<PriceDropEvent, PriceAlertHandler>(opt =>
            {
                opt.MaxConcurrency = 8;  // Price alerts are time-sensitive
            })
            
            // Inspection notifications
            .Subscribe<InspectionScheduledEvent, InspectionReminderHandler>()
            .Subscribe<InspectionCompletedEvent, InspectionResultHandler>()
            
            // Customer events
            .Subscribe<CustomerSavedSearchMatchEvent, SavedSearchHandler>()
            .Subscribe<CustomerWatchlistItemChangedEvent, WatchlistHandler>()
            .Subscribe<CustomerOfferReceivedEvent, OfferNotificationHandler>()
            .Subscribe<CustomerOfferAcceptedEvent, OfferAcceptedHandler>()
            .Subscribe<CustomerOfferRejectedEvent, OfferRejectedHandler>()
            
            // Dealer notifications
            .Subscribe<DealerLeadReceivedEvent, LeadNotificationHandler>()
            .Subscribe<DealerReviewPostedEvent, ReviewNotificationHandler>()
            
            // System notifications
            .Subscribe<SystemMaintenanceScheduledEvent, MaintenanceAlertHandler>()
            .Subscribe<FeatureAnnouncementEvent, FeatureAnnouncementHandler>()
            
            .Build();
    }
}
```

---

## 4. Message Handler Infrastructure

### 4.1 Handler Interfaces

```csharp
public interface IMessageHandler
{
    Task HandleAsync(object message, MessageContext context, CancellationToken ct);
}

public interface IMessageHandler<TMessage> : IMessageHandler 
    where TMessage : IMessage
{
    Task HandleAsync(TMessage message, MessageContext context, CancellationToken ct);
    
    // Default implementation for non-generic interface
    Task IMessageHandler.HandleAsync(object message, MessageContext context, CancellationToken ct)
        => HandleAsync((TMessage)message, context, ct);
}

public sealed class MessageContext
{
    public required MessageHeader Header { get; init; }
    public required string Channel { get; init; }
    public required DateTimeOffset ReceivedAt { get; init; }
    public int RetryCount { get; init; }
    public IServiceProvider Services { get; init; } = null!;
    
    // For distributed tracing
    public Activity? Activity { get; init; }
}

// Base class with common functionality
public abstract class MessageHandlerBase<TMessage> : IMessageHandler<TMessage>
    where TMessage : IMessage
{
    protected ILogger Logger { get; }
    
    protected MessageHandlerBase(ILogger logger)
    {
        Logger = logger;
    }

    public abstract Task HandleAsync(TMessage message, MessageContext context, CancellationToken ct);
    
    protected void LogHandling(TMessage message, MessageContext context)
    {
        Logger.LogDebug(
            "Handling {MessageType} [{MessageId}] from {Channel}",
            typeof(TMessage).Name,
            context.Header.MessageId,
            context.Channel);
    }
}
```

### 4.2 Concrete Handler Examples

```csharp
// Simple handler
public sealed class VehicleListedHandler : MessageHandlerBase<VehicleListedEvent>
{
    private readonly IVehicleRepository _repository;
    private readonly ISearchIndexer _indexer;

    public VehicleListedHandler(
        IVehicleRepository repository,
        ISearchIndexer indexer,
        ILogger<VehicleListedHandler> logger) : base(logger)
    {
        _repository = repository;
        _indexer = indexer;
    }

    public override async Task HandleAsync(
        VehicleListedEvent message, 
        MessageContext context, 
        CancellationToken ct)
    {
        LogHandling(message, context);
        
        var vehicle = await _repository.GetByIdAsync(message.VehicleId, ct);
        if (vehicle == null)
        {
            // First time seeing this vehicle - create it
            vehicle = Vehicle.FromListedEvent(message);
            await _repository.AddAsync(vehicle, ct);
        }
        
        // Update search index
        await _indexer.IndexVehicleAsync(vehicle, ct);
    }
}

// Handler that consolidates multiple message types
public sealed class VehicleContextHandler : IMessageHandler<VehicleListedEvent>, 
                                             IMessageHandler<VehicleUpdatedEvent>
{
    private readonly IInspectionContextCache _cache;
    private readonly ILogger<VehicleContextHandler> _logger;

    public VehicleContextHandler(
        IInspectionContextCache cache,
        ILogger<VehicleContextHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task HandleAsync(VehicleListedEvent message, MessageContext context, CancellationToken ct)
    {
        await _cache.UpdateVehicleContextAsync(message.VehicleId, v =>
        {
            v.Vin = message.Vin;
            v.Year = message.Year;
            v.Make = message.Make;
            v.Model = message.Model;
            v.Mileage = message.Mileage;
        }, ct);
    }

    public async Task HandleAsync(VehicleUpdatedEvent message, MessageContext context, CancellationToken ct)
    {
        await _cache.UpdateVehicleContextAsync(message.VehicleId, v =>
        {
            if (message.Mileage.HasValue) v.Mileage = message.Mileage.Value;
            if (message.Condition.HasValue) v.Condition = message.Condition.Value;
        }, ct);
    }
}

// Handler with idempotency
public sealed class InspectionCompletedHandler : MessageHandlerBase<InspectionCompletedEvent>
{
    private readonly IInspectionRepository _repository;
    private readonly IIdempotencyStore _idempotency;

    public InspectionCompletedHandler(
        IInspectionRepository repository,
        IIdempotencyStore idempotency,
        ILogger<InspectionCompletedHandler> logger) : base(logger)
    {
        _repository = repository;
        _idempotency = idempotency;
    }

    public override async Task HandleAsync(
        InspectionCompletedEvent message, 
        MessageContext context, 
        CancellationToken ct)
    {
        // Idempotency check - critical for cyclical events
        var idempotencyKey = $"inspection-completed:{message.InspectionId}:{context.Header.MessageId}";
        if (await _idempotency.HasProcessedAsync(idempotencyKey, ct))
        {
            Logger.LogDebug("Skipping duplicate message {MessageId}", context.Header.MessageId);
            return;
        }
        
        LogHandling(message, context);
        
        var inspection = await _repository.GetByIdAsync(message.InspectionId, ct);
        if (inspection == null)
        {
            throw new InvalidOperationException($"Inspection {message.InspectionId} not found");
        }
        
        inspection.MarkCompleted(message.Result, message.Items, message.CompletedAt);
        await _repository.UpdateAsync(inspection, ct);
        
        await _idempotency.MarkProcessedAsync(idempotencyKey, TimeSpan.FromDays(7), ct);
    }
}
```

---

## 5. Worker Service Implementation

### 5.1 Background Worker with Channel Multiplexing

```csharp
public sealed class MessageSubscriberWorker : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ISubscriptionRegistry _subscriptions;
    private readonly IMessageSerializer _serializer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MessageSubscriberWorker> _logger;
    private readonly MessageSubscriberOptions _options;
    private readonly ActivitySource _activitySource;
    
    // Channel processors for concurrent handling
    private readonly ConcurrentDictionary<string, Channel<RedisValue>> _messageQueues = new();
    private readonly List<Task> _processorTasks = new();

    public MessageSubscriberWorker(
        IConnectionMultiplexer redis,
        ISubscriptionRegistry subscriptions,
        IMessageSerializer serializer,
        IServiceScopeFactory scopeFactory,
        IOptions<MessageSubscriberOptions> options,
        ILogger<MessageSubscriberWorker> logger)
    {
        _redis = redis;
        _subscriptions = subscriptions;
        _serializer = serializer;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
        _activitySource = new ActivitySource("Automotive.Messaging.Consumer");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();
        var subscriptionsByChannel = _subscriptions.GetSubscriptions()
            .GroupBy(s => s.ChannelPattern)
            .ToDictionary(g => g.Key, g => g.ToList());

        _logger.LogInformation(
            "Starting message subscriber with {Count} channel subscriptions",
            subscriptionsByChannel.Count);

        // Create processing channels and start processors
        foreach (var (channel, descriptors) in subscriptionsByChannel)
        {
            var maxConcurrency = descriptors.Max(d => d.MaxConcurrency);
            var messageChannel = Channel.CreateBounded<RedisValue>(new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = maxConcurrency == 1,
                SingleWriter = true
            });
            
            _messageQueues[channel] = messageChannel;
            
            // Start processor tasks based on concurrency
            for (int i = 0; i < maxConcurrency; i++)
            {
                var task = ProcessMessagesAsync(channel, descriptors, messageChannel.Reader, stoppingToken);
                _processorTasks.Add(task);
            }
        }

        // Subscribe to Redis channels
        foreach (var channelPattern in subscriptionsByChannel.Keys)
        {
            await subscriber.SubscribeAsync(
                RedisChannel.Literal(channelPattern),
                (redisChannel, message) =>
                {
                    if (_messageQueues.TryGetValue(channelPattern, out var queue))
                    {
                        // Non-blocking write - if full, will wait
                        queue.Writer.TryWrite(message);
                    }
                });
            
            _logger.LogDebug("Subscribed to channel: {Channel}", channelPattern);
        }

        // Wait for cancellation
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Shutting down message subscriber");
        }

        // Complete all channels
        foreach (var queue in _messageQueues.Values)
        {
            queue.Writer.Complete();
        }

        // Wait for processors to finish
        await Task.WhenAll(_processorTasks);
    }

    private async Task ProcessMessagesAsync(
        string channelPattern,
        List<SubscriptionDescriptor> descriptors,
        ChannelReader<RedisValue> reader,
        CancellationToken ct)
    {
        await foreach (var message in reader.ReadAllAsync(ct))
        {
            try
            {
                await ProcessSingleMessageAsync(channelPattern, descriptors, message, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error processing message from {Channel}", channelPattern);
            }
        }
    }

    private async Task ProcessSingleMessageAsync(
        string channelPattern,
        List<SubscriptionDescriptor> descriptors,
        RedisValue message,
        CancellationToken ct)
    {
        // Peek header to determine message type
        var headerInfo = _serializer.PeekHeader(message);
        if (headerInfo == null)
        {
            _logger.LogWarning("Could not deserialize message header from {Channel}", channelPattern);
            return;
        }

        var (header, payloadType) = headerInfo.Value;
        
        // Find matching descriptor
        var descriptor = descriptors.FirstOrDefault(d => d.MessageType == payloadType);
        if (descriptor == null)
        {
            _logger.LogDebug(
                "No handler registered for {MessageType} on {Channel}",
                header.MessageType, channelPattern);
            return;
        }

        using var activity = _activitySource.StartActivity(
            $"Process {header.MessageType}",
            ActivityKind.Consumer);
        
        activity?.SetTag("messaging.message_id", header.MessageId);
        activity?.SetTag("messaging.destination", channelPattern);

        var context = new MessageContext
        {
            Header = header,
            Channel = channelPattern,
            ReceivedAt = DateTimeOffset.UtcNow,
            Activity = activity
        };

        await ExecuteWithRetryAsync(descriptor, message, context, ct);
    }

    private async Task ExecuteWithRetryAsync(
        SubscriptionDescriptor descriptor,
        ReadOnlyMemory<byte> messageBytes,
        MessageContext context,
        CancellationToken ct)
    {
        var retryPolicy = descriptor.RetryPolicy;
        var attempt = 0;
        var delay = retryPolicy.InitialDelay;

        while (true)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                
                // Deserialize the full message
                var deserializeMethod = typeof(IMessageSerializer)
                    .GetMethod(nameof(IMessageSerializer.Deserialize))!
                    .MakeGenericMethod(descriptor.MessageType);
                
                var envelope = deserializeMethod.Invoke(_serializer, new object[] { messageBytes });
                if (envelope == null) return;
                
                // Get payload via reflection (or use a non-generic interface)
                var payload = envelope.GetType().GetProperty("Payload")!.GetValue(envelope);
                
                // Resolve and execute handler
                var handler = (IMessageHandler)scope.ServiceProvider.GetRequiredService(descriptor.HandlerType);
                
                context = context with { Services = scope.ServiceProvider, RetryCount = attempt };
                
                await handler.HandleAsync(payload!, context, ct);
                return;
            }
            catch (Exception ex) when (attempt < retryPolicy.MaxRetries)
            {
                attempt++;
                _logger.LogWarning(ex,
                    "Handler failed for {MessageType}, attempt {Attempt}/{MaxRetries}. Retrying in {Delay}",
                    descriptor.MessageType.Name, attempt, retryPolicy.MaxRetries, delay);
                
                await Task.Delay(delay, ct);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * retryPolicy.BackoffMultiplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Handler failed for {MessageType} after {Attempts} attempts",
                    descriptor.MessageType.Name, attempt + 1);
                
                // Consider dead-letter queue here
                throw;
            }
        }
    }
}

public sealed class MessageSubscriberOptions
{
    public string ServiceName { get; set; } = "unknown";
    public int DefaultMaxConcurrency { get; set; } = 1;
    public int MessageQueueCapacity { get; set; } = 1000;
}
```

### 5.2 Optimized Message Router (Alternative Pattern)

For scenarios where you want to minimize Redis subscriptions by using patterns:

```csharp
public sealed class PatternBasedMessageRouter : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ISubscriptionRegistry _subscriptions;
    private readonly IMessageSerializer _serializer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PatternBasedMessageRouter> _logger;
    
    // Pre-computed routing table for O(1) lookup
    private readonly FrozenDictionary<string, SubscriptionDescriptor> _routingTable;
    private readonly FrozenSet<string> _subscribedMessageTypes;

    public PatternBasedMessageRouter(
        IConnectionMultiplexer redis,
        ISubscriptionRegistry subscriptions,
        IMessageSerializer serializer,
        IServiceScopeFactory scopeFactory,
        ILogger<PatternBasedMessageRouter> logger)
    {
        _redis = redis;
        _subscriptions = subscriptions;
        _serializer = serializer;
        _scopeFactory = scopeFactory;
        _logger = logger;
        
        // Build frozen routing table at startup
        var routing = subscriptions.GetSubscriptions()
            .ToDictionary(
                s => GetMessageTypeString(s.MessageType),
                s => s);
        _routingTable = routing.ToFrozenDictionary();
        _subscribedMessageTypes = routing.Keys.ToFrozenSet();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();
        
        // Subscribe to domain patterns only (reduces subscription count)
        var domains = _subscriptions.GetSubscriptions()
            .Select(s => GetDomain(s.ChannelPattern))
            .Distinct()
            .ToList();

        _logger.LogInformation(
            "Subscribing to {DomainCount} domain patterns for {MessageCount} message types",
            domains.Count,
            _routingTable.Count);

        foreach (var domain in domains)
        {
            await subscriber.SubscribeAsync(
                RedisChannel.Pattern($"{domain}.*"),
                async (channel, message) =>
                {
                    await RouteMessageAsync(channel!, message, stoppingToken);
                });
            
            _logger.LogDebug("Subscribed to domain pattern: {Domain}.*", domain);
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task RouteMessageAsync(string channel, RedisValue message, CancellationToken ct)
    {
        // Quick peek at message type
        var headerInfo = _serializer.PeekHeader(message);
        if (headerInfo == null) return;

        var (header, _) = headerInfo.Value;
        
        // O(1) lookup - skip if we don't handle this message type
        if (!_subscribedMessageTypes.Contains(header.MessageType))
        {
            return;  // Early exit - we don't care about this message
        }

        if (!_routingTable.TryGetValue(header.MessageType, out var descriptor))
        {
            return;
        }

        // Only now do we fully deserialize and process
        await ProcessMessageAsync(descriptor, header, message, channel, ct);
    }

    private async Task ProcessMessageAsync(
        SubscriptionDescriptor descriptor,
        MessageHeader header,
        ReadOnlyMemory<byte> messageBytes,
        string channel,
        CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        
        try
        {
            var envelope = DeserializeEnvelope(descriptor.MessageType, messageBytes);
            if (envelope == null) return;

            var payload = GetPayload(envelope);
            var handler = (IMessageHandler)scope.ServiceProvider.GetRequiredService(descriptor.HandlerType);
            
            var context = new MessageContext
            {
                Header = header,
                Channel = channel,
                ReceivedAt = DateTimeOffset.UtcNow,
                Services = scope.ServiceProvider
            };

            await handler.HandleAsync(payload, context, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {MessageType}", header.MessageType);
        }
    }

    private static string GetDomain(string channelPattern)
    {
        var dotIndex = channelPattern.IndexOf('.');
        return dotIndex > 0 ? channelPattern[..dotIndex] : channelPattern;
    }

    private static string GetMessageTypeString(Type type)
    {
        var attr = type.GetCustomAttribute<MessageChannelAttribute>();
        return attr?.Channel ?? type.Name.ToLowerInvariant();
    }
    
    // ... deserialization helpers
}
```

---

## 6. Handling Cyclical Events

### 6.1 Cycle Detection and Breaking

```csharp
public interface ICycleDetector
{
    bool IsInCycle(MessageHeader header);
    void RecordProcessing(MessageHeader header);
}

public sealed class CycleDetector : ICycleDetector
{
    private readonly IDistributedCache _cache;
    private readonly CycleDetectorOptions _options;

    public CycleDetector(IDistributedCache cache, IOptions<CycleDetectorOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public bool IsInCycle(MessageHeader header)
    {
        // Check if we've seen this correlation chain too many times
        var key = $"cycle:{header.CorrelationId}:{header.MessageType}";
        var countBytes = _cache.Get(key);
        
        if (countBytes == null) return false;
        
        var count = BitConverter.ToInt32(countBytes);
        return count >= _options.MaxCycleDepth;
    }

    public void RecordProcessing(MessageHeader header)
    {
        var key = $"cycle:{header.CorrelationId}:{header.MessageType}";
        var countBytes = _cache.Get(key);
        var count = countBytes == null ? 0 : BitConverter.ToInt32(countBytes);
        
        _cache.Set(
            key, 
            BitConverter.GetBytes(count + 1),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _options.CycleWindowDuration
            });
    }
}

public sealed class CycleDetectorOptions
{
    public int MaxCycleDepth { get; set; } = 10;
    public TimeSpan CycleWindowDuration { get; set; } = TimeSpan.FromMinutes(5);
}

// Decorator for handlers that participate in cycles
public sealed class CycleAwareHandlerDecorator<TMessage> : IMessageHandler<TMessage>
    where TMessage : IMessage
{
    private readonly IMessageHandler<TMessage> _inner;
    private readonly ICycleDetector _cycleDetector;
    private readonly ILogger<CycleAwareHandlerDecorator<TMessage>> _logger;

    public CycleAwareHandlerDecorator(
        IMessageHandler<TMessage> inner,
        ICycleDetector cycleDetector,
        ILogger<CycleAwareHandlerDecorator<TMessage>> logger)
    {
        _inner = inner;
        _cycleDetector = cycleDetector;
        _logger = logger;
    }

    public async Task HandleAsync(TMessage message, MessageContext context, CancellationToken ct)
    {
        if (_cycleDetector.IsInCycle(context.Header))
        {
            _logger.LogWarning(
                "Cycle detected for {MessageType} in correlation {CorrelationId}. Breaking cycle.",
                typeof(TMessage).Name,
                context.Header.CorrelationId);
            return;
        }

        _cycleDetector.RecordProcessing(context.Header);
        await _inner.HandleAsync(message, context, ct);
    }
}
```

### 6.2 Causation Chain Tracking

```csharp
// Enhanced publisher that maintains causation chain
public sealed class CausationAwarePublisher : IMessagePublisher
{
    private readonly IMessagePublisher _inner;
    private readonly IMessageContextAccessor _contextAccessor;

    public CausationAwarePublisher(
        IMessagePublisher inner,
        IMessageContextAccessor contextAccessor)
    {
        _inner = inner;
        _contextAccessor = contextAccessor;
    }

    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : IMessage
    {
        // If we're inside a message handler, propagate the causation chain
        var currentContext = _contextAccessor.Current;
        
        if (currentContext != null)
        {
            // The current message becomes the cause of the new message
            var causationId = currentContext.Header.MessageId;
            var correlationId = currentContext.Header.CorrelationId;
            
            await _inner.PublishWithCausationAsync(message, correlationId, causationId, ct);
        }
        else
        {
            await _inner.PublishAsync(message, ct);
        }
    }
}

// Context accessor for tracking current message being processed
public interface IMessageContextAccessor
{
    MessageContext? Current { get; set; }
}

public sealed class MessageContextAccessor : IMessageContextAccessor
{
    private static readonly AsyncLocal<MessageContext?> _current = new();
    
    public MessageContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}
```

### 6.3 Idempotency Store for Exactly-Once Processing

```csharp
public interface IIdempotencyStore
{
    Task<bool> HasProcessedAsync(string key, CancellationToken ct = default);
    Task MarkProcessedAsync(string key, TimeSpan retention, CancellationToken ct = default);
    Task<bool> TryProcessAsync(string key, TimeSpan retention, CancellationToken ct = default);
}

public sealed class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _prefix;

    public RedisIdempotencyStore(IConnectionMultiplexer redis, string servicePrefix)
    {
        _redis = redis;
        _prefix = $"idempotency:{servicePrefix}:";
    }

    public async Task<bool> HasProcessedAsync(string key, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(_prefix + key);
    }

    public async Task MarkProcessedAsync(string key, TimeSpan retention, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(_prefix + key, "1", retention);
    }

    public async Task<bool> TryProcessAsync(string key, TimeSpan retention, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        // Atomic check-and-set
        return await db.StringSetAsync(_prefix + key, "1", retention, When.NotExists);
    }
}

// Usage in handler
public sealed class IdempotentInspectionHandler : MessageHandlerBase<InspectionCompletedEvent>
{
    private readonly IIdempotencyStore _idempotency;
    private readonly IInspectionService _service;

    public override async Task HandleAsync(
        InspectionCompletedEvent message, 
        MessageContext context, 
        CancellationToken ct)
    {
        // Composite key: message type + aggregate ID + message ID
        var idempotencyKey = $"inspection-completed:{message.InspectionId}:{context.Header.MessageId}";
        
        // Atomic try-process - returns false if already processed
        if (!await _idempotency.TryProcessAsync(idempotencyKey, TimeSpan.FromDays(7), ct))
        {
            Logger.LogDebug("Message {MessageId} already processed", context.Header.MessageId);
            return;
        }

        await _service.CompleteInspectionAsync(message, ct);
    }
}
```

---

## 7. Dependency Injection Setup

### 7.1 Service Registration

```csharp
public static class MessagingServiceExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SubscriptionRegistryBuilder> configureSubscriptions)
    {
        // Core infrastructure
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
        
        services.AddSingleton<IMessageSerializer, MessagePackMessageSerializer>();
        services.AddSingleton<IMessageTypeRegistry>(sp =>
        {
            var registry = new MessageTypeRegistry();
            GeneratedMessageRegistration.RegisterAll(registry);
            return registry;
        });
        
        // Build subscription registry
        var builder = new SubscriptionRegistryBuilder(
            configuration.GetValue<string>("ServiceName") ?? "unknown");
        configureSubscriptions(builder);
        services.AddSingleton<ISubscriptionRegistry>(builder.Build());
        
        // Publisher
        services.AddSingleton<IMessagePublisher, RedisMessagePublisher>();
        services.Decorate<IMessagePublisher, CausationAwarePublisher>();
        
        // Context and utilities
        services.AddSingleton<IMessageContextAccessor, MessageContextAccessor>();
        services.AddSingleton<ICycleDetector, CycleDetector>();
        services.AddSingleton<IIdempotencyStore>(sp =>
            new RedisIdempotencyStore(
                sp.GetRequiredService<IConnectionMultiplexer>(),
                configuration.GetValue<string>("ServiceName") ?? "unknown"));
        
        // Options
        services.Configure<MessageSubscriberOptions>(configuration.GetSection("Messaging"));
        services.Configure<CycleDetectorOptions>(configuration.GetSection("CycleDetection"));
        
        // Worker service
        services.AddHostedService<MessageSubscriberWorker>();
        
        return services;
    }
    
    public static IServiceCollection AddMessageHandlers(
        this IServiceCollection services,
        Assembly assembly)
    {
        // Auto-register all handlers from assembly
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>)));

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);
            
            // Also register as IMessageHandler<T> for each message type it handles
            var handlerInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>));
            
            foreach (var handlerInterface in handlerInterfaces)
            {
                services.AddScoped(handlerInterface, handlerType);
            }
        }

        return services;
    }
}
```

### 7.2 Program.cs for Each Microservice

```csharp
// Inventory Service Program.cs
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMessaging(builder.Configuration, subscriptions =>
{
    subscriptions
        .Subscribe<VehicleListedEvent, VehicleListedHandler>(opt => opt.MaxConcurrency = 4)
        .Subscribe<VehicleUpdatedEvent, VehicleUpdatedHandler>()
        .Subscribe<VehicleSoldEvent, VehicleSoldHandler>()
        .Subscribe<VehicleArchivedEvent, VehicleArchivedHandler>()
        .Subscribe<DealerVerifiedEvent, DealerVerifiedHandler>()
        .Subscribe<DealerSuspendedEvent, DealerSuspendedHandler>()
        .Subscribe<PriceDropEvent, PriceDropHandler>()
        .Subscribe<PhotosUploadedEvent, PhotosUploadedHandler>();
});

builder.Services.AddMessageHandlers(typeof(Program).Assembly);

// Domain services
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<ISearchIndexer, ElasticSearchIndexer>();

var host = builder.Build();
await host.RunAsync();
```

```csharp
// Inspection Service Program.cs
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMessaging(builder.Configuration, subscriptions =>
{
    subscriptions
        .Subscribe<InspectionRequestedEvent, InspectionRequestedHandler>(opt =>
        {
            opt.MaxConcurrency = 2;
            opt.RetryPolicy = new RetryPolicy { MaxRetries = 5 };
        })
        .Subscribe<InspectionScheduledEvent, InspectionScheduledHandler>()
        .Subscribe<InspectionCompletedEvent, InspectionCompletedHandler>()
        .Subscribe<VehicleListedEvent, VehicleContextHandler>()
        .Subscribe<VehicleUpdatedEvent, VehicleContextHandler>()
        .Subscribe<InspectorAssignedEvent, InspectorAssignedHandler>();
});

builder.Services.AddMessageHandlers(typeof(Program).Assembly);

builder.Services.AddScoped<IInspectionRepository, InspectionRepository>();
builder.Services.AddSingleton<IInspectionContextCache, RedisInspectionContextCache>();

var host = builder.Build();
await host.RunAsync();
```

---

## 8. Monitoring and Observability

### 8.1 Metrics

```csharp
public sealed class MessagingMetrics
{
    private readonly Counter<long> _messagesReceived;
    private readonly Counter<long> _messagesProcessed;
    private readonly Counter<long> _messagesFailed;
    private readonly Counter<long> _messagesSkipped;
    private readonly Histogram<double> _processingDuration;

    public MessagingMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Automotive.Messaging");
        
        _messagesReceived = meter.CreateCounter<long>(
            "messaging.messages.received",
            description: "Total messages received from Redis");
        
        _messagesProcessed = meter.CreateCounter<long>(
            "messaging.messages.processed",
            description: "Successfully processed messages");
        
        _messagesFailed = meter.CreateCounter<long>(
            "messaging.messages.failed",
            description: "Failed message processing attempts");
        
        _messagesSkipped = meter.CreateCounter<long>(
            "messaging.messages.skipped",
            description: "Messages skipped (not subscribed or duplicate)");
        
        _processingDuration = meter.CreateHistogram<double>(
            "messaging.processing.duration",
            unit: "ms",
            description: "Message processing duration");
    }

    public void RecordReceived(string messageType, string channel)
    {
        _messagesReceived.Add(1, 
            new KeyValuePair<string, object?>("message_type", messageType),
            new KeyValuePair<string, object?>("channel", channel));
    }

    public void RecordProcessed(string messageType, double durationMs)
    {
        _messagesProcessed.Add(1,
            new KeyValuePair<string, object?>("message_type", messageType));
        _processingDuration.Record(durationMs,
            new KeyValuePair<string, object?>("message_type", messageType));
    }

    public void RecordFailed(string messageType, string errorType)
    {
        _messagesFailed.Add(1,
            new KeyValuePair<string, object?>("message_type", messageType),
            new KeyValuePair<string, object?>("error_type", errorType));
    }

    public void RecordSkipped(string messageType, string reason)
    {
        _messagesSkipped.Add(1,
            new KeyValuePair<string, object?>("message_type", messageType),
            new KeyValuePair<string, object?>("reason", reason));
    }
}
```

---

## 9. Summary: Key Design Decisions

| Concern | Solution |
|---------|----------|
| **Selective Subscription** | Declarative `SubscriptionRegistry` per service, only subscribe to needed channels |
| **Efficient Routing** | Pre-computed `FrozenDictionary` routing table, O(1) message type lookup |
| **Minimal Deserialization** | `PeekHeader()` for early filtering before full deserialization |
| **Cyclical Events** | `CycleDetector` with correlation tracking, configurable depth limits |
| **Idempotency** | Redis-backed `IdempotencyStore` with composite keys |
| **Concurrency Control** | Per-subscription `MaxConcurrency` with `System.Threading.Channels` |
| **Retry Handling** | Configurable `RetryPolicy` with exponential backoff |
| **Causation Tracking** | `CausationAwarePublisher` decorator propagates correlation/causation chain |
| **Observability** | OpenTelemetry metrics and distributed tracing integration |

This architecture ensures each microservice only processes messages it cares about while maintaining proper event ordering, idempotency, and cycle protection for your automotive platform.