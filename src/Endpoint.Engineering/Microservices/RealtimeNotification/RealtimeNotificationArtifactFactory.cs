// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.RealtimeNotification;

/// <summary>
/// Factory for creating Realtime Notification microservice artifacts.
/// Provides WebSocket/SignalR real-time notifications with Redis Pub/Sub backend.
/// </summary>
public class RealtimeNotificationArtifactFactory : IRealtimeNotificationArtifactFactory
{
    private readonly ILogger<RealtimeNotificationArtifactFactory> logger;

    public RealtimeNotificationArtifactFactory(ILogger<RealtimeNotificationArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding RealtimeNotification.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("NotificationConnection", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RealtimeNotification.Core.Entities;

                /// <summary>
                /// Represents an active WebSocket/SignalR connection.
                /// </summary>
                public class NotificationConnection
                {
                    public required string ConnectionId { get; set; }
                    public required string UserId { get; set; }
                    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? LastActivityAt { get; set; }
                    public bool IsActive { get; set; } = true;
                    public Dictionary<string, string> Metadata { get; set; } = new();
                }
                """
        });

        project.Files.Add(new FileModel("NotificationMessage", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RealtimeNotification.Core.Entities;

                /// <summary>
                /// Notification message to be sent to clients.
                /// </summary>
                public class NotificationMessage
                {
                    public Guid MessageId { get; set; } = Guid.NewGuid();
                    public required string Type { get; set; }
                    public required string Payload { get; set; }
                    public string? TargetUserId { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
                }

                public enum NotificationPriority
                {
                    Low,
                    Normal,
                    High,
                    Critical
                }
                """
        });

