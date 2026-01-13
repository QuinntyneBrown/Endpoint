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

        // TelemetryMessage entity per REQ-STREAM-008 (Name, Ust, Value)
        project.Files.Add(new FileModel("TelemetryMessage", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.TelemetryStreaming.Core.Entities;

                /// <summary>
                /// Telemetry message with Name, Ust (UTC timestamp), and Value per REQ-STREAM-008.
                /// </summary>
                public class TelemetryMessage
                {
                    public Guid TelemetryMessageId { get; set; }

                    /// <summary>
                    /// Name of the telemetry metric (e.g., "PropulsionTemperature", "BatteryVoltage").
                    /// </summary>
                    public required string Name { get; set; }

                    /// <summary>
                    /// UTC timestamp when the telemetry was recorded.
                    /// </summary>
                    public DateTime Ust { get; set; } = DateTime.UtcNow;

                    /// <summary>
                    /// Value as string - can represent numeric, boolean, or enum values.
                    /// </summary>
                    public required string Value { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("TelemetrySubscription", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.TelemetryStreaming.Core.Entities;

                public class TelemetrySubscription
                {
                    public Guid SubscriptionId { get; set; }
                    public required string ConnectionId { get; set; }
                    public required string ClientId { get; set; }
                    public List<string> SubscribedMetrics { get; set; } = new List<string>();
                    public List<string> SubscribedSources { get; set; } = new List<string>();
                    public int UpdateRateMs { get; set; } = 200;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? LastUpdateAt { get; set; }
                    public bool IsActive { get; set; } = true;
                }
                """
        });

        // TelemetryTypes - 50 space vehicle telemetry types per REQ-STREAM-006
        project.Files.Add(new FileModel("SpaceVehicleTelemetryTypes", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.TelemetryStreaming.Core.Entities;

                /// <summary>
                /// 50 types of telemetry messages representing various space vehicle components per REQ-STREAM-006.
                /// </summary>
                public static class SpaceVehicleTelemetryTypes
                {
                    public static readonly string[] AllTypes = new[]
                    {
                        // Propulsion System (1-8)
                        "PropulsionMainEngineThrust",
                        "PropulsionMainEngineTemperature",
                        "PropulsionFuelPressure",
                        "PropulsionOxidizerPressure",
                        "PropulsionCombustionChamberTemp",
                        "PropulsionNozzleTemperature",
                        "PropulsionTurboPumpSpeed",
                        "PropulsionFuelFlowRate",

                        // Electrical Power System (9-16)
                        "PowerSolarPanelVoltage",
                        "PowerSolarPanelCurrent",
                        "PowerBatteryVoltage",
                        "PowerBatteryTemperature",
                        "PowerBatteryStateOfCharge",
                        "PowerBusVoltage",
                        "PowerLoadCurrent",
                        "PowerGenerationWatts",

                        // Thermal Control System (17-24)
                        "ThermalRadiatorTemperature",
                        "ThermalHeatPipeStatus",
                        "ThermalHeaterPower",
                        "ThermalCoolantFlowRate",
                        "ThermalMLITemperature",
                        "ThermalLouverPosition",
                        "ThermalHeatExchangerDelta",
                        "ThermalCryoCoolerTemp",

                        // Attitude Control System (25-32)
                        "AttitudeRollAngle",
                        "AttitudePitchAngle",
                        "AttitudeYawAngle",
                        "AttitudeRollRate",
                        "AttitudePitchRate",
                        "AttitudeYawRate",
                        "AttitudeReactionWheelSpeed",
                        "AttitudeThrusterFiring",

                        // Navigation & Guidance (33-40)
                        "NavPositionX",
                        "NavPositionY",
                        "NavPositionZ",
                        "NavVelocityX",
                        "NavVelocityY",
                        "NavVelocityZ",
                        "NavAltitude",
                        "NavGroundSpeed",

                        // Communications (41-46)
                        "CommSignalStrength",
                        "CommBitErrorRate",
                        "CommAntennaPointing",
                        "CommDataRate",
                        "CommTransmitterPower",
                        "CommReceiverSensitivity",

                        // Life Support (for crewed vehicles) (47-50)
                        "LifeSupportOxygenLevel",
                        "LifeSupportCO2Level",
                        "LifeSupportCabinPressure",
                        "LifeSupportCabinTemperature"
                    };

                    public static int Count => AllTypes.Length;
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("ITelemetryGenerator", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.TelemetryStreaming.Core.Entities;

                namespace EventMonitoring.TelemetryStreaming.Core.Interfaces;

                public interface ITelemetryGenerator
                {
                    TelemetryMessage Generate(string name);
                    IEnumerable<TelemetryMessage> GenerateBatch(IEnumerable<string> names);
                    IEnumerable<TelemetryMessage> GenerateAll();
                }
                """
        });

        project.Files.Add(new FileModel("ISubscriptionManager", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.TelemetryStreaming.Core.Entities;

                namespace EventMonitoring.TelemetryStreaming.Core.Interfaces;

                public interface ISubscriptionManager
                {
                    TelemetrySubscription CreateSubscription(string connectionId, string clientId);
                    void UpdateSubscription(Guid subscriptionId, List<string> metrics, List<string> sources, int updateRate);
                    TelemetrySubscription? GetSubscription(string connectionId);
                    void RemoveSubscription(string connectionId);
                    IEnumerable<TelemetrySubscription> GetActiveSubscriptions();
                    IEnumerable<TelemetrySubscription> GetSubscriptionsForMetric(string metricName);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("TelemetrySubscriptionCreatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.TelemetryStreaming.Core.Events;

                public record TelemetrySubscriptionCreatedEvent(Guid SubscriptionId, string ConnectionId, string ClientId, DateTime CreatedAt);
                """
        });

        project.Files.Add(new FileModel("TelemetryDataPublishedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.TelemetryStreaming.Core.Events;

                public record TelemetryDataPublishedEvent(string Name, string Value, DateTime Ust);
                """
        });

        // DTOs
        project.Files.Add(new FileModel("TelemetryMessageDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.TelemetryStreaming.Core.DTOs;

                /// <summary>
                /// DTO for telemetry message with Name, Ust, Value per REQ-STREAM-008.
                /// </summary>
                public class TelemetryMessageDto
                {
                    public required string Name { get; set; }
                    public DateTime Ust { get; set; }
                    public required string Value { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("SubscriptionRequestDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.TelemetryStreaming.Core.DTOs;

                public class SubscriptionRequestDto
                {
                    public required string ClientId { get; set; }
                    public List<string> Metrics { get; set; } = new List<string>();
                    public List<string> Sources { get; set; } = new List<string>();
                    public int UpdateRateMs { get; set; } = 200;
                }
                """
        });

        // ITelemetryPublisher interface for decoupling Infrastructure from SignalR
        project.Files.Add(new FileModel("ITelemetryPublisher", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.TelemetryStreaming.Core.DTOs;

                namespace EventMonitoring.TelemetryStreaming.Core.Interfaces;

                /// <summary>
                /// Interface for publishing telemetry to clients.
                /// </summary>
                public interface ITelemetryPublisher
                {
                    Task PublishToClientAsync(string connectionId, TelemetryMessageDto message, CancellationToken cancellationToken = default);
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding TelemetryStreaming.Infrastructure files");

        var servicesDir = Path.Combine(project.Directory, "Services");
        var backgroundServicesDir = Path.Combine(project.Directory, "BackgroundServices");

        // TelemetryGenerator - generates telemetry for 50 space vehicle types
        project.Files.Add(new FileModel("TelemetryGenerator", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.TelemetryStreaming.Core.Entities;
                using EventMonitoring.TelemetryStreaming.Core.Interfaces;

                namespace EventMonitoring.TelemetryStreaming.Infrastructure.Services;

                public class TelemetryGenerator : ITelemetryGenerator
                {
                    private readonly Random random = new();
                    private readonly Dictionary<string, Func<string>> valueGenerators;

                    public TelemetryGenerator()
                    {
                        valueGenerators = new Dictionary<string, Func<string>>
                        {
                            // Propulsion
                            { "PropulsionMainEngineThrust", () => (random.NextDouble() * 1000000 + 500000).ToString("F2") },
                            { "PropulsionMainEngineTemperature", () => (random.NextDouble() * 500 + 2500).ToString("F1") },
                            { "PropulsionFuelPressure", () => (random.NextDouble() * 50 + 200).ToString("F2") },
                            { "PropulsionOxidizerPressure", () => (random.NextDouble() * 50 + 200).ToString("F2") },
                            { "PropulsionCombustionChamberTemp", () => (random.NextDouble() * 200 + 3000).ToString("F1") },
                            { "PropulsionNozzleTemperature", () => (random.NextDouble() * 300 + 1500).ToString("F1") },
                            { "PropulsionTurboPumpSpeed", () => (random.NextDouble() * 5000 + 30000).ToString("F0") },
                            { "PropulsionFuelFlowRate", () => (random.NextDouble() * 100 + 200).ToString("F2") },

                            // Power
                            { "PowerSolarPanelVoltage", () => (random.NextDouble() * 5 + 28).ToString("F2") },
                            { "PowerSolarPanelCurrent", () => (random.NextDouble() * 10 + 15).ToString("F2") },
                            { "PowerBatteryVoltage", () => (random.NextDouble() * 2 + 26).ToString("F2") },
                            { "PowerBatteryTemperature", () => (random.NextDouble() * 20 + 15).ToString("F1") },
                            { "PowerBatteryStateOfCharge", () => (random.NextDouble() * 30 + 70).ToString("F1") },
                            { "PowerBusVoltage", () => (random.NextDouble() * 0.5 + 28).ToString("F2") },
                            { "PowerLoadCurrent", () => (random.NextDouble() * 5 + 10).ToString("F2") },
                            { "PowerGenerationWatts", () => (random.NextDouble() * 500 + 2000).ToString("F1") },

                            // Thermal
                            { "ThermalRadiatorTemperature", () => (random.NextDouble() * 50 - 20).ToString("F1") },
                            { "ThermalHeatPipeStatus", () => random.Next(0, 2).ToString() },
                            { "ThermalHeaterPower", () => (random.NextDouble() * 100).ToString("F1") },
                            { "ThermalCoolantFlowRate", () => (random.NextDouble() * 2 + 1).ToString("F2") },
                            { "ThermalMLITemperature", () => (random.NextDouble() * 100 - 150).ToString("F1") },
                            { "ThermalLouverPosition", () => (random.NextDouble() * 100).ToString("F1") },
                            { "ThermalHeatExchangerDelta", () => (random.NextDouble() * 10 + 5).ToString("F2") },
                            { "ThermalCryoCoolerTemp", () => (random.NextDouble() * 5 - 270).ToString("F2") },

                            // Attitude
                            { "AttitudeRollAngle", () => (random.NextDouble() * 360 - 180).ToString("F3") },
                            { "AttitudePitchAngle", () => (random.NextDouble() * 180 - 90).ToString("F3") },
                            { "AttitudeYawAngle", () => (random.NextDouble() * 360 - 180).ToString("F3") },
                            { "AttitudeRollRate", () => (random.NextDouble() * 2 - 1).ToString("F4") },
                            { "AttitudePitchRate", () => (random.NextDouble() * 2 - 1).ToString("F4") },
                            { "AttitudeYawRate", () => (random.NextDouble() * 2 - 1).ToString("F4") },
                            { "AttitudeReactionWheelSpeed", () => (random.NextDouble() * 6000 - 3000).ToString("F1") },
                            { "AttitudeThrusterFiring", () => random.Next(0, 2).ToString() },

                            // Navigation
                            { "NavPositionX", () => (random.NextDouble() * 1000000 - 500000).ToString("F2") },
                            { "NavPositionY", () => (random.NextDouble() * 1000000 - 500000).ToString("F2") },
                            { "NavPositionZ", () => (random.NextDouble() * 1000000 - 500000).ToString("F2") },
                            { "NavVelocityX", () => (random.NextDouble() * 100 - 50).ToString("F4") },
                            { "NavVelocityY", () => (random.NextDouble() * 100 - 50).ToString("F4") },
                            { "NavVelocityZ", () => (random.NextDouble() * 100 - 50).ToString("F4") },
                            { "NavAltitude", () => (random.NextDouble() * 100000 + 300000).ToString("F2") },
                            { "NavGroundSpeed", () => (random.NextDouble() * 1000 + 7000).ToString("F2") },

                            // Communications
                            { "CommSignalStrength", () => (random.NextDouble() * 30 - 100).ToString("F1") },
                            { "CommBitErrorRate", () => (random.NextDouble() * 0.001).ToString("E3") },
                            { "CommAntennaPointing", () => (random.NextDouble() * 2).ToString("F3") },
                            { "CommDataRate", () => (random.NextDouble() * 100 + 50).ToString("F1") },
                            { "CommTransmitterPower", () => (random.NextDouble() * 10 + 20).ToString("F1") },
                            { "CommReceiverSensitivity", () => (random.NextDouble() * 10 - 130).ToString("F1") },

                            // Life Support
                            { "LifeSupportOxygenLevel", () => (random.NextDouble() * 2 + 20).ToString("F2") },
                            { "LifeSupportCO2Level", () => (random.NextDouble() * 0.5 + 0.3).ToString("F3") },
                            { "LifeSupportCabinPressure", () => (random.NextDouble() * 2 + 100).ToString("F2") },
                            { "LifeSupportCabinTemperature", () => (random.NextDouble() * 5 + 20).ToString("F1") }
                        };
                    }

                    public TelemetryMessage Generate(string name)
                    {
                        var value = valueGenerators.TryGetValue(name, out var generator)
                            ? generator()
                            : random.NextDouble().ToString("F4");

                        return new TelemetryMessage
                        {
                            TelemetryMessageId = Guid.NewGuid(),
                            Name = name,
                            Ust = DateTime.UtcNow,
                            Value = value
                        };
                    }

                    public IEnumerable<TelemetryMessage> GenerateBatch(IEnumerable<string> names)
                    {
                        return names.Select(Generate).ToList();
                    }

                    public IEnumerable<TelemetryMessage> GenerateAll()
                    {
                        return GenerateBatch(SpaceVehicleTelemetryTypes.AllTypes);
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
                using EventMonitoring.TelemetryStreaming.Core.Entities;
                using EventMonitoring.TelemetryStreaming.Core.Interfaces;

                namespace EventMonitoring.TelemetryStreaming.Infrastructure.Services;

                /// <summary>
                /// In-memory subscription manager per REQ-STREAM-003.
                /// </summary>
                public class SubscriptionManager : ISubscriptionManager
                {
                    private readonly ConcurrentDictionary<string, TelemetrySubscription> subscriptions = new();

                    public TelemetrySubscription CreateSubscription(string connectionId, string clientId)
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
                        return subscription;
                    }

                    public void UpdateSubscription(Guid subscriptionId, List<string> metrics, List<string> sources, int updateRate)
                    {
                        var subscription = subscriptions.Values.FirstOrDefault(s => s.SubscriptionId == subscriptionId);
                        if (subscription != null)
                        {
                            subscription.SubscribedMetrics = metrics;
                            subscription.SubscribedSources = sources;
                            subscription.UpdateRateMs = updateRate;
                            subscription.LastUpdateAt = DateTime.UtcNow;
                        }
                    }

                    public TelemetrySubscription? GetSubscription(string connectionId)
                    {
                        subscriptions.TryGetValue(connectionId, out var subscription);
                        return subscription;
                    }

                    public void RemoveSubscription(string connectionId)
                    {
                        subscriptions.TryRemove(connectionId, out _);
                    }

                    public IEnumerable<TelemetrySubscription> GetActiveSubscriptions()
                    {
                        return subscriptions.Values.Where(s => s.IsActive);
                    }

                    public IEnumerable<TelemetrySubscription> GetSubscriptionsForMetric(string metricName)
                    {
                        return subscriptions.Values.Where(s =>
                            s.IsActive &&
                            (s.SubscribedMetrics.Count == 0 || s.SubscribedMetrics.Contains(metricName)));
                    }
                }
                """
        });

        // Background Service for cyclic telemetry generation at 200ms (5Hz) per REQ-STREAM-004
        project.Files.Add(new FileModel("TelemetryPublisherService", backgroundServicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.Extensions.Hosting;
                using Microsoft.Extensions.Logging;
                using Microsoft.Extensions.Options;
                using EventMonitoring.TelemetryStreaming.Core.DTOs;
                using EventMonitoring.TelemetryStreaming.Core.Interfaces;

                namespace EventMonitoring.TelemetryStreaming.Infrastructure.BackgroundServices;

                /// <summary>
                /// Background service that cyclically generates telemetry per REQ-STREAM-004.
                /// Default rate is 200ms (5Hz).
                /// </summary>
                public class TelemetryPublisherService : BackgroundService
                {
                    private readonly ILogger<TelemetryPublisherService> logger;
                    private readonly ITelemetryGenerator telemetryGenerator;
                    private readonly ISubscriptionManager subscriptionManager;
                    private readonly ITelemetryPublisher telemetryPublisher;
                    private readonly TelemetryStreamingOptions options;

                    public TelemetryPublisherService(
                        ILogger<TelemetryPublisherService> logger,
                        ITelemetryGenerator telemetryGenerator,
                        ISubscriptionManager subscriptionManager,
                        ITelemetryPublisher telemetryPublisher,
                        IOptions<TelemetryStreamingOptions> options)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.telemetryGenerator = telemetryGenerator ?? throw new ArgumentNullException(nameof(telemetryGenerator));
                        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
                        this.telemetryPublisher = telemetryPublisher ?? throw new ArgumentNullException(nameof(telemetryPublisher));
                        this.options = options?.Value ?? new TelemetryStreamingOptions();
                    }

                    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
                    {
                        logger.LogInformation("Telemetry Publisher Service starting with {Rate}ms generation rate", options.GenerationRateMs);

                        while (!stoppingToken.IsCancellationRequested)
                        {
                            try
                            {
                                // Generate all telemetry
                                var allMessages = telemetryGenerator.GenerateAll();

                                // Send to each subscribed client based on their filters per REQ-STREAM-007
                                foreach (var message in allMessages)
                                {
                                    var subscribers = subscriptionManager.GetSubscriptionsForMetric(message.Name);

                                    foreach (var subscription in subscribers)
                                    {
                                        var dto = new TelemetryMessageDto
                                        {
                                            Name = message.Name,
                                            Ust = message.Ust,
                                            Value = message.Value
                                        };

                                        await telemetryPublisher.PublishToClientAsync(subscription.ConnectionId, dto, stoppingToken);
                                    }
                                }

                                // Wait for next cycle - default 200ms (5Hz) per REQ-STREAM-004
                                await Task.Delay(options.GenerationRateMs, stoppingToken);
                            }
                            catch (OperationCanceledException)
                            {
                                // Expected when stopping
                                break;
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error occurred while publishing telemetry");
                                await Task.Delay(1000, stoppingToken); // Back off on error
                            }
                        }

                        logger.LogInformation("Telemetry Publisher Service stopped");
                    }
                }

                public class TelemetryStreamingOptions
                {
                    /// <summary>
                    /// Telemetry generation rate in milliseconds. Default is 200ms (5Hz) per REQ-STREAM-004.
                    /// </summary>
                    public int GenerationRateMs { get; set; } = 200;
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
                using EventMonitoring.TelemetryStreaming.Core.Interfaces;
                using EventMonitoring.TelemetryStreaming.Infrastructure.BackgroundServices;
                using EventMonitoring.TelemetryStreaming.Infrastructure.Services;

                namespace EventMonitoring.TelemetryStreaming.Infrastructure;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.Configure<TelemetryStreamingOptions>(configuration.GetSection("TelemetryStreaming"));

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
        var servicesDir = Path.Combine(project.Directory, "Services");

        // SignalRTelemetryPublisher - implements ITelemetryPublisher using SignalR
        project.Files.Add(new FileModel("SignalRTelemetryPublisher", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.SignalR;
                using EventMonitoring.TelemetryStreaming.Api.Hubs;
                using EventMonitoring.TelemetryStreaming.Core.DTOs;
                using EventMonitoring.TelemetryStreaming.Core.Interfaces;

                namespace EventMonitoring.TelemetryStreaming.Api.Services;

                /// <summary>
                /// SignalR implementation of ITelemetryPublisher.
                /// </summary>
                public class SignalRTelemetryPublisher : ITelemetryPublisher
                {
                    private readonly IHubContext<TelemetryHub> hubContext;

                    public SignalRTelemetryPublisher(IHubContext<TelemetryHub> hubContext)
                    {
                        this.hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
                    }

                    public async Task PublishToClientAsync(string connectionId, TelemetryMessageDto message, CancellationToken cancellationToken = default)
                    {
                        await hubContext.Clients.Client(connectionId).SendAsync("ReceiveTelemetry", message, cancellationToken);
                    }
                }
                """
        });

        // TelemetryHub mapped to /telemetry per REQ-STREAM-001
        project.Files.Add(new FileModel("TelemetryHub", hubsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.SignalR;
                using EventMonitoring.TelemetryStreaming.Core.DTOs;
                using EventMonitoring.TelemetryStreaming.Core.Interfaces;

                namespace EventMonitoring.TelemetryStreaming.Api.Hubs;

                /// <summary>
                /// SignalR hub for telemetry streaming per REQ-STREAM-001.
                /// Mapped to /telemetry endpoint.
                /// </summary>
                public class TelemetryHub : Hub
                {
                    private readonly ILogger<TelemetryHub> logger;
                    private readonly ISubscriptionManager subscriptionManager;

                    public TelemetryHub(
                        ILogger<TelemetryHub> logger,
                        ISubscriptionManager subscriptionManager)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
                    }

                    public override async Task OnConnectedAsync()
                    {
                        logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
                        await base.OnConnectedAsync();
                    }

                    public override async Task OnDisconnectedAsync(Exception? exception)
                    {
                        logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
                        subscriptionManager.RemoveSubscription(Context.ConnectionId);
                        await base.OnDisconnectedAsync(exception);
                    }

                    /// <summary>
                    /// Subscribe to telemetry per REQ-STREAM-002.
                    /// </summary>
                    public async Task Subscribe(SubscriptionRequestDto request)
                    {
                        logger.LogInformation("Client {ClientId} subscribing to metrics: {Metrics}",
                            request.ClientId, string.Join(", ", request.Metrics));

                        var subscription = subscriptionManager.CreateSubscription(Context.ConnectionId, request.ClientId);
                        subscriptionManager.UpdateSubscription(
                            subscription.SubscriptionId,
                            request.Metrics,
                            request.Sources,
                            request.UpdateRateMs);

                        await Clients.Caller.SendAsync("SubscriptionConfirmed", subscription.SubscriptionId);
                    }

                    /// <summary>
                    /// Unsubscribe from telemetry per REQ-STREAM-002.
                    /// </summary>
                    public async Task Unsubscribe()
                    {
                        logger.LogInformation("Client {ConnectionId} unsubscribing from telemetry", Context.ConnectionId);
                        subscriptionManager.RemoveSubscription(Context.ConnectionId);
                        await Clients.Caller.SendAsync("UnsubscriptionConfirmed");
                    }

                    /// <summary>
                    /// Update existing subscription.
                    /// </summary>
                    public async Task UpdateSubscription(SubscriptionRequestDto request)
                    {
                        logger.LogInformation("Client updating subscription: {ConnectionId}", Context.ConnectionId);
                        var subscription = subscriptionManager.GetSubscription(Context.ConnectionId);

                        if (subscription != null)
                        {
                            subscriptionManager.UpdateSubscription(
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

        // appsettings.json with TelemetryStreaming configuration
        project.Files.Add(new FileModel("appsettings", project.Directory, ".json")
        {
            Body = """
                {
                  "TelemetryStreaming": {
                    "GenerationRateMs": 200
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

        // Program.cs
        project.Files.Add(new FileModel("Program", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.TelemetryStreaming.Api.Hubs;
                using EventMonitoring.TelemetryStreaming.Api.Services;
                using EventMonitoring.TelemetryStreaming.Core.Interfaces;
                using EventMonitoring.TelemetryStreaming.Infrastructure;

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddSignalR();
                builder.Services.AddInfrastructureServices(builder.Configuration);

                // Register SignalR telemetry publisher
                builder.Services.AddSingleton<ITelemetryPublisher, SignalRTelemetryPublisher>();

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
