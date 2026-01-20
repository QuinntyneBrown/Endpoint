// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Expressions;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Attributes;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Distribution;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

/// <summary>
/// Factory for creating Distribution microservice artifacts.
/// Provides subscription-based telemetry distribution via SignalR WebSockets.
/// </summary>
public class DistributionArtifactFactory : IDistributionArtifactFactory
{
    private readonly ILogger<DistributionArtifactFactory> logger;

    public DistributionArtifactFactory(ILogger<DistributionArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Distribution.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateTelemetryDataFile(entitiesDir));
        project.Files.Add(CreateTelemetrySubscriptionFile(entitiesDir));
        project.Files.Add(CreateDistributionChannelFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateITelemetryDistributorFile(interfacesDir));
        project.Files.Add(CreateISubscriptionManagerFile(interfacesDir));
        project.Files.Add(CreateITelemetrySourceFile(interfacesDir));

        // Events
        project.Files.Add(CreateSubscriptionCreatedEventFile(eventsDir));
        project.Files.Add(CreateTelemetryDistributedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateTelemetryDataDtoFile(dtosDir));
        project.Files.Add(CreateSubscriptionRequestDtoFile(dtosDir));
        project.Files.Add(CreateSubscriptionResponseDtoFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Distribution.Infrastructure files");

        var servicesDir = Path.Combine(project.Directory, "Services");
        var backgroundServicesDir = Path.Combine(project.Directory, "BackgroundServices");

        // Services
        project.Files.Add(CreateSubscriptionManagerFile(servicesDir));
        project.Files.Add(CreateTelemetrySourceFile(servicesDir));

        // Background Services
        project.Files.Add(CreateTelemetryDistributionServiceFile(backgroundServicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Distribution.Api files");

        var hubsDir = Path.Combine(project.Directory, "Hubs");
        var servicesDir = Path.Combine(project.Directory, "Services");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Hubs
        project.Files.Add(CreateTelemetryHubFile(hubsDir));

        // Services
        project.Files.Add(CreateSignalRTelemetryDistributorFile(servicesDir));

        // Controllers
        project.Files.Add(CreateSubscriptionControllerFile(controllersDir));

        // Configuration files
        project.Files.Add(CreateAppSettingsFile(project.Directory));
        project.Files.Add(CreateProgramFile(project.Directory));
    }

    public void AddTestFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Distribution.Tests files");

        var integrationTestsDir = Path.Combine(project.Directory, "Integration");
        var fixturesDir = Path.Combine(project.Directory, "Fixtures");

        // Test Fixtures
        project.Files.Add(CreateWebApplicationFactoryFixtureFile(fixturesDir));

        // Integration Tests
        project.Files.Add(CreateTelemetryHubIntegrationTestsFile(integrationTestsDir));
    }

    #region Core Layer Files

    private static FileModel CreateTelemetryDataFile(string directory)
    {
        return new FileModel("TelemetryData", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Distribution.Core.Entities;

                /// <summary>
                /// Represents a telemetry data point for distribution.
                /// </summary>
                public class TelemetryData
                {
                    public Guid TelemetryDataId { get; set; } = Guid.NewGuid();

                    public required string Channel { get; set; }

                    public required string MetricName { get; set; }

                    public required string Value { get; set; }

                    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

                    public Dictionary<string, string> Tags { get; set; } = new();

                    public string? Source { get; set; }
                }
                """
        };
    }

    private static FileModel CreateTelemetrySubscriptionFile(string directory)
    {
        return new FileModel("TelemetrySubscription", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Distribution.Core.Entities;

                /// <summary>
                /// Represents a client subscription to telemetry data channels.
                /// </summary>
                public class TelemetrySubscription
                {
                    public Guid SubscriptionId { get; set; } = Guid.NewGuid();

                    public required string ConnectionId { get; set; }

                    public required string ClientId { get; set; }

                    public List<string> Channels { get; set; } = new();

                    public List<string> MetricFilters { get; set; } = new();

                    public int PushIntervalMs { get; set; } = 100;

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? LastPushAt { get; set; }

                    public bool IsActive { get; set; } = true;

                    public long TotalMessagesPushed { get; set; }
                }
                """
        };
    }

    private static FileModel CreateDistributionChannelFile(string directory)
    {
        return new FileModel("DistributionChannel", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Distribution.Core.Entities;

                /// <summary>
                /// Represents a telemetry distribution channel.
                /// </summary>
                public class DistributionChannel
                {
                    public required string Name { get; set; }

                    public string? Description { get; set; }

                    public bool IsEnabled { get; set; } = true;

                    public int DefaultPushIntervalMs { get; set; } = 100;

                    public List<string> AvailableMetrics { get; set; } = new();

                    public int ActiveSubscriberCount { get; set; }
                }

                /// <summary>
                /// Predefined distribution channels for common telemetry types.
                /// </summary>
                public static class DistributionChannels
                {
                    public const string System = "system";
                    public const string Application = "application";
                    public const string Performance = "performance";
                    public const string Security = "security";
                    public const string Custom = "custom";

                    public static readonly string[] All = new[]
                    {
                        System,
                        Application,
                        Performance,
                        Security,
                        Custom
                    };
                }
                """
        };
    }

    private static FileModel CreateITelemetryDistributorFile(string directory)
    {
        return new FileModel("ITelemetryDistributor", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Distribution.Core.DTOs;

                namespace Distribution.Core.Interfaces;

                /// <summary>
                /// Interface for distributing telemetry data to subscribed clients.
                /// </summary>
                public interface ITelemetryDistributor
                {
                    /// <summary>
                    /// Pushes telemetry data to a specific client connection.
                    /// </summary>
                    Task PushToClientAsync(string connectionId, TelemetryDataDto data, CancellationToken cancellationToken = default);

                    /// <summary>
                    /// Pushes telemetry data to all subscribers of a channel.
                    /// </summary>
                    Task PushToChannelAsync(string channel, TelemetryDataDto data, CancellationToken cancellationToken = default);

                    /// <summary>
                    /// Pushes a batch of telemetry data to a specific client.
                    /// </summary>
                    Task PushBatchToClientAsync(string connectionId, IEnumerable<TelemetryDataDto> data, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateISubscriptionManagerFile(string directory)
    {
        return new FileModel("ISubscriptionManager", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Distribution.Core.Entities;
                using Distribution.Core.DTOs;

                namespace Distribution.Core.Interfaces;

                /// <summary>
                /// Interface for managing telemetry subscriptions.
                /// </summary>
                public interface ISubscriptionManager
                {
                    /// <summary>
                    /// Creates a new subscription for a client.
                    /// </summary>
                    TelemetrySubscription CreateSubscription(string connectionId, SubscriptionRequestDto request);

                    /// <summary>
                    /// Updates an existing subscription.
                    /// </summary>
                    void UpdateSubscription(Guid subscriptionId, SubscriptionRequestDto request);

                    /// <summary>
                    /// Removes a subscription by connection ID.
                    /// </summary>
                    void RemoveSubscription(string connectionId);

                    /// <summary>
                    /// Gets a subscription by connection ID.
                    /// </summary>
                    TelemetrySubscription? GetSubscription(string connectionId);

                    /// <summary>
                    /// Gets all active subscriptions.
                    /// </summary>
                    IEnumerable<TelemetrySubscription> GetActiveSubscriptions();

                    /// <summary>
                    /// Gets all subscriptions for a specific channel.
                    /// </summary>
                    IEnumerable<TelemetrySubscription> GetSubscriptionsForChannel(string channel);

                    /// <summary>
                    /// Gets all subscriptions that match a specific metric.
                    /// </summary>
                    IEnumerable<TelemetrySubscription> GetSubscriptionsForMetric(string channel, string metricName);

                    /// <summary>
                    /// Records a push to a subscription.
                    /// </summary>
                    void RecordPush(string connectionId, int messageCount);
                }
                """
        };
    }

    private static FileModel CreateITelemetrySourceFile(string directory)
    {
        return new FileModel("ITelemetrySource", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Distribution.Core.Entities;

                namespace Distribution.Core.Interfaces;

                /// <summary>
                /// Interface for generating or retrieving telemetry data.
                /// </summary>
                public interface ITelemetrySource
                {
                    /// <summary>
                    /// Generates telemetry data for a specific channel.
                    /// </summary>
                    IEnumerable<TelemetryData> GenerateForChannel(string channel);

                    /// <summary>
                    /// Generates telemetry data for specific metrics.
                    /// </summary>
                    IEnumerable<TelemetryData> GenerateForMetrics(string channel, IEnumerable<string> metrics);

                    /// <summary>
                    /// Gets available metrics for a channel.
                    /// </summary>
                    IEnumerable<string> GetAvailableMetrics(string channel);

                    /// <summary>
                    /// Gets all available channels.
                    /// </summary>
                    IEnumerable<DistributionChannel> GetAvailableChannels();
                }
                """
        };
    }

    private static FileModel CreateSubscriptionCreatedEventFile(string directory)
    {
        return new FileModel("SubscriptionCreatedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Distribution.Core.Events;

                public record SubscriptionCreatedEvent(
                    Guid SubscriptionId,
                    string ConnectionId,
                    string ClientId,
                    List<string> Channels,
                    DateTime CreatedAt);
                """
        };
    }

    private static FileModel CreateTelemetryDistributedEventFile(string directory)
    {
        return new FileModel("TelemetryDistributedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Distribution.Core.Events;

                public record TelemetryDistributedEvent(
                    string Channel,
                    string MetricName,
                    int SubscriberCount,
                    DateTime DistributedAt);
                """
        };
    }

    private static FileModel CreateTelemetryDataDtoFile(string directory)
    {
        return new FileModel("TelemetryDataDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Distribution.Core.DTOs;

                /// <summary>
                /// Data transfer object for telemetry data.
                /// </summary>
                public class TelemetryDataDto
                {
                    public required string Channel { get; set; }

                    public required string MetricName { get; set; }

                    public required string Value { get; set; }

                    public DateTime Timestamp { get; set; }

                    public Dictionary<string, string>? Tags { get; set; }

                    public string? Source { get; set; }
                }
                """
        };
    }

    private static FileModel CreateSubscriptionRequestDtoFile(string directory)
    {
        return new FileModel("SubscriptionRequestDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Distribution.Core.DTOs;

                /// <summary>
                /// Request DTO for creating or updating a subscription.
                /// </summary>
                public class SubscriptionRequestDto
                {
                    public required string ClientId { get; set; }

                    public List<string> Channels { get; set; } = new();

                    public List<string> MetricFilters { get; set; } = new();

                    public int PushIntervalMs { get; set; } = 100;
                }
                """
        };
    }

    private static FileModel CreateSubscriptionResponseDtoFile(string directory)
    {
        return new FileModel("SubscriptionResponseDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Distribution.Core.DTOs;

                /// <summary>
                /// Response DTO for subscription operations.
                /// </summary>
                public class SubscriptionResponseDto
                {
                    public Guid SubscriptionId { get; set; }

                    public string? ConnectionId { get; set; }

                    public List<string> Channels { get; set; } = new();

                    public List<string> MetricFilters { get; set; } = new();

                    public int PushIntervalMs { get; set; }

                    public DateTime CreatedAt { get; set; }

                    public bool IsActive { get; set; }
                }
                """
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static FileModel CreateSubscriptionManagerFile(string directory)
    {
        return new FileModel("SubscriptionManager", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Collections.Concurrent;
                using Distribution.Core.DTOs;
                using Distribution.Core.Entities;
                using Distribution.Core.Interfaces;
                using Microsoft.Extensions.Logging;

                namespace Distribution.Infrastructure.Services;

                /// <summary>
                /// Manages telemetry subscriptions in memory using thread-safe collections.
                /// </summary>
                public class SubscriptionManager : ISubscriptionManager
                {
                    private readonly ILogger<SubscriptionManager> logger;
                    private readonly ConcurrentDictionary<string, TelemetrySubscription> subscriptions = new();

                    public SubscriptionManager(ILogger<SubscriptionManager> logger)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    public TelemetrySubscription CreateSubscription(string connectionId, SubscriptionRequestDto request)
                    {
                        var subscription = new TelemetrySubscription
                        {
                            SubscriptionId = Guid.NewGuid(),
                            ConnectionId = connectionId,
                            ClientId = request.ClientId,
                            Channels = request.Channels,
                            MetricFilters = request.MetricFilters,
                            PushIntervalMs = request.PushIntervalMs,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        subscriptions.TryAdd(connectionId, subscription);
                        logger.LogInformation("Created subscription {SubscriptionId} for client {ClientId} on channels: {Channels}",
                            subscription.SubscriptionId, request.ClientId, string.Join(", ", request.Channels));

                        return subscription;
                    }

                    public void UpdateSubscription(Guid subscriptionId, SubscriptionRequestDto request)
                    {
                        var subscription = subscriptions.Values.FirstOrDefault(s => s.SubscriptionId == subscriptionId);
                        if (subscription != null)
                        {
                            subscription.Channels = request.Channels;
                            subscription.MetricFilters = request.MetricFilters;
                            subscription.PushIntervalMs = request.PushIntervalMs;
                            logger.LogInformation("Updated subscription {SubscriptionId}", subscriptionId);
                        }
                    }

                    public void RemoveSubscription(string connectionId)
                    {
                        if (subscriptions.TryRemove(connectionId, out var subscription))
                        {
                            subscription.IsActive = false;
                            logger.LogInformation("Removed subscription {SubscriptionId} for connection {ConnectionId}",
                                subscription.SubscriptionId, connectionId);
                        }
                    }

                    public TelemetrySubscription? GetSubscription(string connectionId)
                    {
                        subscriptions.TryGetValue(connectionId, out var subscription);
                        return subscription;
                    }

                    public IEnumerable<TelemetrySubscription> GetActiveSubscriptions()
                    {
                        return subscriptions.Values.Where(s => s.IsActive);
                    }

                    public IEnumerable<TelemetrySubscription> GetSubscriptionsForChannel(string channel)
                    {
                        return subscriptions.Values.Where(s =>
                            s.IsActive &&
                            (s.Channels.Count == 0 || s.Channels.Contains(channel)));
                    }

                    public IEnumerable<TelemetrySubscription> GetSubscriptionsForMetric(string channel, string metricName)
                    {
                        return subscriptions.Values.Where(s =>
                            s.IsActive &&
                            (s.Channels.Count == 0 || s.Channels.Contains(channel)) &&
                            (s.MetricFilters.Count == 0 || s.MetricFilters.Any(f => metricName.Contains(f, StringComparison.OrdinalIgnoreCase))));
                    }

                    public void RecordPush(string connectionId, int messageCount)
                    {
                        if (subscriptions.TryGetValue(connectionId, out var subscription))
                        {
                            subscription.LastPushAt = DateTime.UtcNow;
                            subscription.TotalMessagesPushed += messageCount;
                        }
                    }
                }
                """
        };
    }

    private static FileModel CreateTelemetrySourceFile(string directory)
    {
        return new FileModel("TelemetrySource", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Distribution.Core.Entities;
                using Distribution.Core.Interfaces;

                namespace Distribution.Infrastructure.Services;

                /// <summary>
                /// Generates simulated telemetry data for various channels.
                /// </summary>
                public class TelemetrySource : ITelemetrySource
                {
                    private readonly Random random = new();

                    private readonly Dictionary<string, List<string>> channelMetrics = new()
                    {
                        [DistributionChannels.System] = new()
                        {
                            "cpu_usage_percent",
                            "memory_usage_percent",
                            "disk_io_read_bytes",
                            "disk_io_write_bytes",
                            "network_bytes_in",
                            "network_bytes_out",
                            "process_count",
                            "thread_count"
                        },
                        [DistributionChannels.Application] = new()
                        {
                            "request_count",
                            "response_time_ms",
                            "error_count",
                            "active_connections",
                            "queue_depth",
                            "cache_hit_ratio"
                        },
                        [DistributionChannels.Performance] = new()
                        {
                            "gc_collection_count",
                            "gc_heap_size_bytes",
                            "thread_pool_queue_length",
                            "contention_count",
                            "exception_count",
                            "request_latency_p99"
                        },
                        [DistributionChannels.Security] = new()
                        {
                            "auth_success_count",
                            "auth_failure_count",
                            "blocked_requests",
                            "suspicious_activity_count",
                            "certificate_expiry_days"
                        },
                        [DistributionChannels.Custom] = new()
                        {
                            "custom_metric_1",
                            "custom_metric_2",
                            "custom_metric_3"
                        }
                    };

                    public IEnumerable<TelemetryData> GenerateForChannel(string channel)
                    {
                        if (!channelMetrics.TryGetValue(channel, out var metrics))
                        {
                            return Enumerable.Empty<TelemetryData>();
                        }

                        return metrics.Select(metric => GenerateTelemetryData(channel, metric));
                    }

                    public IEnumerable<TelemetryData> GenerateForMetrics(string channel, IEnumerable<string> metrics)
                    {
                        return metrics.Select(metric => GenerateTelemetryData(channel, metric));
                    }

                    public IEnumerable<string> GetAvailableMetrics(string channel)
                    {
                        return channelMetrics.TryGetValue(channel, out var metrics)
                            ? metrics
                            : Enumerable.Empty<string>();
                    }

                    public IEnumerable<DistributionChannel> GetAvailableChannels()
                    {
                        return channelMetrics.Select(kvp => new DistributionChannel
                        {
                            Name = kvp.Key,
                            Description = $"Telemetry channel for {kvp.Key} metrics",
                            IsEnabled = true,
                            DefaultPushIntervalMs = 100,
                            AvailableMetrics = kvp.Value
                        });
                    }

                    private TelemetryData GenerateTelemetryData(string channel, string metricName)
                    {
                        var value = metricName switch
                        {
                            "cpu_usage_percent" => (random.NextDouble() * 100).ToString("F2"),
                            "memory_usage_percent" => (random.NextDouble() * 100).ToString("F2"),
                            "disk_io_read_bytes" => random.Next(0, 1000000).ToString(),
                            "disk_io_write_bytes" => random.Next(0, 1000000).ToString(),
                            "network_bytes_in" => random.Next(0, 10000000).ToString(),
                            "network_bytes_out" => random.Next(0, 10000000).ToString(),
                            "process_count" => random.Next(50, 200).ToString(),
                            "thread_count" => random.Next(100, 500).ToString(),
                            "request_count" => random.Next(0, 1000).ToString(),
                            "response_time_ms" => random.Next(1, 500).ToString(),
                            "error_count" => random.Next(0, 10).ToString(),
                            "active_connections" => random.Next(0, 1000).ToString(),
                            "queue_depth" => random.Next(0, 100).ToString(),
                            "cache_hit_ratio" => (random.NextDouble() * 100).ToString("F2"),
                            "gc_collection_count" => random.Next(0, 50).ToString(),
                            "gc_heap_size_bytes" => random.Next(10000000, 100000000).ToString(),
                            "request_latency_p99" => random.Next(10, 1000).ToString(),
                            "auth_success_count" => random.Next(0, 100).ToString(),
                            "auth_failure_count" => random.Next(0, 10).ToString(),
                            _ => random.NextDouble().ToString("F4")
                        };

                        return new TelemetryData
                        {
                            TelemetryDataId = Guid.NewGuid(),
                            Channel = channel,
                            MetricName = metricName,
                            Value = value,
                            Timestamp = DateTime.UtcNow,
                            Source = "TelemetrySource"
                        };
                    }
                }
                """
        };
    }

    private static FileModel CreateTelemetryDistributionServiceFile(string directory)
    {
        return new FileModel("TelemetryDistributionService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Distribution.Core.DTOs;
                using Distribution.Core.Entities;
                using Distribution.Core.Interfaces;
                using Microsoft.Extensions.Hosting;
                using Microsoft.Extensions.Logging;
                using Microsoft.Extensions.Options;

                namespace Distribution.Infrastructure.BackgroundServices;

                /// <summary>
                /// Background service that distributes telemetry to subscribed clients.
                /// </summary>
                public class TelemetryDistributionService : BackgroundService
                {
                    private readonly ILogger<TelemetryDistributionService> logger;
                    private readonly ITelemetrySource telemetrySource;
                    private readonly ISubscriptionManager subscriptionManager;
                    private readonly ITelemetryDistributor telemetryDistributor;
                    private readonly DistributionOptions options;

                    public TelemetryDistributionService(
                        ILogger<TelemetryDistributionService> logger,
                        ITelemetrySource telemetrySource,
                        ISubscriptionManager subscriptionManager,
                        ITelemetryDistributor telemetryDistributor,
                        IOptions<DistributionOptions> options)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.telemetrySource = telemetrySource ?? throw new ArgumentNullException(nameof(telemetrySource));
                        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
                        this.telemetryDistributor = telemetryDistributor ?? throw new ArgumentNullException(nameof(telemetryDistributor));
                        this.options = options?.Value ?? new DistributionOptions();
                    }

                    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
                    {
                        logger.LogInformation("Telemetry Distribution Service starting with {Rate}ms base interval",
                            options.BaseIntervalMs);

                        while (!stoppingToken.IsCancellationRequested)
                        {
                            try
                            {
                                await DistributeTelemetryAsync(stoppingToken);
                                await Task.Delay(options.BaseIntervalMs, stoppingToken);
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error occurred while distributing telemetry");
                                await Task.Delay(1000, stoppingToken);
                            }
                        }

                        logger.LogInformation("Telemetry Distribution Service stopped");
                    }

                    private async Task DistributeTelemetryAsync(CancellationToken cancellationToken)
                    {
                        var activeSubscriptions = subscriptionManager.GetActiveSubscriptions().ToList();

                        if (activeSubscriptions.Count == 0)
                        {
                            return;
                        }

                        foreach (var channel in DistributionChannels.All)
                        {
                            var channelData = telemetrySource.GenerateForChannel(channel).ToList();

                            foreach (var data in channelData)
                            {
                                var subscribers = subscriptionManager.GetSubscriptionsForMetric(channel, data.MetricName);

                                foreach (var subscription in subscribers)
                                {
                                    var dto = new TelemetryDataDto
                                    {
                                        Channel = data.Channel,
                                        MetricName = data.MetricName,
                                        Value = data.Value,
                                        Timestamp = data.Timestamp,
                                        Source = data.Source,
                                        Tags = data.Tags
                                    };

                                    await telemetryDistributor.PushToClientAsync(
                                        subscription.ConnectionId,
                                        dto,
                                        cancellationToken);

                                    subscriptionManager.RecordPush(subscription.ConnectionId, 1);
                                }
                            }
                        }
                    }
                }

                /// <summary>
                /// Configuration options for the distribution service.
                /// </summary>
                public class DistributionOptions
                {
                    /// <summary>
                    /// Base interval in milliseconds for the distribution cycle.
                    /// Default is 100ms (10Hz).
                    /// </summary>
                    public int BaseIntervalMs { get; set; } = 100;

                    /// <summary>
                    /// Whether to enable batch distribution mode.
                    /// </summary>
                    public bool EnableBatchMode { get; set; } = false;

                    /// <summary>
                    /// Maximum batch size when batch mode is enabled.
                    /// </summary>
                    public int MaxBatchSize { get; set; } = 100;
                }
                """
        };
    }

    private static FileModel CreateInfrastructureConfigureServicesFile(string directory)
    {
        return new FileModel("ConfigureServices", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Distribution.Core.Interfaces;
                using Distribution.Infrastructure.BackgroundServices;
                using Distribution.Infrastructure.Services;
                using Microsoft.Extensions.Configuration;
                using Microsoft.Extensions.DependencyInjection;

                namespace Distribution.Infrastructure;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddInfrastructureServices(
                        this IServiceCollection services,
                        IConfiguration configuration)
                    {
                        services.Configure<DistributionOptions>(configuration.GetSection("Distribution"));

                        services.AddSingleton<ITelemetrySource, TelemetrySource>();
                        services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
                        services.AddHostedService<TelemetryDistributionService>();

                        return services;
                    }
                }
                """
        };
    }

    #endregion

    #region API Layer Files

    private static FileModel CreateTelemetryHubFile(string directory)
    {
        return new FileModel("TelemetryHub", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Distribution.Core.DTOs;
                using Distribution.Core.Interfaces;
                using Microsoft.AspNetCore.SignalR;
                using Microsoft.Extensions.Logging;

                namespace Distribution.Api.Hubs;

                /// <summary>
                /// SignalR Hub for real-time telemetry distribution to clients.
                /// Clients can subscribe to channels and receive telemetry data via WebSocket connections.
                /// </summary>
                public class TelemetryHub : Hub
                {
                    private readonly ILogger<TelemetryHub> logger;
                    private readonly ISubscriptionManager subscriptionManager;
                    private readonly ITelemetrySource telemetrySource;

                    public TelemetryHub(
                        ILogger<TelemetryHub> logger,
                        ISubscriptionManager subscriptionManager,
                        ITelemetrySource telemetrySource)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
                        this.telemetrySource = telemetrySource ?? throw new ArgumentNullException(nameof(telemetrySource));
                    }

                    public override async Task OnConnectedAsync()
                    {
                        logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
                        await base.OnConnectedAsync();
                    }

                    public override async Task OnDisconnectedAsync(Exception? exception)
                    {
                        logger.LogInformation("Client disconnected: {ConnectionId}, Exception: {Exception}",
                            Context.ConnectionId, exception?.Message);

                        subscriptionManager.RemoveSubscription(Context.ConnectionId);
                        await base.OnDisconnectedAsync(exception);
                    }

                    /// <summary>
                    /// Subscribe to telemetry channels.
                    /// </summary>
                    public async Task Subscribe(SubscriptionRequestDto request)
                    {
                        logger.LogInformation("Client {ClientId} subscribing to channels: {Channels}",
                            request.ClientId, string.Join(", ", request.Channels));

                        var subscription = subscriptionManager.CreateSubscription(Context.ConnectionId, request);

                        var response = new SubscriptionResponseDto
                        {
                            SubscriptionId = subscription.SubscriptionId,
                            ConnectionId = subscription.ConnectionId,
                            Channels = subscription.Channels,
                            MetricFilters = subscription.MetricFilters,
                            PushIntervalMs = subscription.PushIntervalMs,
                            CreatedAt = subscription.CreatedAt,
                            IsActive = subscription.IsActive
                        };

                        await Clients.Caller.SendAsync("SubscriptionConfirmed", response);

                        // Add to SignalR groups for each channel
                        foreach (var channel in request.Channels)
                        {
                            await Groups.AddToGroupAsync(Context.ConnectionId, channel);
                        }
                    }

                    /// <summary>
                    /// Update an existing subscription.
                    /// </summary>
                    public async Task UpdateSubscription(SubscriptionRequestDto request)
                    {
                        logger.LogInformation("Client updating subscription: {ConnectionId}", Context.ConnectionId);

                        var existingSubscription = subscriptionManager.GetSubscription(Context.ConnectionId);
                        if (existingSubscription != null)
                        {
                            // Remove from old channel groups
                            foreach (var channel in existingSubscription.Channels)
                            {
                                await Groups.RemoveFromGroupAsync(Context.ConnectionId, channel);
                            }

                            subscriptionManager.UpdateSubscription(existingSubscription.SubscriptionId, request);

                            // Add to new channel groups
                            foreach (var channel in request.Channels)
                            {
                                await Groups.AddToGroupAsync(Context.ConnectionId, channel);
                            }

                            await Clients.Caller.SendAsync("SubscriptionUpdated");
                        }
                    }

                    /// <summary>
                    /// Unsubscribe from all telemetry.
                    /// </summary>
                    public async Task Unsubscribe()
                    {
                        logger.LogInformation("Client unsubscribing: {ConnectionId}", Context.ConnectionId);

                        var subscription = subscriptionManager.GetSubscription(Context.ConnectionId);
                        if (subscription != null)
                        {
                            foreach (var channel in subscription.Channels)
                            {
                                await Groups.RemoveFromGroupAsync(Context.ConnectionId, channel);
                            }
                        }

                        subscriptionManager.RemoveSubscription(Context.ConnectionId);
                        await Clients.Caller.SendAsync("UnsubscriptionConfirmed");
                    }

                    /// <summary>
                    /// Get available channels.
                    /// </summary>
                    public async Task GetChannels()
                    {
                        var channels = telemetrySource.GetAvailableChannels();
                        await Clients.Caller.SendAsync("ChannelList", channels);
                    }

                    /// <summary>
                    /// Get available metrics for a channel.
                    /// </summary>
                    public async Task GetMetrics(string channel)
                    {
                        var metrics = telemetrySource.GetAvailableMetrics(channel);
                        await Clients.Caller.SendAsync("MetricList", channel, metrics);
                    }

                    /// <summary>
                    /// Ping to check connection health.
                    /// </summary>
                    public async Task Ping()
                    {
                        await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
                    }
                }
                """
        };
    }

    private static FileModel CreateSignalRTelemetryDistributorFile(string directory)
    {
        return new FileModel("SignalRTelemetryDistributor", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Distribution.Api.Hubs;
                using Distribution.Core.DTOs;
                using Distribution.Core.Interfaces;
                using Microsoft.AspNetCore.SignalR;

                namespace Distribution.Api.Services;

                /// <summary>
                /// SignalR-based implementation of ITelemetryDistributor.
                /// </summary>
                public class SignalRTelemetryDistributor : ITelemetryDistributor
                {
                    private readonly IHubContext<TelemetryHub> hubContext;

                    public SignalRTelemetryDistributor(IHubContext<TelemetryHub> hubContext)
                    {
                        this.hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
                    }

                    public async Task PushToClientAsync(string connectionId, TelemetryDataDto data, CancellationToken cancellationToken = default)
                    {
                        await hubContext.Clients.Client(connectionId)
                            .SendAsync("ReceiveTelemetry", data, cancellationToken);
                    }

                    public async Task PushToChannelAsync(string channel, TelemetryDataDto data, CancellationToken cancellationToken = default)
                    {
                        await hubContext.Clients.Group(channel)
                            .SendAsync("ReceiveTelemetry", data, cancellationToken);
                    }

                    public async Task PushBatchToClientAsync(string connectionId, IEnumerable<TelemetryDataDto> data, CancellationToken cancellationToken = default)
                    {
                        await hubContext.Clients.Client(connectionId)
                            .SendAsync("ReceiveTelemetryBatch", data, cancellationToken);
                    }
                }
                """
        };
    }

    private static FileModel CreateSubscriptionControllerFile(string directory)
    {
        return new FileModel("SubscriptionController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Distribution.Core.DTOs;
                using Distribution.Core.Interfaces;
                using Microsoft.AspNetCore.Mvc;

                namespace Distribution.Api.Controllers;

                /// <summary>
                /// REST API controller for managing subscriptions and querying telemetry metadata.
                /// </summary>
                [ApiController]
                [Route("api/[controller]")]
                public class SubscriptionController : ControllerBase
                {
                    private readonly ISubscriptionManager subscriptionManager;
                    private readonly ITelemetrySource telemetrySource;

                    public SubscriptionController(
                        ISubscriptionManager subscriptionManager,
                        ITelemetrySource telemetrySource)
                    {
                        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
                        this.telemetrySource = telemetrySource ?? throw new ArgumentNullException(nameof(telemetrySource));
                    }

                    /// <summary>
                    /// Get all active subscriptions.
                    /// </summary>
                    [HttpGet]
                    public IActionResult GetSubscriptions()
                    {
                        var subscriptions = subscriptionManager.GetActiveSubscriptions()
                            .Select(s => new SubscriptionResponseDto
                            {
                                SubscriptionId = s.SubscriptionId,
                                ConnectionId = s.ConnectionId,
                                Channels = s.Channels,
                                MetricFilters = s.MetricFilters,
                                PushIntervalMs = s.PushIntervalMs,
                                CreatedAt = s.CreatedAt,
                                IsActive = s.IsActive
                            });

                        return Ok(subscriptions);
                    }

                    /// <summary>
                    /// Get available channels.
                    /// </summary>
                    [HttpGet("channels")]
                    public IActionResult GetChannels()
                    {
                        var channels = telemetrySource.GetAvailableChannels();
                        return Ok(channels);
                    }

                    /// <summary>
                    /// Get available metrics for a specific channel.
                    /// </summary>
                    [HttpGet("channels/{channel}/metrics")]
                    public IActionResult GetMetrics(string channel)
                    {
                        var metrics = telemetrySource.GetAvailableMetrics(channel);
                        return Ok(metrics);
                    }

                    /// <summary>
                    /// Get subscription statistics.
                    /// </summary>
                    [HttpGet("stats")]
                    public IActionResult GetStats()
                    {
                        var subscriptions = subscriptionManager.GetActiveSubscriptions().ToList();

                        var stats = new
                        {
                            TotalActiveSubscriptions = subscriptions.Count,
                            TotalMessagesPushed = subscriptions.Sum(s => s.TotalMessagesPushed),
                            SubscriptionsByChannel = subscriptions
                                .SelectMany(s => s.Channels)
                                .GroupBy(c => c)
                                .ToDictionary(g => g.Key, g => g.Count())
                        };

                        return Ok(stats);
                    }
                }
                """
        };
    }

    private static FileModel CreateAppSettingsFile(string directory)
    {
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "Distribution": {
                    "BaseIntervalMs": 100,
                    "EnableBatchMode": false,
                    "MaxBatchSize": 100
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
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Distribution.Api.Hubs;
                using Distribution.Api.Services;
                using Distribution.Core.Interfaces;
                using Distribution.Infrastructure;

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddSignalR();
                builder.Services.AddInfrastructureServices(builder.Configuration);

                // Register SignalR telemetry distributor
                builder.Services.AddSingleton<ITelemetryDistributor, SignalRTelemetryDistributor>();

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

                // Make Program class accessible for WebApplicationFactory
                public partial class Program { }
                """
        };
    }

    #endregion

    #region Test Files

    private static FileModel CreateWebApplicationFactoryFixtureFile(string directory)
    {
        return new FileModel("DistributionWebApplicationFactory", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Hosting;
                using Microsoft.AspNetCore.Mvc.Testing;
                using Microsoft.AspNetCore.SignalR.Client;
                using Microsoft.Extensions.DependencyInjection;
                using Microsoft.Extensions.Hosting;

                namespace Distribution.Tests.Fixtures;

                /// <summary>
                /// WebApplicationFactory for Distribution API integration tests.
                /// </summary>
                public class DistributionWebApplicationFactory : WebApplicationFactory<Program>
                {
                    protected override void ConfigureWebHost(IWebHostBuilder builder)
                    {
                        builder.UseEnvironment("Testing");

                        builder.ConfigureServices(services =>
                        {
                            // Remove the background service for controlled testing
                            var descriptor = services.SingleOrDefault(
                                d => d.ServiceType == typeof(IHostedService) &&
                                     d.ImplementationType?.Name == "TelemetryDistributionService");

                            if (descriptor != null)
                            {
                                services.Remove(descriptor);
                            }
                        });
                    }

                    /// <summary>
                    /// Creates a SignalR hub connection for testing.
                    /// </summary>
                    public HubConnection CreateHubConnection()
                    {
                        var server = Server;
                        var hubConnection = new HubConnectionBuilder()
                            .WithUrl(
                                new Uri(server.BaseAddress, "/telemetry"),
                                options =>
                                {
                                    options.HttpMessageHandlerFactory = _ => server.CreateHandler();
                                })
                            .Build();

                        return hubConnection;
                    }
                }
                """
        };
    }

    private static FileModel CreateTelemetryHubIntegrationTestsFile(string directory)
    {
        return new FileModel("TelemetryHubIntegrationTests", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Distribution.Core.DTOs;
                using Distribution.Core.Entities;
                using Distribution.Tests.Fixtures;
                using Microsoft.AspNetCore.SignalR.Client;
                using System.Net.Http.Json;
                using Xunit;

                namespace Distribution.Tests.Integration;

                /// <summary>
                /// Integration tests for the TelemetryHub using WebApplicationFactory.
                /// </summary>
                public class TelemetryHubIntegrationTests : IClassFixture<DistributionWebApplicationFactory>, IAsyncLifetime
                {
                    private readonly DistributionWebApplicationFactory factory;
                    private readonly HttpClient httpClient;
                    private HubConnection? hubConnection;

                    public TelemetryHubIntegrationTests(DistributionWebApplicationFactory factory)
                    {
                        this.factory = factory;
                        this.httpClient = factory.CreateClient();
                    }

                    public async Task InitializeAsync()
                    {
                        hubConnection = factory.CreateHubConnection();
                        await hubConnection.StartAsync();
                    }

                    public async Task DisposeAsync()
                    {
                        if (hubConnection != null)
                        {
                            await hubConnection.DisposeAsync();
                        }
                    }

                    [Fact]
                    public async Task Subscribe_ShouldReceiveConfirmation()
                    {
                        // Arrange
                        var confirmationReceived = new TaskCompletionSource<SubscriptionResponseDto>();

                        hubConnection!.On<SubscriptionResponseDto>("SubscriptionConfirmed", response =>
                        {
                            confirmationReceived.SetResult(response);
                        });

                        var request = new SubscriptionRequestDto
                        {
                            ClientId = "test-client-1",
                            Channels = new List<string> { "system", "application" },
                            MetricFilters = new List<string>(),
                            PushIntervalMs = 100
                        };

                        // Act
                        await hubConnection.InvokeAsync("Subscribe", request);

                        // Assert
                        var result = await confirmationReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
                        Assert.NotNull(result);
                        Assert.Equal("test-client-1", result.ConnectionId is null ? null : "test-client-1");
                        Assert.Contains("system", result.Channels);
                        Assert.Contains("application", result.Channels);
                        Assert.True(result.IsActive);
                    }

                    [Fact]
                    public async Task Unsubscribe_ShouldReceiveConfirmation()
                    {
                        // Arrange
                        var unsubscribeConfirmed = new TaskCompletionSource<bool>();

                        hubConnection!.On("UnsubscriptionConfirmed", () =>
                        {
                            unsubscribeConfirmed.SetResult(true);
                        });

                        // First subscribe
                        var subscribeConfirmed = new TaskCompletionSource<bool>();
                        hubConnection.On<SubscriptionResponseDto>("SubscriptionConfirmed", _ =>
                        {
                            subscribeConfirmed.SetResult(true);
                        });

                        await hubConnection.InvokeAsync("Subscribe", new SubscriptionRequestDto
                        {
                            ClientId = "test-client-2",
                            Channels = new List<string> { "system" }
                        });

                        await subscribeConfirmed.Task.WaitAsync(TimeSpan.FromSeconds(5));

                        // Act
                        await hubConnection.InvokeAsync("Unsubscribe");

                        // Assert
                        var result = await unsubscribeConfirmed.Task.WaitAsync(TimeSpan.FromSeconds(5));
                        Assert.True(result);
                    }

                    [Fact]
                    public async Task GetChannels_ShouldReturnAvailableChannels()
                    {
                        // Arrange
                        var channelsReceived = new TaskCompletionSource<IEnumerable<DistributionChannel>>();

                        hubConnection!.On<IEnumerable<DistributionChannel>>("ChannelList", channels =>
                        {
                            channelsReceived.SetResult(channels);
                        });

                        // Act
                        await hubConnection.InvokeAsync("GetChannels");

                        // Assert
                        var result = await channelsReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
                        Assert.NotNull(result);
                        Assert.NotEmpty(result);
                    }

                    [Fact]
                    public async Task GetMetrics_ShouldReturnMetricsForChannel()
                    {
                        // Arrange
                        var metricsReceived = new TaskCompletionSource<(string channel, IEnumerable<string> metrics)>();

                        hubConnection!.On<string, IEnumerable<string>>("MetricList", (channel, metrics) =>
                        {
                            metricsReceived.SetResult((channel, metrics));
                        });

                        // Act
                        await hubConnection.InvokeAsync("GetMetrics", "system");

                        // Assert
                        var result = await metricsReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
                        Assert.Equal("system", result.channel);
                        Assert.NotEmpty(result.metrics);
                        Assert.Contains("cpu_usage_percent", result.metrics);
                    }

                    [Fact]
                    public async Task Ping_ShouldReceivePong()
                    {
                        // Arrange
                        var pongReceived = new TaskCompletionSource<DateTime>();

                        hubConnection!.On<DateTime>("Pong", timestamp =>
                        {
                            pongReceived.SetResult(timestamp);
                        });

                        // Act
                        await hubConnection.InvokeAsync("Ping");

                        // Assert
                        var result = await pongReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
                        Assert.True(result <= DateTime.UtcNow);
                        Assert.True(result > DateTime.UtcNow.AddMinutes(-1));
                    }

                    [Fact]
                    public async Task UpdateSubscription_ShouldReceiveConfirmation()
                    {
                        // Arrange
                        var updateConfirmed = new TaskCompletionSource<bool>();

                        hubConnection!.On("SubscriptionUpdated", () =>
                        {
                            updateConfirmed.SetResult(true);
                        });

                        // First subscribe
                        var subscribeConfirmed = new TaskCompletionSource<bool>();
                        hubConnection.On<SubscriptionResponseDto>("SubscriptionConfirmed", _ =>
                        {
                            subscribeConfirmed.SetResult(true);
                        });

                        await hubConnection.InvokeAsync("Subscribe", new SubscriptionRequestDto
                        {
                            ClientId = "test-client-3",
                            Channels = new List<string> { "system" }
                        });

                        await subscribeConfirmed.Task.WaitAsync(TimeSpan.FromSeconds(5));

                        // Act
                        await hubConnection.InvokeAsync("UpdateSubscription", new SubscriptionRequestDto
                        {
                            ClientId = "test-client-3",
                            Channels = new List<string> { "application", "performance" },
                            MetricFilters = new List<string> { "cpu", "memory" }
                        });

                        // Assert
                        var result = await updateConfirmed.Task.WaitAsync(TimeSpan.FromSeconds(5));
                        Assert.True(result);
                    }

                    [Fact]
                    public async Task RestApi_GetChannels_ShouldReturnChannels()
                    {
                        // Act
                        var response = await httpClient.GetAsync("/api/Subscription/channels");

                        // Assert
                        response.EnsureSuccessStatusCode();
                        var channels = await response.Content.ReadFromJsonAsync<IEnumerable<DistributionChannel>>();
                        Assert.NotNull(channels);
                        Assert.NotEmpty(channels);
                    }

                    [Fact]
                    public async Task RestApi_GetMetrics_ShouldReturnMetrics()
                    {
                        // Act
                        var response = await httpClient.GetAsync("/api/Subscription/channels/system/metrics");

                        // Assert
                        response.EnsureSuccessStatusCode();
                        var metrics = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
                        Assert.NotNull(metrics);
                        Assert.NotEmpty(metrics);
                    }

                    [Fact]
                    public async Task RestApi_GetStats_ShouldReturnStatistics()
                    {
                        // Act
                        var response = await httpClient.GetAsync("/api/Subscription/stats");

                        // Assert
                        response.EnsureSuccessStatusCode();
                    }

                    [Fact]
                    public async Task MultipleClients_ShouldMaintainSeparateSubscriptions()
                    {
                        // Arrange
                        var connection1 = factory.CreateHubConnection();
                        var connection2 = factory.CreateHubConnection();

                        await connection1.StartAsync();
                        await connection2.StartAsync();

                        var confirmation1Received = new TaskCompletionSource<SubscriptionResponseDto>();
                        var confirmation2Received = new TaskCompletionSource<SubscriptionResponseDto>();

                        connection1.On<SubscriptionResponseDto>("SubscriptionConfirmed", response =>
                        {
                            confirmation1Received.SetResult(response);
                        });

                        connection2.On<SubscriptionResponseDto>("SubscriptionConfirmed", response =>
                        {
                            confirmation2Received.SetResult(response);
                        });

                        // Act
                        await connection1.InvokeAsync("Subscribe", new SubscriptionRequestDto
                        {
                            ClientId = "client-1",
                            Channels = new List<string> { "system" }
                        });

                        await connection2.InvokeAsync("Subscribe", new SubscriptionRequestDto
                        {
                            ClientId = "client-2",
                            Channels = new List<string> { "application" }
                        });

                        // Assert
                        var result1 = await confirmation1Received.Task.WaitAsync(TimeSpan.FromSeconds(5));
                        var result2 = await confirmation2Received.Task.WaitAsync(TimeSpan.FromSeconds(5));

                        Assert.NotEqual(result1.SubscriptionId, result2.SubscriptionId);
                        Assert.Contains("system", result1.Channels);
                        Assert.Contains("application", result2.Channels);

                        // Cleanup
                        await connection1.DisposeAsync();
                        await connection2.DisposeAsync();
                    }
                }
                """
        };
    }

    #endregion
}