        project.Files.Add(new FileModel("Subscription", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RealtimeNotification.Core.Entities;

                /// <summary>
                /// User subscription to notification channels.
                /// </summary>
                public class Subscription
                {
                    public Guid SubscriptionId { get; set; } = Guid.NewGuid();
                    public required string UserId { get; set; }
                    public required string ConnectionId { get; set; }
                    public List<string> Channels { get; set; } = new();
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public bool IsActive { get; set; } = true;
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IConnectionManager", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using RealtimeNotification.Core.Entities;

                namespace RealtimeNotification.Core.Interfaces;

                /// <summary>
                /// Manages WebSocket/SignalR connections with resilience tracking.
                /// </summary>
                public interface IConnectionManager
                {
                    Task AddConnectionAsync(NotificationConnection connection, CancellationToken cancellationToken = default);
                    Task RemoveConnectionAsync(string connectionId, CancellationToken cancellationToken = default);
                    Task<NotificationConnection?> GetConnectionAsync(string connectionId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<NotificationConnection>> GetConnectionsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
                    Task UpdateLastActivityAsync(string connectionId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<NotificationConnection>> GetActiveConnectionsAsync(CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("INotificationPublisher", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using RealtimeNotification.Core.Entities;

                namespace RealtimeNotification.Core.Interfaces;

                /// <summary>
                /// Publishes notifications to connected clients.
                /// </summary>
                public interface INotificationPublisher
                {
                    Task PublishToUserAsync(string userId, NotificationMessage message, CancellationToken cancellationToken = default);
                    Task PublishToConnectionAsync(string connectionId, NotificationMessage message, CancellationToken cancellationToken = default);
                    Task PublishToAllAsync(NotificationMessage message, CancellationToken cancellationToken = default);
                    Task PublishToChannelAsync(string channel, NotificationMessage message, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IRedisSubscriber", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RealtimeNotification.Core.Interfaces;

                /// <summary>
                /// Subscribes to Redis Pub/Sub channels for event notifications.
                /// </summary>
                public interface IRedisSubscriber
                {
                    Task SubscribeAsync(string channel, Func<string, Task> handler, CancellationToken cancellationToken = default);
                    Task UnsubscribeAsync(string channel, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ISubscriptionManager", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using RealtimeNotification.Core.Entities;

                namespace RealtimeNotification.Core.Interfaces;

                /// <summary>
                /// Manages user subscriptions to notification channels.
                /// </summary>
                public interface ISubscriptionManager
                {
                    Task<Subscription> CreateSubscriptionAsync(string userId, string connectionId, List<string> channels, CancellationToken cancellationToken = default);
                    Task UpdateSubscriptionAsync(Guid subscriptionId, List<string> channels, CancellationToken cancellationToken = default);
                    Task RemoveSubscriptionAsync(string connectionId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Subscription>> GetSubscriptionsForChannelAsync(string channel, CancellationToken cancellationToken = default);
                    Task<Subscription?> GetSubscriptionByConnectionIdAsync(string connectionId, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("ConnectionEstablishedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RealtimeNotification.Core.Events;

                public record ConnectionEstablishedEvent(string ConnectionId, string UserId, DateTime ConnectedAt);
                """
        });

        project.Files.Add(new FileModel("ConnectionClosedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RealtimeNotification.Core.Events;

                public record ConnectionClosedEvent(string ConnectionId, string UserId, DateTime DisconnectedAt);
                """
        });

        project.Files.Add(new FileModel("NotificationSentEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RealtimeNotification.Core.Events;

                public record NotificationSentEvent(Guid MessageId, string Type, string? TargetUserId, DateTime SentAt);
                """
        });

        // DTOs
        project.Files.Add(new FileModel("NotificationDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RealtimeNotification.Core.DTOs;

                /// <summary>
                /// DTO for notification messages.
                /// </summary>
                public class NotificationDto
                {
                    public Guid MessageId { get; set; }
                    public required string Type { get; set; }
                    public required string Payload { get; set; }
                    public DateTime CreatedAt { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("SubscriptionRequestDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RealtimeNotification.Core.DTOs;

                /// <summary>
                /// DTO for subscription requests.
                /// </summary>
                public class SubscriptionRequestDto
                {
                    public List<string> Channels { get; set; } = new();
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding RealtimeNotification.Infrastructure files");

        var servicesDir = Path.Combine(project.Directory, "Services");
        var backgroundServicesDir = Path.Combine(project.Directory, "BackgroundServices");

        // ConnectionManager implementation
        project.Files.Add(new FileModel("ConnectionManager", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Collections.Concurrent;
                using RealtimeNotification.Core.Entities;
                using RealtimeNotification.Core.Interfaces;

                namespace RealtimeNotification.Infrastructure.Services;

                /// <summary>
                /// In-memory connection manager with state tracking for connection resilience.
                /// </summary>
                public class ConnectionManager : IConnectionManager
                {
                    private readonly ConcurrentDictionary<string, NotificationConnection> connections = new();

                    public Task AddConnectionAsync(NotificationConnection connection, CancellationToken cancellationToken = default)
                    {
                        connections.TryAdd(connection.ConnectionId, connection);
                        return Task.CompletedTask;
                    }

                    public Task RemoveConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
                    {
                        connections.TryRemove(connectionId, out _);
                        return Task.CompletedTask;
                    }

                    public Task<NotificationConnection?> GetConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
                    {
                        connections.TryGetValue(connectionId, out var connection);
                        return Task.FromResult(connection);
                    }

                    public Task<IEnumerable<NotificationConnection>> GetConnectionsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
                    {
                        var userConnections = connections.Values
                            .Where(c => c.UserId == userId && c.IsActive)
                            .AsEnumerable();
                        return Task.FromResult(userConnections);
                    }

                    public Task UpdateLastActivityAsync(string connectionId, CancellationToken cancellationToken = default)
                    {
                        if (connections.TryGetValue(connectionId, out var connection))
                        {
                            connection.LastActivityAt = DateTime.UtcNow;
                        }
                        return Task.CompletedTask;
                    }

                    public Task<IEnumerable<NotificationConnection>> GetActiveConnectionsAsync(CancellationToken cancellationToken = default)
                    {
                        var activeConnections = connections.Values
                            .Where(c => c.IsActive)
                            .AsEnumerable();
                        return Task.FromResult(activeConnections);
                    }
                }
                """
        });

        // RedisSubscriber implementation
        project.Files.Add(new FileModel("RedisSubscriber", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.Extensions.Logging;
                using StackExchange.Redis;
                using RealtimeNotification.Core.Interfaces;

                namespace RealtimeNotification.Infrastructure.Services;

                /// <summary>
                /// Redis Pub/Sub subscriber for receiving events from event bus.
                /// </summary>
                public class RedisSubscriber : IRedisSubscriber
                {
                    private readonly ILogger<RedisSubscriber> logger;
                    private readonly IConnectionMultiplexer redis;

                    public RedisSubscriber(ILogger<RedisSubscriber> logger, IConnectionMultiplexer redis)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.redis = redis ?? throw new ArgumentNullException(nameof(redis));
                    }

                    public async Task SubscribeAsync(string channel, Func<string, Task> handler, CancellationToken cancellationToken = default)
                    {
                        var subscriber = redis.GetSubscriber();
                        
                        await subscriber.SubscribeAsync(channel, async (ch, message) =>
                        {
                            try
                            {
                                logger.LogDebug("Received message on channel {Channel}", channel);
                                await handler(message.ToString());
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error handling message from channel {Channel}", channel);
                            }
                        });

                        logger.LogInformation("Subscribed to Redis channel: {Channel}", channel);
                    }

                    public async Task UnsubscribeAsync(string channel, CancellationToken cancellationToken = default)
                    {
                        var subscriber = redis.GetSubscriber();
                        await subscriber.UnsubscribeAsync(channel);
                        logger.LogInformation("Unsubscribed from Redis channel: {Channel}", channel);
                    }
                }
                """
        });

        // SubscriptionManager implementation
        project.Files.Add(new FileModel("SubscriptionManager", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Collections.Concurrent;
                using RealtimeNotification.Core.Entities;
                using RealtimeNotification.Core.Interfaces;

                namespace RealtimeNotification.Infrastructure.Services;

                /// <summary>
                /// In-memory subscription manager for channel subscriptions.
                /// </summary>
                public class SubscriptionManager : ISubscriptionManager
                {
                    private readonly ConcurrentDictionary<string, Subscription> subscriptionsByConnection = new();
                    private readonly ConcurrentDictionary<string, List<Subscription>> subscriptionsByChannel = new();

                    public Task<Subscription> CreateSubscriptionAsync(string userId, string connectionId, List<string> channels, CancellationToken cancellationToken = default)
                    {
                        var subscription = new Subscription
                        {
                            UserId = userId,
                            ConnectionId = connectionId,
                            Channels = channels
                        };

                        subscriptionsByConnection.TryAdd(connectionId, subscription);

                        foreach (var channel in channels)
                        {
                            subscriptionsByChannel.AddOrUpdate(
                                channel,
                                _ => new List<Subscription> { subscription },
                                (_, list) =>
                                {
                                    list.Add(subscription);
                                    return list;
                                });
                        }

                        return Task.FromResult(subscription);
                    }

                    public Task UpdateSubscriptionAsync(Guid subscriptionId, List<string> channels, CancellationToken cancellationToken = default)
                    {
                        var subscription = subscriptionsByConnection.Values
                            .FirstOrDefault(s => s.SubscriptionId == subscriptionId);

                        if (subscription != null)
                        {
                            // Remove old channel subscriptions
                            foreach (var oldChannel in subscription.Channels)
                            {
                                if (subscriptionsByChannel.TryGetValue(oldChannel, out var list))
                                {
                                    list.Remove(subscription);
                                }
                            }

                            // Add new channel subscriptions
                            subscription.Channels = channels;
                            foreach (var channel in channels)
                            {
                                subscriptionsByChannel.AddOrUpdate(
                                    channel,
                                    _ => new List<Subscription> { subscription },
                                    (_, list) =>
                                    {
                                        list.Add(subscription);
                                        return list;
                                    });
                            }
                        }

                        return Task.CompletedTask;
                    }

                    public Task RemoveSubscriptionAsync(string connectionId, CancellationToken cancellationToken = default)
                    {
                        if (subscriptionsByConnection.TryRemove(connectionId, out var subscription))
                        {
                            foreach (var channel in subscription.Channels)
                            {
                                if (subscriptionsByChannel.TryGetValue(channel, out var list))
                                {
                                    list.Remove(subscription);
                                }
                            }
                        }

                        return Task.CompletedTask;
                    }

                    public Task<IEnumerable<Subscription>> GetSubscriptionsForChannelAsync(string channel, CancellationToken cancellationToken = default)
                    {
                        if (subscriptionsByChannel.TryGetValue(channel, out var list))
                        {
                            return Task.FromResult(list.Where(s => s.IsActive).AsEnumerable());
                        }

                        return Task.FromResult(Enumerable.Empty<Subscription>());
                    }

                    public Task<Subscription?> GetSubscriptionByConnectionIdAsync(string connectionId, CancellationToken cancellationToken = default)
                    {
                        subscriptionsByConnection.TryGetValue(connectionId, out var subscription);
                        return Task.FromResult(subscription);
                    }
                }
                """
        });

        // Background service for Redis event listening
        project.Files.Add(new FileModel("NotificationListenerService", backgroundServicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Text.Json;
                using Microsoft.Extensions.Hosting;
                using Microsoft.Extensions.Logging;
                using Microsoft.Extensions.Options;
                using RealtimeNotification.Core.Entities;
                using RealtimeNotification.Core.Interfaces;

                namespace RealtimeNotification.Infrastructure.BackgroundServices;

                /// <summary>
                /// Background service that listens to Redis Pub/Sub for notification events.
                /// Routes messages to specific users based on event data.
                /// </summary>
                public class NotificationListenerService : BackgroundService
                {
                    private readonly ILogger<NotificationListenerService> logger;
                    private readonly IRedisSubscriber redisSubscriber;
                    private readonly INotificationPublisher notificationPublisher;
                    private readonly NotificationOptions options;

                    public NotificationListenerService(
                        ILogger<NotificationListenerService> logger,
                        IRedisSubscriber redisSubscriber,
                        INotificationPublisher notificationPublisher,
                        IOptions<NotificationOptions> options)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.redisSubscriber = redisSubscriber ?? throw new ArgumentNullException(nameof(redisSubscriber));
                        this.notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
                        this.options = options?.Value ?? new NotificationOptions();
                    }

                    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
                    {
                        logger.LogInformation("Notification Listener Service starting");

                        foreach (var channel in options.Channels)
                        {
                            await redisSubscriber.SubscribeAsync(channel, async (message) =>
                            {
                                await HandleMessageAsync(channel, message, stoppingToken);
                            }, stoppingToken);
                        }

                        logger.LogInformation("Subscribed to {Count} Redis channels", options.Channels.Count);

                        // Keep the service running
                        await Task.Delay(Timeout.Infinite, stoppingToken);
                    }

                    private async Task HandleMessageAsync(string channel, string message, CancellationToken cancellationToken)
                    {
                        try
                        {
                            var notification = JsonSerializer.Deserialize<NotificationMessage>(message);
                            if (notification == null)
                            {
                                logger.LogWarning("Failed to deserialize message from channel {Channel}", channel);
                                return;
                            }

                            // Route to specific user or broadcast
                            if (!string.IsNullOrEmpty(notification.TargetUserId))
                            {
                                await notificationPublisher.PublishToUserAsync(notification.TargetUserId, notification, cancellationToken);
                            }
                            else
                            {
                                await notificationPublisher.PublishToChannelAsync(channel, notification, cancellationToken);
                            }

                            logger.LogDebug("Processed notification {MessageId} from channel {Channel}", notification.MessageId, channel);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error handling message from channel {Channel}", channel);
                        }
                    }
                }

                public class NotificationOptions
                {
                    /// <summary>
                    /// Redis channels to subscribe to for notifications.
                    /// </summary>
                    public List<string> Channels { get; set; } = new() { "notifications", "git-analysis-results" };
                }
                """
        });

        // ConfigureServices
        project.Files.Add(new FileModel("ConfigureServices", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.Extensions.Configuration;
                using Microsoft.Extensions.DependencyInjection;
                using StackExchange.Redis;
                using RealtimeNotification.Core.Interfaces;
                using RealtimeNotification.Infrastructure.Services;
                using RealtimeNotification.Infrastructure.BackgroundServices;

                namespace RealtimeNotification.Infrastructure;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.Configure<NotificationOptions>(configuration.GetSection("Notifications"));

                        // Configure Redis connection
                        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
                        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

                        services.AddSingleton<IConnectionManager, ConnectionManager>();
                        services.AddSingleton<IRedisSubscriber, RedisSubscriber>();
                        services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
                        services.AddHostedService<NotificationListenerService>();

                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding RealtimeNotification.Api files");

        var hubsDir = Path.Combine(project.Directory, "Hubs");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // SignalRNotificationPublisher implementation
        project.Files.Add(new FileModel("SignalRNotificationPublisher", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.SignalR;
                using RealtimeNotification.Api.Hubs;
                using RealtimeNotification.Core.DTOs;
                using RealtimeNotification.Core.Entities;
                using RealtimeNotification.Core.Interfaces;

                namespace RealtimeNotification.Api.Services;

                /// <summary>
                /// SignalR implementation of notification publisher.
                /// Provides targeted broadcasting to specific users and connections.
                /// </summary>
                public class SignalRNotificationPublisher : INotificationPublisher
                {
                    private readonly IHubContext<NotificationHub> hubContext;
                    private readonly IConnectionManager connectionManager;
                    private readonly ISubscriptionManager subscriptionManager;

                    public SignalRNotificationPublisher(
                        IHubContext<NotificationHub> hubContext,
                        IConnectionManager connectionManager,
                        ISubscriptionManager subscriptionManager)
                    {
                        this.hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
                        this.connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
                        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
                    }

                    public async Task PublishToUserAsync(string userId, NotificationMessage message, CancellationToken cancellationToken = default)
                    {
                        var connections = await connectionManager.GetConnectionsByUserIdAsync(userId, cancellationToken);
                        var dto = MapToDto(message);

                        foreach (var connection in connections)
                        {
                            await hubContext.Clients.Client(connection.ConnectionId)
                                .SendAsync("ReceiveNotification", dto, cancellationToken);
                        }
                    }

                    public async Task PublishToConnectionAsync(string connectionId, NotificationMessage message, CancellationToken cancellationToken = default)
                    {
                        var dto = MapToDto(message);
                        await hubContext.Clients.Client(connectionId)
                            .SendAsync("ReceiveNotification", dto, cancellationToken);
                    }

                    public async Task PublishToAllAsync(NotificationMessage message, CancellationToken cancellationToken = default)
                    {
                        var dto = MapToDto(message);
                        await hubContext.Clients.All
                            .SendAsync("ReceiveNotification", dto, cancellationToken);
                    }

                    public async Task PublishToChannelAsync(string channel, NotificationMessage message, CancellationToken cancellationToken = default)
                    {
                        var subscriptions = await subscriptionManager.GetSubscriptionsForChannelAsync(channel, cancellationToken);
                        var dto = MapToDto(message);

                        foreach (var subscription in subscriptions)
                        {
                            await hubContext.Clients.Client(subscription.ConnectionId)
                                .SendAsync("ReceiveNotification", dto, cancellationToken);
                        }
                    }

                    private static NotificationDto MapToDto(NotificationMessage message)
                    {
                        return new NotificationDto
                        {
                            MessageId = message.MessageId,
                            Type = message.Type,
                            Payload = message.Payload,
                            CreatedAt = message.CreatedAt
                        };
                    }
                }
                """
        });

        // NotificationHub
        project.Files.Add(new FileModel("NotificationHub", hubsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.SignalR;
                using RealtimeNotification.Core.DTOs;
                using RealtimeNotification.Core.Entities;
                using RealtimeNotification.Core.Interfaces;

                namespace RealtimeNotification.Api.Hubs;

                /// <summary>
                /// SignalR hub for real-time notifications.
                /// Supports JWT authentication and automatic reconnection.
                /// </summary>
                [Authorize]
                public class NotificationHub : Hub
                {
                    private readonly ILogger<NotificationHub> logger;
                    private readonly IConnectionManager connectionManager;
                    private readonly ISubscriptionManager subscriptionManager;

                    public NotificationHub(
                        ILogger<NotificationHub> logger,
                        IConnectionManager connectionManager,
                        ISubscriptionManager subscriptionManager)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
                        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
                    }

                    public override async Task OnConnectedAsync()
                    {
                        var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;
                        logger.LogInformation("Client connected: {ConnectionId}, User: {UserId}", Context.ConnectionId, userId);

                        var connection = new NotificationConnection
                        {
                            ConnectionId = Context.ConnectionId,
                            UserId = userId
                        };

                        await connectionManager.AddConnectionAsync(connection);
                        await base.OnConnectedAsync();
                    }

                    public override async Task OnDisconnectedAsync(Exception? exception)
                    {
                        logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
                        await connectionManager.RemoveConnectionAsync(Context.ConnectionId);
                        await subscriptionManager.RemoveSubscriptionAsync(Context.ConnectionId);
                        await base.OnDisconnectedAsync(exception);
                    }

                    /// <summary>
                    /// Subscribe to notification channels.
                    /// </summary>
                    public async Task Subscribe(SubscriptionRequestDto request)
                    {
                        var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;
                        logger.LogInformation("User {UserId} subscribing to channels: {Channels}",
                            userId, string.Join(", ", request.Channels));

                        var subscription = await subscriptionManager.CreateSubscriptionAsync(
                            userId,
                            Context.ConnectionId,
                            request.Channels);

                        await Clients.Caller.SendAsync("SubscriptionConfirmed", subscription.SubscriptionId);
                    }

                    /// <summary>
                    /// Update subscription channels.
                    /// </summary>
                    public async Task UpdateSubscription(SubscriptionRequestDto request)
                    {
                        var subscription = await subscriptionManager.GetSubscriptionByConnectionIdAsync(Context.ConnectionId);
                        
                        if (subscription != null)
                        {
                            await subscriptionManager.UpdateSubscriptionAsync(subscription.SubscriptionId, request.Channels);
                            await Clients.Caller.SendAsync("SubscriptionUpdated");
                        }
                    }

                    /// <summary>
                    /// Unsubscribe from all channels.
                    /// </summary>
                    public async Task Unsubscribe()
                    {
                        logger.LogInformation("Client {ConnectionId} unsubscribing", Context.ConnectionId);
                        await subscriptionManager.RemoveSubscriptionAsync(Context.ConnectionId);
                        await Clients.Caller.SendAsync("UnsubscriptionConfirmed");
                    }

                    /// <summary>
                    /// Heartbeat to update last activity.
                    /// </summary>
                    public async Task Ping()
                    {
                        await connectionManager.UpdateLastActivityAsync(Context.ConnectionId);
                        await Clients.Caller.SendAsync("Pong");
                    }
                }
                """
        });

        // Program.cs
        project.Files.Add(new FileModel("Program", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Authentication.JwtBearer;
                using Microsoft.IdentityModel.Tokens;
                using System.Text;
                using RealtimeNotification.Api.Hubs;
                using RealtimeNotification.Api.Services;
                using RealtimeNotification.Core.Interfaces;
                using RealtimeNotification.Infrastructure;

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                // Configure JWT authentication
                var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-secret-key-min-32-chars-long!";
                var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RealtimeNotification";
                var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RealtimeNotification";

                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwtIssuer,
                            ValidAudience = jwtAudience,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                        };

                        // Allow JWT in SignalR query string
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"];
                                var path = context.HttpContext.Request.Path;
                                
                                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notifications"))
                                {
                                    context.Token = accessToken;
                                }
                                
                                return Task.CompletedTask;
                            }
                        };
                    });

                builder.Services.AddAuthorization();

                // Configure SignalR with Redis backplane for horizontal scaling
                var signalRBuilder = builder.Services.AddSignalR();
                var redisConnection = builder.Configuration.GetConnectionString("Redis");
                if (!string.IsNullOrEmpty(redisConnection))
                {
                    signalRBuilder.AddStackExchangeRedis(redisConnection);
                }

                builder.Services.AddInfrastructureServices(builder.Configuration);
                builder.Services.AddSingleton<INotificationPublisher, SignalRNotificationPublisher>();

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();
                app.MapHub<NotificationHub>("/notifications");

                app.Run();
                """
        });

        // appsettings.json
        project.Files.Add(new FileModel("appsettings", project.Directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "Redis": "localhost:6379"
                  },
                  "Jwt": {
                    "Key": "your-secret-key-min-32-chars-long!",
                    "Issuer": "RealtimeNotification",
                    "Audience": "RealtimeNotification"
                  },
                  "Notifications": {
                    "Channels": [
                      "notifications",
                      "git-analysis-results",
                      "system-events"
                    ]
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information",
                      "Microsoft.AspNetCore": "Warning",
                      "Microsoft.AspNetCore.SignalR": "Debug"
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        });
    }
}
