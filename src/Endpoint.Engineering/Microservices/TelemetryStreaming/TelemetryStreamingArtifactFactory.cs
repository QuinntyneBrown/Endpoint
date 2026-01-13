// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.TelemetryStreaming;

/// <summary>
/// Factory for creating Telemetry Streaming microservice artifacts.
/// Provides low-latency, push-based updates to subscribed clients.
/// </summary>
public class TelemetryStreamingArtifactFactory : ITelemetryStreamingArtifactFactory
{
    private readonly ILogger<TelemetryStreamingArtifactFactory> logger;

    public TelemetryStreamingArtifactFactory(ILogger<TelemetryStreamingArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding TelemetryStreaming.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("TelemetryMessage", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace TelemetryStreaming.Core.Entities;

                public class TelemetryMessage
                {
                    public Guid TelemetryMessageId { get; set; }
                    public required string Source { get; set; }
                    public required string MetricName { get; set; }
                    public required string Value { get; set; }
                    public string? Unit { get; set; }
                    public TelemetryType Type { get; set; } = TelemetryType.Metric;
                    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
                    public Dictionary<string, string>? Tags { get; set; }
                }

                public enum TelemetryType { Metric, Event, Trace, Log }
                """
        });

        project.Files.Add(new FileModel("TelemetrySubscription", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace TelemetryStreaming.Core.Entities;

                public class TelemetrySubscription
                {
                    public Guid SubscriptionId { get; set; }
                    public required string ConnectionId { get; set; }
                    public required string ClientId { get; set; }
                    public List<string> SubscribedMetrics { get; set; } = new List<string>();
                    public List<string> SubscribedSources { get; set; } = new List<string>();
                    public int UpdateRateMs { get; set; } = 1000;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? LastUpdateAt { get; set; }
                    public bool IsActive { get; set; } = true;
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("ITelemetryGenerator", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using TelemetryStreaming.Core.Entities;

                namespace TelemetryStreaming.Core.Interfaces;

                public interface ITelemetryGenerator
                {
                    Task<TelemetryMessage> GenerateAsync(string source, string metricName, CancellationToken cancellationToken = default);
                    Task<IEnumerable<TelemetryMessage>> GenerateBatchAsync(string source, IEnumerable<string> metricNames, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ISubscriptionManager", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using TelemetryStreaming.Core.Entities;

                namespace TelemetryStreaming.Core.Interfaces;

                public interface ISubscriptionManager
                {
                    Task<TelemetrySubscription> CreateSubscriptionAsync(string connectionId, string clientId, CancellationToken cancellationToken = default);
                    Task UpdateSubscriptionAsync(Guid subscriptionId, List<string> metrics, List<string> sources, int updateRate, CancellationToken cancellationToken = default);
                    Task<TelemetrySubscription?> GetSubscriptionAsync(string connectionId, CancellationToken cancellationToken = default);
                    Task RemoveSubscriptionAsync(string connectionId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<TelemetrySubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("TelemetrySubscriptionCreatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace TelemetryStreaming.Core.Events;

                public record TelemetrySubscriptionCreatedEvent(Guid SubscriptionId, string ConnectionId, string ClientId, DateTime CreatedAt);
                """
        });

        project.Files.Add(new FileModel("TelemetryDataPublishedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace TelemetryStreaming.Core.Events;

                public record TelemetryDataPublishedEvent(string Source, string MetricName, string Value, DateTime Timestamp);
                """
        });

        // DTOs
        project.Files.Add(new FileModel("TelemetryMessageDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace TelemetryStreaming.Core.DTOs;

                public class TelemetryMessageDto
                {
                    public required string Source { get; set; }
                    public required string MetricName { get; set; }
                    public required string Value { get; set; }
                    public string? Unit { get; set; }
                    public DateTime Timestamp { get; set; }
                    public Dictionary<string, string>? Tags { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("SubscriptionRequestDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace TelemetryStreaming.Core.DTOs;

                public class SubscriptionRequestDto
                {
                    public required string ClientId { get; set; }
                    public List<string> Metrics { get; set; } = new List<string>();
                    public List<string> Sources { get; set; } = new List<string>();
                    public int UpdateRateMs { get; set; } = 1000;
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding TelemetryStreaming.Infrastructure files");

        var servicesDir = Path.Combine(project.Directory, "Services");
        var backgroundServicesDir = Path.Combine(project.Directory, "BackgroundServices");

        // Services
        project.Files.Add(new FileModel("TelemetryGenerator", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using TelemetryStreaming.Core.Entities;
                using TelemetryStreaming.Core.Interfaces;

                namespace TelemetryStreaming.Infrastructure.Services;

                public class TelemetryGenerator : ITelemetryGenerator
                {
                    private readonly Random random = new Random();

                    public Task<TelemetryMessage> GenerateAsync(string source, string metricName, CancellationToken cancellationToken = default)
                    {
                        var message = new TelemetryMessage
                        {
                            TelemetryMessageId = Guid.NewGuid(),
                            Source = source,
                            MetricName = metricName,
                            Value = random.Next(0, 100).ToString(),
                            Unit = "units",
                            Type = TelemetryType.Metric,
                            Timestamp = DateTime.UtcNow,
                            Tags = new Dictionary<string, string>
                            {
                                { "generated", "true" },
                                { "version", "1.0" }
                            }
                        };

                        return Task.FromResult(message);
                    }

                    public async Task<IEnumerable<TelemetryMessage>> GenerateBatchAsync(string source, IEnumerable<string> metricNames, CancellationToken cancellationToken = default)
                    {
                        var messages = new List<TelemetryMessage>();
                        foreach (var metricName in metricNames)
                        {
                            messages.Add(await GenerateAsync(source, metricName, cancellationToken));
                        }
                        return messages;
                    }
                }
                """
        });

