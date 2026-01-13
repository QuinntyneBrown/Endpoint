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

namespace Endpoint.Engineering.Microservices.TelemetryStreaming;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

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
        project.Files.Add(CreateTelemetryMessageFile(entitiesDir));
        project.Files.Add(CreateTelemetrySubscriptionFile(entitiesDir));
        project.Files.Add(CreateSpaceVehicleTelemetryTypesFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateITelemetryGeneratorFile(interfacesDir));
        project.Files.Add(CreateISubscriptionManagerFile(interfacesDir));
        project.Files.Add(CreateITelemetryPublisherFile(interfacesDir));

        // Events (records - keep as FileModel)
        project.Files.Add(CreateTelemetrySubscriptionCreatedEventFile(eventsDir));
        project.Files.Add(CreateTelemetryDataPublishedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateTelemetryMessageDtoFile(dtosDir));
        project.Files.Add(CreateSubscriptionRequestDtoFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding TelemetryStreaming.Infrastructure files");

        var servicesDir = Path.Combine(project.Directory, "Services");
        var backgroundServicesDir = Path.Combine(project.Directory, "BackgroundServices");

        // Services
        project.Files.Add(CreateTelemetryGeneratorFile(servicesDir));
        project.Files.Add(CreateSubscriptionManagerFile(servicesDir));

        // Background Services
        project.Files.Add(CreateTelemetryPublisherServiceFile(backgroundServicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding TelemetryStreaming.Api files");

        var hubsDir = Path.Combine(project.Directory, "Hubs");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // Services
        project.Files.Add(CreateSignalRTelemetryPublisherFile(servicesDir));

        // Hubs
        project.Files.Add(CreateTelemetryHubFile(hubsDir));

        // Configuration files (keep as FileModel)
        project.Files.Add(CreateAppSettingsFile(project.Directory));
        project.Files.Add(CreateProgramFile(project.Directory));
    }

    #region Core Layer Files

    private static CodeFileModel<ClassModel> CreateTelemetryMessageFile(string directory)
    {
        var classModel = new ClassModel("TelemetryMessage");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TelemetryMessageId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Ust", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));

        return new CodeFileModel<ClassModel>(classModel, "TelemetryMessage", directory, CSharp)
        {
            Namespace = "EventMonitoring.TelemetryStreaming.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateTelemetrySubscriptionFile(string directory)
    {
        var classModel = new ClassModel("TelemetrySubscription");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SubscriptionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ConnectionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ClientId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("List") { GenericTypeParameters = [new TypeModel("string")] }, "SubscribedMetrics", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<string>()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("List") { GenericTypeParameters = [new TypeModel("string")] }, "SubscribedSources", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<string>()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "UpdateRateMs", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "200" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "LastUpdateAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });

        return new CodeFileModel<ClassModel>(classModel, "TelemetrySubscription", directory, CSharp)
        {
            Namespace = "EventMonitoring.TelemetryStreaming.Core.Entities"
        };
    }

    private static FileModel CreateSpaceVehicleTelemetryTypesFile(string directory)
    {
        // Static class with complex array initialization - keep as FileModel
        return new FileModel("SpaceVehicleTelemetryTypes", directory, CSharp)
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
        };
    }

    private static CodeFileModel<InterfaceModel> CreateITelemetryGeneratorFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ITelemetryGenerator");

        interfaceModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "Generate",
            Interface = true,
            ReturnType = new TypeModel("TelemetryMessage"),
            Params = [new ParamModel { Name = "name", Type = new TypeModel("string") }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GenerateBatch",
            Interface = true,
            ReturnType = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TelemetryMessage")] },
            Params = [new ParamModel { Name = "names", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")] } }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GenerateAll",
            Interface = true,
            ReturnType = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TelemetryMessage")] }
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ITelemetryGenerator", directory, CSharp)
        {
            Namespace = "EventMonitoring.TelemetryStreaming.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateISubscriptionManagerFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ISubscriptionManager");

        interfaceModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CreateSubscription",
            Interface = true,
            ReturnType = new TypeModel("TelemetrySubscription"),
            Params =
            [
                new ParamModel { Name = "connectionId", Type = new TypeModel("string") },
                new ParamModel { Name = "clientId", Type = new TypeModel("string") }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateSubscription",
            Interface = true,
            ReturnType = new TypeModel("void"),
            Params =
            [
                new ParamModel { Name = "subscriptionId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "metrics", Type = new TypeModel("List") { GenericTypeParameters = [new TypeModel("string")] } },
                new ParamModel { Name = "sources", Type = new TypeModel("List") { GenericTypeParameters = [new TypeModel("string")] } },
                new ParamModel { Name = "updateRate", Type = new TypeModel("int") }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetSubscription",
            Interface = true,
            ReturnType = new TypeModel("TelemetrySubscription") { Nullable = true },
            Params = [new ParamModel { Name = "connectionId", Type = new TypeModel("string") }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RemoveSubscription",
            Interface = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "connectionId", Type = new TypeModel("string") }]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetActiveSubscriptions",
            Interface = true,
            ReturnType = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TelemetrySubscription")] }
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetSubscriptionsForMetric",
            Interface = true,
            ReturnType = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TelemetrySubscription")] },
            Params = [new ParamModel { Name = "metricName", Type = new TypeModel("string") }]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ISubscriptionManager", directory, CSharp)
        {
            Namespace = "EventMonitoring.TelemetryStreaming.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateITelemetryPublisherFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ITelemetryPublisher");

        interfaceModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Core.DTOs"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "PublishToClientAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "connectionId", Type = new TypeModel("string") },
                new ParamModel { Name = "message", Type = new TypeModel("TelemetryMessageDto") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ITelemetryPublisher", directory, CSharp)
        {
            Namespace = "EventMonitoring.TelemetryStreaming.Core.Interfaces"
        };
    }

    private static FileModel CreateTelemetrySubscriptionCreatedEventFile(string directory)
    {
        // Record - keep as FileModel
        return new FileModel("TelemetrySubscriptionCreatedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.TelemetryStreaming.Core.Events;

                public record TelemetrySubscriptionCreatedEvent(Guid SubscriptionId, string ConnectionId, string ClientId, DateTime CreatedAt);
                """
        };
    }

    private static FileModel CreateTelemetryDataPublishedEventFile(string directory)
    {
        // Record - keep as FileModel
        return new FileModel("TelemetryDataPublishedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.TelemetryStreaming.Core.Events;

                public record TelemetryDataPublishedEvent(string Name, string Value, DateTime Ust);
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateTelemetryMessageDtoFile(string directory)
    {
        var classModel = new ClassModel("TelemetryMessageDto");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Ust", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));

        return new CodeFileModel<ClassModel>(classModel, "TelemetryMessageDto", directory, CSharp)
        {
            Namespace = "EventMonitoring.TelemetryStreaming.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateSubscriptionRequestDtoFile(string directory)
    {
        var classModel = new ClassModel("SubscriptionRequestDto");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ClientId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("List") { GenericTypeParameters = [new TypeModel("string")] }, "Metrics", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<string>()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("List") { GenericTypeParameters = [new TypeModel("string")] }, "Sources", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<string>()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "UpdateRateMs", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "200" });

        return new CodeFileModel<ClassModel>(classModel, "SubscriptionRequestDto", directory, CSharp)
        {
            Namespace = "EventMonitoring.TelemetryStreaming.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static FileModel CreateTelemetryGeneratorFile(string directory)
    {
        // Complex dictionary initialization - keep as FileModel
        return new FileModel("TelemetryGenerator", directory, CSharp)
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
        };
    }

    private static CodeFileModel<ClassModel> CreateSubscriptionManagerFile(string directory)
    {
        var classModel = new ClassModel("SubscriptionManager");

        classModel.Usings.Add(new UsingModel("System.Collections.Concurrent"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Core.Entities"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ISubscriptionManager"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "subscriptions",
            Type = new TypeModel("ConcurrentDictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("TelemetrySubscription")] },
            AccessModifier = AccessModifier.Private,
            ReadOnly = true,
            DefaultValue = "new()"
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CreateSubscription",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("TelemetrySubscription"),
            Params =
            [
                new ParamModel { Name = "connectionId", Type = new TypeModel("string") },
                new ParamModel { Name = "clientId", Type = new TypeModel("string") }
            ],
            Body = new ExpressionModel(@"var subscription = new TelemetrySubscription
        {
            SubscriptionId = Guid.NewGuid(),
            ConnectionId = connectionId,
            ClientId = clientId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        subscriptions.TryAdd(connectionId, subscription);
        return subscription;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateSubscription",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params =
            [
                new ParamModel { Name = "subscriptionId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "metrics", Type = new TypeModel("List") { GenericTypeParameters = [new TypeModel("string")] } },
                new ParamModel { Name = "sources", Type = new TypeModel("List") { GenericTypeParameters = [new TypeModel("string")] } },
                new ParamModel { Name = "updateRate", Type = new TypeModel("int") }
            ],
            Body = new ExpressionModel(@"var subscription = subscriptions.Values.FirstOrDefault(s => s.SubscriptionId == subscriptionId);
        if (subscription != null)
        {
            subscription.SubscribedMetrics = metrics;
            subscription.SubscribedSources = sources;
            subscription.UpdateRateMs = updateRate;
            subscription.LastUpdateAt = DateTime.UtcNow;
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetSubscription",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("TelemetrySubscription") { Nullable = true },
            Params = [new ParamModel { Name = "connectionId", Type = new TypeModel("string") }],
            Body = new ExpressionModel(@"subscriptions.TryGetValue(connectionId, out var subscription);
        return subscription;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "RemoveSubscription",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "connectionId", Type = new TypeModel("string") }],
            Body = new ExpressionModel("subscriptions.TryRemove(connectionId, out _);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetActiveSubscriptions",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TelemetrySubscription")] },
            Body = new ExpressionModel("return subscriptions.Values.Where(s => s.IsActive);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetSubscriptionsForMetric",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("TelemetrySubscription")] },
            Params = [new ParamModel { Name = "metricName", Type = new TypeModel("string") }],
            Body = new ExpressionModel(@"return subscriptions.Values.Where(s =>
            s.IsActive &&
            (s.SubscribedMetrics.Count == 0 || s.SubscribedMetrics.Contains(metricName)));")
        });

        return new CodeFileModel<ClassModel>(classModel, "SubscriptionManager", directory, CSharp)
        {
            Namespace = "EventMonitoring.TelemetryStreaming.Infrastructure.Services"
        };
    }

    private static FileModel CreateTelemetryPublisherServiceFile(string directory)
    {
        // Contains multiple classes (TelemetryPublisherService + TelemetryStreamingOptions) - keep as FileModel
        return new FileModel("TelemetryPublisherService", directory, CSharp)
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
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.DependencyInjection"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Infrastructure.BackgroundServices"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddInfrastructureServices",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.Configure<TelemetryStreamingOptions>(configuration.GetSection(""TelemetryStreaming""));

        services.AddSingleton<ITelemetryGenerator, TelemetryGenerator>();
        services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
        services.AddHostedService<TelemetryPublisherService>();

        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "EventMonitoring.TelemetryStreaming.Infrastructure"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateSignalRTelemetryPublisherFile(string directory)
    {
        var classModel = new ClassModel("SignalRTelemetryPublisher");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.SignalR"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Api.Hubs"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ITelemetryPublisher"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "hubContext",
            Type = new TypeModel("IHubContext") { GenericTypeParameters = [new TypeModel("TelemetryHub")] },
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "SignalRTelemetryPublisher")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "hubContext", Type = new TypeModel("IHubContext") { GenericTypeParameters = [new TypeModel("TelemetryHub")] } }],
            Body = new ExpressionModel("this.hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "PublishToClientAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "connectionId", Type = new TypeModel("string") },
                new ParamModel { Name = "message", Type = new TypeModel("TelemetryMessageDto") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("await hubContext.Clients.Client(connectionId).SendAsync(\"ReceiveTelemetry\", message, cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "SignalRTelemetryPublisher", directory, CSharp)
        {
            Namespace = "EventMonitoring.TelemetryStreaming.Api.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateTelemetryHubFile(string directory)
    {
        var classModel = new ClassModel("TelemetryHub");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.SignalR"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.TelemetryStreaming.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("Hub"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "logger",
            Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("TelemetryHub")] },
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        classModel.Fields.Add(new FieldModel
        {
            Name = "subscriptionManager",
            Type = new TypeModel("ISubscriptionManager"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "TelemetryHub")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("TelemetryHub")] } },
                new ParamModel { Name = "subscriptionManager", Type = new TypeModel("ISubscriptionManager") }
            ],
            Body = new ExpressionModel(@"this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnConnectedAsync",
            AccessModifier = AccessModifier.Public,
            Override = true,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Body = new ExpressionModel(@"logger.LogInformation(""Client connected: {ConnectionId}"", Context.ConnectionId);
        await base.OnConnectedAsync();")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnDisconnectedAsync",
            AccessModifier = AccessModifier.Public,
            Override = true,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "exception", Type = new TypeModel("Exception") { Nullable = true } }],
            Body = new ExpressionModel(@"logger.LogInformation(""Client disconnected: {ConnectionId}"", Context.ConnectionId);
        subscriptionManager.RemoveSubscription(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Subscribe",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "request", Type = new TypeModel("SubscriptionRequestDto") }],
            Body = new ExpressionModel(@"logger.LogInformation(""Client {ClientId} subscribing to metrics: {Metrics}"",
            request.ClientId, string.Join("", "", request.Metrics));

        var subscription = subscriptionManager.CreateSubscription(Context.ConnectionId, request.ClientId);
        subscriptionManager.UpdateSubscription(
            subscription.SubscriptionId,
            request.Metrics,
            request.Sources,
            request.UpdateRateMs);

        await Clients.Caller.SendAsync(""SubscriptionConfirmed"", subscription.SubscriptionId);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Unsubscribe",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Body = new ExpressionModel(@"logger.LogInformation(""Client {ConnectionId} unsubscribing from telemetry"", Context.ConnectionId);
        subscriptionManager.RemoveSubscription(Context.ConnectionId);
        await Clients.Caller.SendAsync(""UnsubscriptionConfirmed"");")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateSubscription",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "request", Type = new TypeModel("SubscriptionRequestDto") }],
            Body = new ExpressionModel(@"logger.LogInformation(""Client updating subscription: {ConnectionId}"", Context.ConnectionId);
        var subscription = subscriptionManager.GetSubscription(Context.ConnectionId);

        if (subscription != null)
        {
            subscriptionManager.UpdateSubscription(
                subscription.SubscriptionId,
                request.Metrics,
                request.Sources,
                request.UpdateRateMs);

            await Clients.Caller.SendAsync(""SubscriptionUpdated"");
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "TelemetryHub", directory, CSharp)
        {
            Namespace = "EventMonitoring.TelemetryStreaming.Api.Hubs"
        };
    }

    private static FileModel CreateAppSettingsFile(string directory)
    {
        // JSON file - keep as FileModel
        return new FileModel("appsettings", directory, ".json")
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
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        // Top-level statements - keep as FileModel
        return new FileModel("Program", directory, CSharp)
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
        };
    }

    #endregion
}
