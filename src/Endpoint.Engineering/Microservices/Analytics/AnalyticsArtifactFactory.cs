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

namespace Endpoint.Engineering.Microservices.Analytics;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

/// <summary>
/// Factory for creating Analytics microservice artifacts according to analytics-microservice.spec.md.
/// </summary>
public class AnalyticsArtifactFactory : IAnalyticsArtifactFactory
{
    private readonly ILogger<AnalyticsArtifactFactory> logger;

    public AnalyticsArtifactFactory(ILogger<AnalyticsArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Analytics.Core files");

        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");

        // Entities
        project.Files.Add(CreateIAggregateRootFile(entitiesDir));
        project.Files.Add(CreateEventEntityFile(entitiesDir));
        project.Files.Add(CreateMetricEntityFile(entitiesDir));
        project.Files.Add(CreateReportEntityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIDomainEventFile(interfacesDir));
        project.Files.Add(CreateIEventRepositoryFile(interfacesDir));
        project.Files.Add(CreateIMetricsServiceFile(interfacesDir));
        project.Files.Add(CreateIReportGeneratorFile(interfacesDir));

        // Events
        project.Files.Add(CreateEventTrackedEventFile(eventsDir));
        project.Files.Add(CreateReportGeneratedEventFile(eventsDir));
        project.Files.Add(CreateMetricThresholdExceededEventFile(eventsDir));

        // DTOs
        var dtosDir = Path.Combine(project.Directory, "DTOs");
        project.Files.Add(CreateEventDtoFile(dtosDir));
        project.Files.Add(CreateMetricDtoFile(dtosDir));
        project.Files.Add(CreateReportDtoFile(dtosDir));
        project.Files.Add(CreateTrackEventRequestFile(dtosDir));
        project.Files.Add(CreateMetricsQueryRequestFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Analytics.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // DbContext
        project.Files.Add(CreateAnalyticsDbContextFile(dataDir));

        // Entity Configurations
        project.Files.Add(CreateEventConfigurationFile(configurationsDir));
        project.Files.Add(CreateMetricConfigurationFile(configurationsDir));
        project.Files.Add(CreateReportConfigurationFile(configurationsDir));

        // Repositories
        project.Files.Add(CreateEventRepositoryFile(repositoriesDir));

        // Services
        project.Files.Add(CreateMetricsServiceFile(servicesDir));
        project.Files.Add(CreateReportGeneratorFile(servicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Analytics.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Controllers
        project.Files.Add(CreateEventsControllerFile(controllersDir));
        project.Files.Add(CreateMetricsControllerFile(controllersDir));
        project.Files.Add(CreateReportsControllerFile(controllersDir));

        // Program.cs
        project.Files.Add(CreateProgramFile(project.Directory));

        // appsettings.json
        project.Files.Add(CreateAppSettingsFile(project.Directory));
        project.Files.Add(CreateAppSettingsDevelopmentFile(project.Directory));
    }

    #region Core Layer Files

    private static CodeFileModel<InterfaceModel> CreateIAggregateRootFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IAggregateRoot");

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IAggregateRoot", directory, CSharp)
        {
            Namespace = "Analytics.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateEventEntityFile(string directory)
    {
        var classModel = new ClassModel("Event");

        classModel.Implements.Add(new TypeModel("IAggregateRoot"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EventId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EventType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Source", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "SessionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { Nullable = true, GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] }, "Properties", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Timestamp", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "Event", directory, CSharp)
        {
            Namespace = "Analytics.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateMetricEntityFile(string directory)
    {
        var classModel = new ClassModel("Metric");

        classModel.Implements.Add(new TypeModel("IAggregateRoot"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MetricId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Category", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Unit", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { Nullable = true, GenericTypeParameters = [new TypeModel("string"), new TypeModel("string")] }, "Tags", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Timestamp", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "Metric", directory, CSharp)
        {
            Namespace = "Analytics.Core.Entities"
        };
    }

    private static FileModel CreateReportEntityFile(string directory)
    {
        // Keep as FileModel - contains enum which can't be expressed with syntax models
        return new FileModel("Report", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Analytics.Core.Entities;

                /// <summary>
                /// Report entity representing a generated analytics report.
                /// </summary>
                public class Report : IAggregateRoot
                {
                    public Guid ReportId { get; set; }

                    public required string Name { get; set; }

                    public required string ReportType { get; set; }

                    public string? Description { get; set; }

                    public DateTime StartDate { get; set; }

                    public DateTime EndDate { get; set; }

                    public string? GeneratedBy { get; set; }

                    public string? Data { get; set; }

                    public ReportStatus Status { get; set; } = ReportStatus.Pending;

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? CompletedAt { get; set; }
                }

                /// <summary>
                /// Report generation status.
                /// </summary>
                public enum ReportStatus
                {
                    Pending,
                    Processing,
                    Completed,
                    Failed
                }
                """
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIDomainEventFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IDomainEvent");

        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IDomainEvent", directory, CSharp)
        {
            Namespace = "Analytics.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIEventRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IEventRepository");

        interfaceModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Event") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "eventId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByTypeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Event")] }] },
            Params =
            [
                new ParamModel { Name = "eventType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByDateRangeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Event")] }] },
            Params =
            [
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Event")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Event")] }] },
            Params =
            [
                new ParamModel { Name = "skip", Type = new TypeModel("int"), DefaultValue = "0" },
                new ParamModel { Name = "take", Type = new TypeModel("int"), DefaultValue = "100" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Event")] },
            Params =
            [
                new ParamModel { Name = "analyticsEvent", Type = new TypeModel("Event") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddBatchAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "events", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Event")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetCountAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("long")] },
            Params =
            [
                new ParamModel { Name = "eventType", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IEventRepository", directory, CSharp)
        {
            Namespace = "Analytics.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIMetricsServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IMetricsService");

        interfaceModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RecordMetricAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Metric")] },
            Params =
            [
                new ParamModel { Name = "metric", Type = new TypeModel("Metric") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetMetricsByNameAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Metric")] }] },
            Params =
            [
                new ParamModel { Name = "name", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetMetricsByCategoryAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Metric")] }] },
            Params =
            [
                new ParamModel { Name = "category", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetMetricsByDateRangeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Metric")] }] },
            Params =
            [
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAverageAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("double")] },
            Params =
            [
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetSumAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("double")] },
            Params =
            [
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetMinAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("double")] },
            Params =
            [
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetMaxAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("double")] },
            Params =
            [
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CheckThresholdsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IMetricsService", directory, CSharp)
        {
            Namespace = "Analytics.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIReportGeneratorFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IReportGenerator");

        interfaceModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GenerateReportAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Report")] },
            Params =
            [
                new ParamModel { Name = "name", Type = new TypeModel("string") },
                new ParamModel { Name = "reportType", Type = new TypeModel("string") },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "generatedBy", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetReportByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Report") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "reportId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetReportsByTypeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Report")] }] },
            Params =
            [
                new ParamModel { Name = "reportType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllReportsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Report")] }] },
            Params =
            [
                new ParamModel { Name = "skip", Type = new TypeModel("int"), DefaultValue = "0" },
                new ParamModel { Name = "take", Type = new TypeModel("int"), DefaultValue = "100" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateReportStatusAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Report")] },
            Params =
            [
                new ParamModel { Name = "reportId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "status", Type = new TypeModel("ReportStatus") },
                new ParamModel { Name = "data", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IReportGenerator", directory, CSharp)
        {
            Namespace = "Analytics.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateEventTrackedEventFile(string directory)
    {
        var classModel = new ClassModel("EventTrackedEvent")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("Analytics.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IDomainEvent"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EventType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Source", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "EventTrackedEvent", directory, CSharp)
        {
            Namespace = "Analytics.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateReportGeneratedEventFile(string directory)
    {
        var classModel = new ClassModel("ReportGeneratedEvent")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("Analytics.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IDomainEvent"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ReportName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ReportType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "GeneratedBy", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "ReportGeneratedEvent", directory, CSharp)
        {
            Namespace = "Analytics.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateMetricThresholdExceededEventFile(string directory)
    {
        var classModel = new ClassModel("MetricThresholdExceededEvent")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("Analytics.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IDomainEvent"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "MetricName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "CurrentValue", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "ThresholdValue", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ThresholdType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));

        return new CodeFileModel<ClassModel>(classModel, "MetricThresholdExceededEvent", directory, CSharp)
        {
            Namespace = "Analytics.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateEventDtoFile(string directory)
    {
        var classModel = new ClassModel("EventDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EventId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EventType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Source", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "SessionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { Nullable = true, GenericTypeParameters = [new TypeModel("string"), new TypeModel("object")] }, "Properties", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Timestamp", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "EventDto", directory, CSharp)
        {
            Namespace = "Analytics.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateMetricDtoFile(string directory)
    {
        var classModel = new ClassModel("MetricDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MetricId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Category", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Unit", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { Nullable = true, GenericTypeParameters = [new TypeModel("string"), new TypeModel("string")] }, "Tags", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Timestamp", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "MetricDto", directory, CSharp)
        {
            Namespace = "Analytics.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateReportDtoFile(string directory)
    {
        var classModel = new ClassModel("ReportDto")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ReportId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ReportType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "StartDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "EndDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "GeneratedBy", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Data", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ReportStatus"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "ReportDto", directory, CSharp)
        {
            Namespace = "Analytics.Core.DTOs"
        };
    }

    private static FileModel CreateTrackEventRequestFile(string directory)
    {
        // Keep as FileModel - contains [Required] attribute which needs raw syntax
        return new FileModel("TrackEventRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Analytics.Core.DTOs;

                /// <summary>
                /// Request model for tracking an analytics event.
                /// </summary>
                public sealed class TrackEventRequest
                {
                    [Required]
                    public required string EventType { get; init; }

                    [Required]
                    public required string Source { get; init; }

                    public string? UserId { get; init; }

                    public string? SessionId { get; init; }

                    public Dictionary<string, object>? Properties { get; init; }

                    public DateTime? Timestamp { get; init; }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateMetricsQueryRequestFile(string directory)
    {
        var classModel = new ClassModel("MetricsQueryRequest")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Category", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "StartDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "EndDate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "AggregationType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "MetricsQueryRequest", directory, CSharp)
        {
            Namespace = "Analytics.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateAnalyticsDbContextFile(string directory)
    {
        var classModel = new ClassModel("AnalyticsDbContext");

        classModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "AnalyticsDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("AnalyticsDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Event")] }, "Events", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Metric")] }, "Metrics", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Report")] }, "Reports", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);")
        });

        return new CodeFileModel<ClassModel>(classModel, "AnalyticsDbContext", directory, CSharp)
        {
            Namespace = "Analytics.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateEventConfigurationFile(string directory)
    {
        var classModel = new ClassModel("EventConfiguration");

        classModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("Event")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("Event")] } }],
            Body = new ExpressionModel(@"builder.HasKey(e => e.EventId);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Source)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.UserId)
            .HasMaxLength(100);

        builder.Property(e => e.SessionId)
            .HasMaxLength(100);

        builder.Property(e => e.Properties)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(e => e.Timestamp)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.UserId);")
        });

        return new CodeFileModel<ClassModel>(classModel, "EventConfiguration", directory, CSharp)
        {
            Namespace = "Analytics.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateMetricConfigurationFile(string directory)
    {
        var classModel = new ClassModel("MetricConfiguration");

        classModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("Metric")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("Metric")] } }],
            Body = new ExpressionModel(@"builder.HasKey(m => m.MetricId);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Value)
            .IsRequired();

        builder.Property(m => m.Unit)
            .HasMaxLength(50);

        builder.Property(m => m.Tags)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(m => m.Timestamp)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.HasIndex(m => m.Name);
        builder.HasIndex(m => m.Category);
        builder.HasIndex(m => m.Timestamp);")
        });

        return new CodeFileModel<ClassModel>(classModel, "MetricConfiguration", directory, CSharp)
        {
            Namespace = "Analytics.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateReportConfigurationFile(string directory)
    {
        var classModel = new ClassModel("ReportConfiguration");

        classModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("Report")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("Report")] } }],
            Body = new ExpressionModel(@"builder.HasKey(r => r.ReportId);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.ReportType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.GeneratedBy)
            .HasMaxLength(100);

        builder.Property(r => r.Data)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.HasIndex(r => r.ReportType);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CreatedAt);")
        });

        return new CodeFileModel<ClassModel>(classModel, "ReportConfiguration", directory, CSharp)
        {
            Namespace = "Analytics.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateEventRepositoryFile(string directory)
    {
        var classModel = new ClassModel("EventRepository");

        classModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Analytics.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Analytics.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("IEventRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("AnalyticsDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "EventRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("AnalyticsDbContext") }],
            Body = new ExpressionModel("this.context = context ?? throw new ArgumentNullException(nameof(context));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Event") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "eventId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByTypeAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Event")] }] },
            Params =
            [
                new ParamModel { Name = "eventType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Events
            .Where(e => e.EventType == eventType)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByDateRangeAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Event")] }] },
            Params =
            [
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Events
            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Event")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Events
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Event")] }] },
            Params =
            [
                new ParamModel { Name = "skip", Type = new TypeModel("int"), DefaultValue = "0" },
                new ParamModel { Name = "take", Type = new TypeModel("int"), DefaultValue = "100" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Events
            .OrderByDescending(e => e.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Event")] },
            Params =
            [
                new ParamModel { Name = "analyticsEvent", Type = new TypeModel("Event") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"analyticsEvent.EventId = Guid.NewGuid();
        analyticsEvent.CreatedAt = DateTime.UtcNow;
        await context.Events.AddAsync(analyticsEvent, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return analyticsEvent;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddBatchAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "events", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Event")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"foreach (var analyticsEvent in events)
        {
            analyticsEvent.EventId = Guid.NewGuid();
            analyticsEvent.CreatedAt = DateTime.UtcNow;
        }

        await context.Events.AddRangeAsync(events, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetCountAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("long")] },
            Params =
            [
                new ParamModel { Name = "eventType", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var query = context.Events.AsQueryable();

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }

        return await query.LongCountAsync(cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "EventRepository", directory, CSharp)
        {
            Namespace = "Analytics.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateMetricsServiceFile(string directory)
    {
        var classModel = new ClassModel("MetricsService");

        classModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Analytics.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Analytics.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));

        classModel.Implements.Add(new TypeModel("IMetricsService"));

        classModel.Fields.Add(new FieldModel { Name = "context", Type = new TypeModel("AnalyticsDbContext"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("MetricsService")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "MetricsService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "context", Type = new TypeModel("AnalyticsDbContext") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("MetricsService")] } }
            ],
            Body = new ExpressionModel(@"this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "RecordMetricAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Metric")] },
            Params =
            [
                new ParamModel { Name = "metric", Type = new TypeModel("Metric") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"metric.MetricId = Guid.NewGuid();
        metric.CreatedAt = DateTime.UtcNow;
        await context.Metrics.AddAsync(metric, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(""Recorded metric {MetricName} with value {Value}"", metric.Name, metric.Value);
        return metric;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetMetricsByNameAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Metric")] }] },
            Params =
            [
                new ParamModel { Name = "name", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Metrics
            .Where(m => m.Name == name)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetMetricsByCategoryAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Metric")] }] },
            Params =
            [
                new ParamModel { Name = "category", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Metrics
            .Where(m => m.Category == category)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetMetricsByDateRangeAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Metric")] }] },
            Params =
            [
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Metrics
            .Where(m => m.Timestamp >= startDate && m.Timestamp <= endDate)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAverageAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("double")] },
            Params =
            [
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Metrics
            .Where(m => m.Name == metricName && m.Timestamp >= startDate && m.Timestamp <= endDate)
            .AverageAsync(m => m.Value, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetSumAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("double")] },
            Params =
            [
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Metrics
            .Where(m => m.Name == metricName && m.Timestamp >= startDate && m.Timestamp <= endDate)
            .SumAsync(m => m.Value, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetMinAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("double")] },
            Params =
            [
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Metrics
            .Where(m => m.Name == metricName && m.Timestamp >= startDate && m.Timestamp <= endDate)
            .MinAsync(m => m.Value, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetMaxAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("double")] },
            Params =
            [
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Metrics
            .Where(m => m.Name == metricName && m.Timestamp >= startDate && m.Timestamp <= endDate)
            .MaxAsync(m => m.Value, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CheckThresholdsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"// Implementation would check configured thresholds and raise events
        logger.LogDebug(""Checking metric thresholds"");
        await Task.CompletedTask;")
        });

        return new CodeFileModel<ClassModel>(classModel, "MetricsService", directory, CSharp)
        {
            Namespace = "Analytics.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateReportGeneratorFile(string directory)
    {
        var classModel = new ClassModel("ReportGenerator");

        classModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Analytics.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Analytics.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));
        classModel.Usings.Add(new UsingModel("System.Text.Json"));

        classModel.Implements.Add(new TypeModel("IReportGenerator"));

        classModel.Fields.Add(new FieldModel { Name = "context", Type = new TypeModel("AnalyticsDbContext"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "eventRepository", Type = new TypeModel("IEventRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "metricsService", Type = new TypeModel("IMetricsService"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("ReportGenerator")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "ReportGenerator")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "context", Type = new TypeModel("AnalyticsDbContext") },
                new ParamModel { Name = "eventRepository", Type = new TypeModel("IEventRepository") },
                new ParamModel { Name = "metricsService", Type = new TypeModel("IMetricsService") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("ReportGenerator")] } }
            ],
            Body = new ExpressionModel(@"this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        this.metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GenerateReportAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Report")] },
            Params =
            [
                new ParamModel { Name = "name", Type = new TypeModel("string") },
                new ParamModel { Name = "reportType", Type = new TypeModel("string") },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "generatedBy", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var report = new Report
        {
            ReportId = Guid.NewGuid(),
            Name = name,
            ReportType = reportType,
            StartDate = startDate,
            EndDate = endDate,
            GeneratedBy = generatedBy,
            Status = ReportStatus.Processing,
            CreatedAt = DateTime.UtcNow
        };

        await context.Reports.AddAsync(report, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(""Starting report generation: {ReportName} ({ReportType})"", name, reportType);

        try
        {
            var reportData = await GenerateReportDataAsync(reportType, startDate, endDate, cancellationToken);
            report.Data = JsonSerializer.Serialize(reportData);
            report.Status = ReportStatus.Completed;
            report.CompletedAt = DateTime.UtcNow;

            logger.LogInformation(""Report generation completed: {ReportId}"", report.ReportId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ""Report generation failed: {ReportId}"", report.ReportId);
            report.Status = ReportStatus.Failed;
        }

        context.Reports.Update(report);
        await context.SaveChangesAsync(cancellationToken);

        return report;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetReportByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Report") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "reportId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Reports
            .FirstOrDefaultAsync(r => r.ReportId == reportId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetReportsByTypeAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Report")] }] },
            Params =
            [
                new ParamModel { Name = "reportType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Reports
            .Where(r => r.ReportType == reportType)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllReportsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Report")] }] },
            Params =
            [
                new ParamModel { Name = "skip", Type = new TypeModel("int"), DefaultValue = "0" },
                new ParamModel { Name = "take", Type = new TypeModel("int"), DefaultValue = "100" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Reports
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateReportStatusAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Report")] },
            Params =
            [
                new ParamModel { Name = "reportId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "status", Type = new TypeModel("ReportStatus") },
                new ParamModel { Name = "data", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var report = await context.Reports.FindAsync(new object[] { reportId }, cancellationToken)
            ?? throw new InvalidOperationException($""Report {reportId} not found"");

        report.Status = status;

        if (data != null)
        {
            report.Data = data;
        }

        if (status == ReportStatus.Completed)
        {
            report.CompletedAt = DateTime.UtcNow;
        }

        context.Reports.Update(report);
        await context.SaveChangesAsync(cancellationToken);

        return report;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GenerateReportDataAsync",
            AccessModifier = AccessModifier.Private,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("object")] },
            Params =
            [
                new ParamModel { Name = "reportType", Type = new TypeModel("string") },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var events = await eventRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        var metrics = await metricsService.GetMetricsByDateRangeAsync(startDate, endDate, cancellationToken);

        return new
        {
            ReportType = reportType,
            Period = new { StartDate = startDate, EndDate = endDate },
            Summary = new
            {
                TotalEvents = events.Count(),
                TotalMetrics = metrics.Count(),
                EventTypes = events.GroupBy(e => e.EventType).Select(g => new { Type = g.Key, Count = g.Count() }),
                MetricCategories = metrics.GroupBy(m => m.Category).Select(g => new { Category = g.Key, Count = g.Count() })
            },
            GeneratedAt = DateTime.UtcNow
        };")
        });

        return new CodeFileModel<ClassModel>(classModel, "ReportGenerator", directory, CSharp)
        {
            Namespace = "Analytics.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Analytics.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Analytics.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Analytics.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Analytics.Infrastructure.Services"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAnalyticsInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(""AnalyticsDb"") ??
                @""Server=.\SQLEXPRESS;Database=AnalyticsDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IMetricsService, MetricsService>();
        services.AddScoped<IReportGenerator, ReportGenerator>();

        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateEventsControllerFile(string directory)
    {
        var classModel = new ClassModel("EventsController");

        classModel.Usings.Add(new UsingModel("Analytics.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Analytics.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Authorization"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/analytics/events\"" });

        classModel.Fields.Add(new FieldModel { Name = "eventRepository", Type = new TypeModel("IEventRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("EventsController")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "EventsController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "eventRepository", Type = new TypeModel("IEventRepository") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("EventsController")] } }
            ],
            Body = new ExpressionModel(@"this.eventRepository = eventRepository;
        this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        var trackEventMethod = new MethodModel
        {
            Name = "TrackEvent",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("EventDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("TrackEventRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var analyticsEvent = new Event
        {
            EventType = request.EventType,
            Source = request.Source,
            UserId = request.UserId,
            SessionId = request.SessionId,
            Properties = request.Properties,
            Timestamp = request.Timestamp ?? DateTime.UtcNow
        };

        var createdEvent = await eventRepository.AddAsync(analyticsEvent, cancellationToken);

        logger.LogInformation(""Event tracked: {EventType} from {Source}"", request.EventType, request.Source);

        var response = new EventDto
        {
            EventId = createdEvent.EventId,
            EventType = createdEvent.EventType,
            Source = createdEvent.Source,
            UserId = createdEvent.UserId,
            SessionId = createdEvent.SessionId,
            Properties = createdEvent.Properties,
            Timestamp = createdEvent.Timestamp
        };

        return CreatedAtAction(nameof(GetById), new { id = createdEvent.EventId }, response);")
        };
        trackEventMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        trackEventMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "AllowAnonymous" });
        trackEventMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(EventDto), StatusCodes.Status201Created" });
        trackEventMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status400BadRequest" });
        classModel.Methods.Add(trackEventMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("EventDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var analyticsEvent = await eventRepository.GetByIdAsync(id, cancellationToken);

        if (analyticsEvent == null)
        {
            return NotFound();
        }

        return Ok(new EventDto
        {
            EventId = analyticsEvent.EventId,
            EventType = analyticsEvent.EventType,
            Source = analyticsEvent.Source,
            UserId = analyticsEvent.UserId,
            SessionId = analyticsEvent.SessionId,
            Properties = analyticsEvent.Properties,
            Timestamp = analyticsEvent.Timestamp
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Authorize" });
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(EventDto), StatusCodes.Status200OK" });
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(getByIdMethod);

        var getByTypeMethod = new MethodModel
        {
            Name = "GetByType",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("EventDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "eventType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var events = await eventRepository.GetByTypeAsync(eventType, cancellationToken);

        var eventDtos = events.Select(e => new EventDto
        {
            EventId = e.EventId,
            EventType = e.EventType,
            Source = e.Source,
            UserId = e.UserId,
            SessionId = e.SessionId,
            Properties = e.Properties,
            Timestamp = e.Timestamp
        });

        return Ok(eventDtos);")
        };
        getByTypeMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"type/{eventType}\"" });
        getByTypeMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Authorize" });
        getByTypeMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(IEnumerable<EventDto>), StatusCodes.Status200OK" });
        classModel.Methods.Add(getByTypeMethod);

        return new CodeFileModel<ClassModel>(classModel, "EventsController", directory, CSharp)
        {
            Namespace = "Analytics.Api.Controllers"
        };
    }

    private static CodeFileModel<ClassModel> CreateMetricsControllerFile(string directory)
    {
        var classModel = new ClassModel("MetricsController");

        classModel.Usings.Add(new UsingModel("Analytics.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Analytics.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Analytics.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Authorization"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/analytics/metrics\"" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Authorize" });

        classModel.Fields.Add(new FieldModel { Name = "metricsService", Type = new TypeModel("IMetricsService"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("MetricsController")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "MetricsController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "metricsService", Type = new TypeModel("IMetricsService") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("MetricsController")] } }
            ],
            Body = new ExpressionModel(@"this.metricsService = metricsService;
        this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        var getMetricsMethod = new MethodModel
        {
            Name = "GetMetrics",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("MetricDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("MetricsQueryRequest"), Attribute = new AttributeModel() { Name = "FromQuery" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"IEnumerable<Metric> metrics;

        if (!string.IsNullOrEmpty(request.Name))
        {
            metrics = await metricsService.GetMetricsByNameAsync(request.Name, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.Category))
        {
            metrics = await metricsService.GetMetricsByCategoryAsync(request.Category, cancellationToken);
        }
        else if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            metrics = await metricsService.GetMetricsByDateRangeAsync(
                request.StartDate.Value,
                request.EndDate.Value,
                cancellationToken);
        }
        else
        {
            metrics = await metricsService.GetMetricsByDateRangeAsync(
                DateTime.UtcNow.AddDays(-7),
                DateTime.UtcNow,
                cancellationToken);
        }

        var metricDtos = metrics.Select(m => new MetricDto
        {
            MetricId = m.MetricId,
            Name = m.Name,
            Category = m.Category,
            Value = m.Value,
            Unit = m.Unit,
            Tags = m.Tags,
            Timestamp = m.Timestamp
        });

        return Ok(metricDtos);")
        };
        getMetricsMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet" });
        getMetricsMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(IEnumerable<MetricDto>), StatusCodes.Status200OK" });
        classModel.Methods.Add(getMetricsMethod);

        var recordMetricMethod = new MethodModel
        {
            Name = "RecordMetric",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("MetricDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("MetricDto"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var metric = new Metric
        {
            Name = request.Name,
            Category = request.Category,
            Value = request.Value,
            Unit = request.Unit,
            Tags = request.Tags,
            Timestamp = request.Timestamp != default ? request.Timestamp : DateTime.UtcNow
        };

        var createdMetric = await metricsService.RecordMetricAsync(metric, cancellationToken);

        logger.LogInformation(""Metric recorded: {MetricName} = {Value}"", metric.Name, metric.Value);

        var response = new MetricDto
        {
            MetricId = createdMetric.MetricId,
            Name = createdMetric.Name,
            Category = createdMetric.Category,
            Value = createdMetric.Value,
            Unit = createdMetric.Unit,
            Tags = createdMetric.Tags,
            Timestamp = createdMetric.Timestamp
        };

        return CreatedAtAction(nameof(GetMetrics), response);")
        };
        recordMetricMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        recordMetricMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(MetricDto), StatusCodes.Status201Created" });
        recordMetricMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status400BadRequest" });
        classModel.Methods.Add(recordMetricMethod);

        var getAggregatedMetricMethod = new MethodModel
        {
            Name = "GetAggregatedMetric",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("object")] }] },
            Params =
            [
                new ParamModel { Name = "name", Type = new TypeModel("string"), Attribute = new AttributeModel() { Name = "FromQuery" } },
                new ParamModel { Name = "aggregation", Type = new TypeModel("string"), Attribute = new AttributeModel() { Name = "FromQuery" } },
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime"), Attribute = new AttributeModel() { Name = "FromQuery" } },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime"), Attribute = new AttributeModel() { Name = "FromQuery" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"double result = aggregation.ToLowerInvariant() switch
        {
            ""average"" or ""avg"" => await metricsService.GetAverageAsync(name, startDate, endDate, cancellationToken),
            ""sum"" => await metricsService.GetSumAsync(name, startDate, endDate, cancellationToken),
            ""min"" => await metricsService.GetMinAsync(name, startDate, endDate, cancellationToken),
            ""max"" => await metricsService.GetMaxAsync(name, startDate, endDate, cancellationToken),
            _ => throw new ArgumentException($""Unknown aggregation type: {aggregation}"")
        };

        return Ok(new
        {
            MetricName = name,
            Aggregation = aggregation,
            Value = result,
            StartDate = startDate,
            EndDate = endDate
        });")
        };
        getAggregatedMetricMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"aggregate\"" });
        getAggregatedMetricMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(object), StatusCodes.Status200OK" });
        getAggregatedMetricMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status400BadRequest" });
        classModel.Methods.Add(getAggregatedMetricMethod);

        return new CodeFileModel<ClassModel>(classModel, "MetricsController", directory, CSharp)
        {
            Namespace = "Analytics.Api.Controllers"
        };
    }

    private static FileModel CreateReportsControllerFile(string directory)
    {
        // Keep as FileModel - contains inner class GenerateReportRequest
        return new FileModel("ReportsController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.DTOs;
                using Analytics.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace Analytics.Api.Controllers;

                /// <summary>
                /// Controller for report operations.
                /// </summary>
                [ApiController]
                [Route("api/analytics/reports")]
                [Authorize]
                public class ReportsController : ControllerBase
                {
                    private readonly IReportGenerator reportGenerator;
                    private readonly ILogger<ReportsController> logger;

                    public ReportsController(
                        IReportGenerator reportGenerator,
                        ILogger<ReportsController> logger)
                    {
                        this.reportGenerator = reportGenerator;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Get a report by ID.
                    /// </summary>
                    [HttpGet("{id:guid}")]
                    [ProducesResponseType(typeof(ReportDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<ReportDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var report = await reportGenerator.GetReportByIdAsync(id, cancellationToken);

                        if (report == null)
                        {
                            return NotFound();
                        }

                        return Ok(new ReportDto
                        {
                            ReportId = report.ReportId,
                            Name = report.Name,
                            ReportType = report.ReportType,
                            Description = report.Description,
                            StartDate = report.StartDate,
                            EndDate = report.EndDate,
                            GeneratedBy = report.GeneratedBy,
                            Data = report.Data,
                            Status = report.Status,
                            CreatedAt = report.CreatedAt,
                            CompletedAt = report.CompletedAt
                        });
                    }

                    /// <summary>
                    /// Get all reports.
                    /// </summary>
                    [HttpGet]
                    [ProducesResponseType(typeof(IEnumerable<ReportDto>), StatusCodes.Status200OK)]
                    public async Task<ActionResult<IEnumerable<ReportDto>>> GetAll(
                        [FromQuery] int skip = 0,
                        [FromQuery] int take = 100,
                        CancellationToken cancellationToken = default)
                    {
                        var reports = await reportGenerator.GetAllReportsAsync(skip, take, cancellationToken);

                        var reportDtos = reports.Select(r => new ReportDto
                        {
                            ReportId = r.ReportId,
                            Name = r.Name,
                            ReportType = r.ReportType,
                            Description = r.Description,
                            StartDate = r.StartDate,
                            EndDate = r.EndDate,
                            GeneratedBy = r.GeneratedBy,
                            Data = r.Data,
                            Status = r.Status,
                            CreatedAt = r.CreatedAt,
                            CompletedAt = r.CompletedAt
                        });

                        return Ok(reportDtos);
                    }

                    /// <summary>
                    /// Generate a new report.
                    /// </summary>
                    [HttpPost]
                    [ProducesResponseType(typeof(ReportDto), StatusCodes.Status202Accepted)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<ReportDto>> GenerateReport(
                        [FromBody] GenerateReportRequest request,
                        CancellationToken cancellationToken)
                    {
                        var report = await reportGenerator.GenerateReportAsync(
                            request.Name,
                            request.ReportType,
                            request.StartDate,
                            request.EndDate,
                            request.GeneratedBy,
                            cancellationToken);

                        logger.LogInformation("Report generation initiated: {ReportId}", report.ReportId);

                        var response = new ReportDto
                        {
                            ReportId = report.ReportId,
                            Name = report.Name,
                            ReportType = report.ReportType,
                            Description = report.Description,
                            StartDate = report.StartDate,
                            EndDate = report.EndDate,
                            GeneratedBy = report.GeneratedBy,
                            Data = report.Data,
                            Status = report.Status,
                            CreatedAt = report.CreatedAt,
                            CompletedAt = report.CompletedAt
                        };

                        return AcceptedAtAction(nameof(GetById), new { id = report.ReportId }, response);
                    }
                }

                /// <summary>
                /// Request model for generating a report.
                /// </summary>
                public sealed class GenerateReportRequest
                {
                    public required string Name { get; init; }

                    public required string ReportType { get; init; }

                    public DateTime StartDate { get; init; }

                    public DateTime EndDate { get; init; }

                    public string? GeneratedBy { get; init; }
                }
                """
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        // Keep as FileModel - top-level statements
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Text;
                using Microsoft.AspNetCore.Authentication.JwtBearer;
                using Microsoft.IdentityModel.Tokens;
                using Microsoft.OpenApi.Models;
                using Serilog;

                var builder = WebApplication.CreateBuilder(args);

                // Configure Serilog
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();

                builder.Host.UseSerilog();

                // Add services
                builder.Services.AddAnalyticsInfrastructure(builder.Configuration);

                // Configure JWT authentication
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = builder.Configuration["Jwt:Issuer"],
                            ValidAudience = builder.Configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured")))
                        };
                    });

                builder.Services.AddAuthorization();
                builder.Services.AddControllers();

                // Configure Swagger
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Analytics API",
                        Version = "v1",
                        Description = "Analytics microservice for event tracking, metrics, and reporting"
                    });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                // Configure CORS
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        policy.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
                });

                // Add health checks
                builder.Services.AddHealthChecks();

                var app = builder.Build();

                // Configure pipeline
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseCors();
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();
                app.MapHealthChecks("/health");

                app.Run();
                """
        };
    }

    private static FileModel CreateAppSettingsFile(string directory)
    {
        // Keep as FileModel - JSON file
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "AnalyticsDb": "Server=.\\SQLEXPRESS;Database=AnalyticsDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Jwt": {
                    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
                    "Issuer": "Analytics.Api",
                    "Audience": "Analytics.Api",
                    "ExpirationHours": "24"
                  },
                  "Serilog": {
                    "MinimumLevel": {
                      "Default": "Information",
                      "Override": {
                        "Microsoft": "Warning",
                        "Microsoft.Hosting.Lifetime": "Information"
                      }
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        };
    }

    private static FileModel CreateAppSettingsDevelopmentFile(string directory)
    {
        // Keep as FileModel - JSON file
        return new FileModel("appsettings.Development", directory, ".json")
        {
            Body = """
                {
                  "Serilog": {
                    "MinimumLevel": {
                      "Default": "Debug"
                    }
                  }
                }
                """
        };
    }

    #endregion
}