        project.Files.Add(new FileModel("SubscriptionManager", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Collections.Concurrent;
                using TelemetryStreaming.Core.Entities;
                using TelemetryStreaming.Core.Interfaces;

                namespace TelemetryStreaming.Infrastructure.Services;

                public class SubscriptionManager : ISubscriptionManager
                {
                    private readonly ConcurrentDictionary<string, TelemetrySubscription> subscriptions = new();

                    public Task<TelemetrySubscription> CreateSubscriptionAsync(string connectionId, string clientId, CancellationToken cancellationToken = default)
                    {
                        var subscription = new TelemetrySubscription
                        {
                            SubscriptionId = Guid.NewGuid(),
                            ConnectionId = connectionId,
                            ClientId = clientId,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        subscriptions.TryAdd(connectionId, subscription);
                        return Task.FromResult(subscription);
                    }

                    public Task UpdateSubscriptionAsync(Guid subscriptionId, List<string> metrics, List<string> sources, int updateRate, CancellationToken cancellationToken = default)
                    {
                        var subscription = subscriptions.Values.FirstOrDefault(s => s.SubscriptionId == subscriptionId);
                        if (subscription != null)
                        {
                            subscription.SubscribedMetrics = metrics;
                            subscription.SubscribedSources = sources;
                            subscription.UpdateRateMs = updateRate;
                            subscription.LastUpdateAt = DateTime.UtcNow;
                        }
                        return Task.CompletedTask;
                    }

                    public Task<TelemetrySubscription?> GetSubscriptionAsync(string connectionId, CancellationToken cancellationToken = default)
                    {
                        subscriptions.TryGetValue(connectionId, out var subscription);
                        return Task.FromResult(subscription);
                    }

                    public Task RemoveSubscriptionAsync(string connectionId, CancellationToken cancellationToken = default)
                    {
                        subscriptions.TryRemove(connectionId, out _);
                        return Task.CompletedTask;
                    }

                    public Task<IEnumerable<TelemetrySubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
                    {
                        var activeSubscriptions = subscriptions.Values.Where(s => s.IsActive);
                        return Task.FromResult(activeSubscriptions);
                    }
                }
                """
        });

        // Background Service for cyclic telemetry generation
        project.Files.Add(new FileModel("TelemetryPublisherService", backgroundServicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.Extensions.Hosting;
                using Microsoft.Extensions.Logging;
                using TelemetryStreaming.Core.Interfaces;

                namespace TelemetryStreaming.Infrastructure.BackgroundServices;

                public class TelemetryPublisherService : BackgroundService
                {
                    private readonly ILogger<TelemetryPublisherService> logger;
                    private readonly ITelemetryGenerator telemetryGenerator;
                    private readonly ISubscriptionManager subscriptionManager;

                    public TelemetryPublisherService(
                        ILogger<TelemetryPublisherService> logger,
                        ITelemetryGenerator telemetryGenerator,
                        ISubscriptionManager subscriptionManager)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.telemetryGenerator = telemetryGenerator ?? throw new ArgumentNullException(nameof(telemetryGenerator));
                        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
                    }

                    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
                    {
                        logger.LogInformation("Telemetry Publisher Service is starting");

                        while (!stoppingToken.IsCancellationRequested)
                        {
                            try
                            {
                                var subscriptions = await subscriptionManager.GetActiveSubscriptionsAsync(stoppingToken);
                                
                                foreach (var subscription in subscriptions)
                                {
                                    if (subscription.SubscribedMetrics.Any())
                                    {
                                        foreach (var source in subscription.SubscribedSources.Any() ? subscription.SubscribedSources : new List<string> { "default" })
                                        {
                                            var messages = await telemetryGenerator.GenerateBatchAsync(source, subscription.SubscribedMetrics, stoppingToken);
                                            // Messages would be sent via SignalR hub here
                                        }
                                    }
                                }

                                // Wait before next cycle - subscriptions are checked at a base rate
                                await Task.Delay(1000, stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error occurred while publishing telemetry");
                            }
                        }

                        logger.LogInformation("Telemetry Publisher Service is stopping");
                    }
                }
                """
        });

        // ConfigureServices
        project.Files.Add(new FileModel("ConfigureServices", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.Extensions.DependencyInjection;
                using TelemetryStreaming.Core.Interfaces;
                using TelemetryStreaming.Infrastructure.BackgroundServices;
                using TelemetryStreaming.Infrastructure.Services;

                namespace TelemetryStreaming.Infrastructure;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
                    {
                        services.AddSingleton<ITelemetryGenerator, TelemetryGenerator>();
                        services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
                        services.AddHostedService<TelemetryPublisherService>();

                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding TelemetryStreaming.Api files");

        var hubsDir = Path.Combine(project.Directory, "Hubs");

        // TelemetryHub mapped to /telemetry
        project.Files.Add(new FileModel("TelemetryHub", hubsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.SignalR;
                using TelemetryStreaming.Core.DTOs;
                using TelemetryStreaming.Core.Interfaces;

                namespace TelemetryStreaming.Api.Hubs;

                public class TelemetryHub : Hub
                {
                    private readonly ILogger<TelemetryHub> logger;
                    private readonly ISubscriptionManager subscriptionManager;
                    private readonly ITelemetryGenerator telemetryGenerator;

                    public TelemetryHub(
                        ILogger<TelemetryHub> logger,
                        ISubscriptionManager subscriptionManager,
                        ITelemetryGenerator telemetryGenerator)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
                        this.telemetryGenerator = telemetryGenerator ?? throw new ArgumentNullException(nameof(telemetryGenerator));
                    }

                    public override async Task OnConnectedAsync()
                    {
                        logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
                        await base.OnConnectedAsync();
                    }

                    public override async Task OnDisconnectedAsync(Exception? exception)
                    {
                        logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
                        await subscriptionManager.RemoveSubscriptionAsync(Context.ConnectionId);
                        await base.OnDisconnectedAsync(exception);
                    }

                    public async Task Subscribe(SubscriptionRequestDto request)
                    {
                        logger.LogInformation("Client {ClientId} subscribing to telemetry", request.ClientId);
                        
                        var subscription = await subscriptionManager.CreateSubscriptionAsync(Context.ConnectionId, request.ClientId);
                        await subscriptionManager.UpdateSubscriptionAsync(
                            subscription.SubscriptionId,
                            request.Metrics,
                            request.Sources,
                            request.UpdateRateMs);

                        await Clients.Caller.SendAsync("SubscriptionConfirmed", subscription.SubscriptionId);
                    }

                    public async Task Unsubscribe()
                    {
                        logger.LogInformation("Client {ConnectionId} unsubscribing from telemetry", Context.ConnectionId);
                        await subscriptionManager.RemoveSubscriptionAsync(Context.ConnectionId);
                        await Clients.Caller.SendAsync("UnsubscriptionConfirmed");
                    }

                    public async Task UpdateSubscription(SubscriptionRequestDto request)
                    {
                        logger.LogInformation("Client updating subscription: {ConnectionId}", Context.ConnectionId);
                        var subscription = await subscriptionManager.GetSubscriptionAsync(Context.ConnectionId);
                        
                        if (subscription != null)
                        {
                            await subscriptionManager.UpdateSubscriptionAsync(
                                subscription.SubscriptionId,
                                request.Metrics,
                                request.Sources,
                                request.UpdateRateMs);

                            await Clients.Caller.SendAsync("SubscriptionUpdated");
                        }
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

                using TelemetryStreaming.Api.Hubs;
                using TelemetryStreaming.Infrastructure;

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddSignalR();
                builder.Services.AddInfrastructureServices();

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();
                app.MapHub<TelemetryHub>("/telemetry");

                app.Run();
                """
        });
    }
}
