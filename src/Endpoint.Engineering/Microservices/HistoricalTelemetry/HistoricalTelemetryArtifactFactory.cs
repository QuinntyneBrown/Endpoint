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
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.HistoricalTelemetry;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

/// <summary>
/// Factory for creating Historical Telemetry microservice artifacts.
/// Manages long-term storage and retrieval of telemetry data.
/// </summary>
public class HistoricalTelemetryArtifactFactory : IHistoricalTelemetryArtifactFactory
{
    private readonly ILogger<HistoricalTelemetryArtifactFactory> logger;

    public HistoricalTelemetryArtifactFactory(ILogger<HistoricalTelemetryArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding HistoricalTelemetry.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");
        var optionsDir = Path.Combine(project.Directory, "Options");

        // Options per REQ-HIST-006
        project.Files.Add(CreateHistoricalTelemetryOptionsFile(optionsDir));

        // Entities
        project.Files.Add(CreateHistoricalTelemetryRecordFile(entitiesDir));
        project.Files.Add(CreateTelemetryAggregationFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIHistoricalTelemetryRepositoryFile(interfacesDir));
        project.Files.Add(CreateITelemetryPersistenceServiceFile(interfacesDir));

        // Events
        project.Files.Add(CreateTelemetryPersistedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateHistoricalTelemetryDtoFile(dtosDir));
        project.Files.Add(CreateTelemetryQueryRequestFile(dtosDir));
        project.Files.Add(CreatePagedTelemetryResponseFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding HistoricalTelemetry.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");
        var backgroundServicesDir = Path.Combine(project.Directory, "BackgroundServices");

        // DbContext
        project.Files.Add(CreateHistoricalTelemetryDbContextFile(dataDir));

        // Entity Configurations with indexes per REQ-HIST-001
        project.Files.Add(CreateHistoricalTelemetryRecordConfigurationFile(configurationsDir));
        project.Files.Add(CreateTelemetryAggregationConfigurationFile(configurationsDir));

        // Repositories with bulk insert per REQ-HIST-002
        project.Files.Add(CreateHistoricalTelemetryRepositoryFile(repositoriesDir));

        // Services
        project.Files.Add(CreateTelemetryPersistenceServiceFile(servicesDir));

        // Background Service - Redis Pub/Sub listener scaffold per REQ-HIST-004
        project.Files.Add(CreateTelemetryListenerServiceFile(backgroundServicesDir));

        // ConfigureServices using Options pattern per REQ-HIST-006
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding HistoricalTelemetry.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // HistoricalTelemetry controller with paginated queries per REQ-HIST-003
        project.Files.Add(CreateHistoricalTelemetryControllerFile(controllersDir));

        // appsettings.json
        project.Files.Add(CreateAppSettingsFile(project.Directory));

        // Program.cs
        project.Files.Add(CreateProgramFile(project.Directory));
    }

    #region Core Layer Files

    private static CodeFileModel<ClassModel> CreateHistoricalTelemetryOptionsFile(string directory)
    {
        var classModel = new ClassModel("HistoricalTelemetryOptions");

        classModel.Fields.Add(new FieldModel
        {
            Name = "SectionName",
            Type = new TypeModel("string"),
            AccessModifier = AccessModifier.Public,
            Static = true,
            DefaultValue = "\"HistoricalTelemetry\""
        });

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "RedisConnectionString", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "\"localhost:6379\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "TelemetryChannel", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "\"telemetry\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "BatchSize", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "100" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "BatchIntervalMs", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "1000" });

        return new CodeFileModel<ClassModel>(classModel, "HistoricalTelemetryOptions", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Core.Options"
        };
    }

    private static CodeFileModel<ClassModel> CreateHistoricalTelemetryRecordFile(string directory)
    {
        var classModel = new ClassModel("HistoricalTelemetryRecord");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "RecordId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Source", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "MetricName", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Unit", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("TelemetryType"), "Type", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "TelemetryType.Metric" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Timestamp", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "StoredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Tags", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "HistoricalTelemetryRecord", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateTelemetryAggregationFile(string directory)
    {
        var classModel = new ClassModel("TelemetryAggregation");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AggregationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Source", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "MetricName", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("AggregationType"), "AggregationType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "MinValue", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "MaxValue", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "AverageValue", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("decimal"), "SumValue", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Count", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "PeriodStart", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "PeriodEnd", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "TelemetryAggregation", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Core.Entities"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIHistoricalTelemetryRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IHistoricalTelemetryRepository");

        interfaceModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] },
            Params =
            [
                new ParamModel { Name = "record", Type = new TypeModel("HistoricalTelemetryRecord") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddRangeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "records", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetBySourceAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] }] },
            Params =
            [
                new ParamModel { Name = "source", Type = new TypeModel("string") },
                new ParamModel { Name = "startTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "page", Type = new TypeModel("int") },
                new ParamModel { Name = "pageSize", Type = new TypeModel("int") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByMetricAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] }] },
            Params =
            [
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "page", Type = new TypeModel("int") },
                new ParamModel { Name = "pageSize", Type = new TypeModel("int") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetBySourceAndMetricAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] }] },
            Params =
            [
                new ParamModel { Name = "source", Type = new TypeModel("string") },
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "page", Type = new TypeModel("int") },
                new ParamModel { Name = "pageSize", Type = new TypeModel("int") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetCountAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("int")] },
            Params =
            [
                new ParamModel { Name = "source", Type = new TypeModel("string") { Nullable = true } },
                new ParamModel { Name = "metricName", Type = new TypeModel("string") { Nullable = true } },
                new ParamModel { Name = "startTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IHistoricalTelemetryRepository", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateITelemetryPersistenceServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ITelemetryPersistenceService");

        interfaceModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "PersistTelemetryAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "record", Type = new TypeModel("HistoricalTelemetryRecord") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "PersistBatchAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "records", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ITelemetryPersistenceService", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Core.Interfaces"
        };
    }

    private static FileModel CreateTelemetryPersistedEventFile(string directory)
    {
        return new FileModel("TelemetryPersistedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.HistoricalTelemetry.Core.Events;

                public record TelemetryPersistedEvent(Guid RecordId, string Source, string MetricName, DateTime Timestamp, DateTime StoredAt);
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateHistoricalTelemetryDtoFile(string directory)
    {
        var classModel = new ClassModel("HistoricalTelemetryDto");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "RecordId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Source", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "MetricName", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Unit", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Timestamp", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "StoredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "HistoricalTelemetryDto", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateTelemetryQueryRequestFile(string directory)
    {
        var classModel = new ClassModel("TelemetryQueryRequest");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Source", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "MetricName", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "StartTime", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "EndTime", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Page", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "1" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "PageSize", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "50" });

        return new CodeFileModel<ClassModel>(classModel, "TelemetryQueryRequest", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreatePagedTelemetryResponseFile(string directory)
    {
        var classModel = new ClassModel("PagedTelemetryResponse");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryDto")] }, "Data", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "new List<HistoricalTelemetryDto>()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "Page", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "PageSize", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "TotalCount", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "TotalPages", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "PagedTelemetryResponse", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateHistoricalTelemetryDbContextFile(string directory)
    {
        var classModel = new ClassModel("HistoricalTelemetryDbContext");

        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "HistoricalTelemetryDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] }, "TelemetryRecords", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<HistoricalTelemetryRecord>()")]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("TelemetryAggregation")] }, "TelemetryAggregations", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<TelemetryAggregation>()")]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HistoricalTelemetryDbContext).Assembly);")
        });

        return new CodeFileModel<ClassModel>(classModel, "HistoricalTelemetryDbContext", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateHistoricalTelemetryRecordConfigurationFile(string directory)
    {
        var classModel = new ClassModel("HistoricalTelemetryRecordConfiguration");

        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] } }],
            Body = new ExpressionModel(@"builder.HasKey(x => x.RecordId);
        builder.Property(x => x.Source).IsRequired().HasMaxLength(200);
        builder.Property(x => x.MetricName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Value).IsRequired();

        // Index-optimized queries per REQ-HIST-001
        builder.HasIndex(x => new { x.Source, x.Timestamp });
        builder.HasIndex(x => new { x.MetricName, x.Timestamp });
        builder.HasIndex(x => x.Timestamp);")
        });

        return new CodeFileModel<ClassModel>(classModel, "HistoricalTelemetryRecordConfiguration", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateTelemetryAggregationConfigurationFile(string directory)
    {
        var classModel = new ClassModel("TelemetryAggregationConfiguration");

        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("TelemetryAggregation")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("TelemetryAggregation")] } }],
            Body = new ExpressionModel(@"builder.HasKey(x => x.AggregationId);
        builder.Property(x => x.Source).IsRequired().HasMaxLength(200);
        builder.Property(x => x.MetricName).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => new { x.Source, x.MetricName, x.PeriodStart, x.AggregationType });")
        });

        return new CodeFileModel<ClassModel>(classModel, "TelemetryAggregationConfiguration", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateHistoricalTelemetryRepositoryFile(string directory)
    {
        var classModel = new ClassModel("HistoricalTelemetryRepository");

        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Entities"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("IHistoricalTelemetryRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("HistoricalTelemetryDbContext"),
            AccessModifier = AccessModifier.Private,
            Readonly = true
        });

        var constructor = new ConstructorModel(classModel, "HistoricalTelemetryRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("HistoricalTelemetryDbContext") }],
            Body = new ExpressionModel("this.context = context ?? throw new ArgumentNullException(nameof(context));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] },
            Params =
            [
                new ParamModel { Name = "record", Type = new TypeModel("HistoricalTelemetryRecord") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"await context.TelemetryRecords.AddAsync(record, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return record;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddRangeAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "records", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"await context.TelemetryRecords.AddRangeAsync(records, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetBySourceAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] }] },
            Params =
            [
                new ParamModel { Name = "source", Type = new TypeModel("string") },
                new ParamModel { Name = "startTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "page", Type = new TypeModel("int") },
                new ParamModel { Name = "pageSize", Type = new TypeModel("int") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.TelemetryRecords
            .Where(x => x.Source == source && x.Timestamp >= startTime && x.Timestamp <= endTime)
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByMetricAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] }] },
            Params =
            [
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "page", Type = new TypeModel("int") },
                new ParamModel { Name = "pageSize", Type = new TypeModel("int") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.TelemetryRecords
            .Where(x => x.MetricName == metricName && x.Timestamp >= startTime && x.Timestamp <= endTime)
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetBySourceAndMetricAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] }] },
            Params =
            [
                new ParamModel { Name = "source", Type = new TypeModel("string") },
                new ParamModel { Name = "metricName", Type = new TypeModel("string") },
                new ParamModel { Name = "startTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "page", Type = new TypeModel("int") },
                new ParamModel { Name = "pageSize", Type = new TypeModel("int") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.TelemetryRecords
            .Where(x => x.Source == source && x.MetricName == metricName && x.Timestamp >= startTime && x.Timestamp <= endTime)
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetCountAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("int")] },
            Params =
            [
                new ParamModel { Name = "source", Type = new TypeModel("string") { Nullable = true } },
                new ParamModel { Name = "metricName", Type = new TypeModel("string") { Nullable = true } },
                new ParamModel { Name = "startTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endTime", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var query = context.TelemetryRecords.Where(x => x.Timestamp >= startTime && x.Timestamp <= endTime);

        if (!string.IsNullOrEmpty(source))
        {
            query = query.Where(x => x.Source == source);
        }

        if (!string.IsNullOrEmpty(metricName))
        {
            query = query.Where(x => x.MetricName == metricName);
        }

        return await query.CountAsync(cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "HistoricalTelemetryRepository", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateTelemetryPersistenceServiceFile(string directory)
    {
        var classModel = new ClassModel("TelemetryPersistenceService");

        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Entities"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));

        classModel.Implements.Add(new TypeModel("ITelemetryPersistenceService"));

        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("IHistoricalTelemetryRepository"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("TelemetryPersistenceService")] }, AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "TelemetryPersistenceService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "repository", Type = new TypeModel("IHistoricalTelemetryRepository") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("TelemetryPersistenceService")] } }
            ],
            Body = new ExpressionModel(@"this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "PersistTelemetryAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "record", Type = new TypeModel("HistoricalTelemetryRecord") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"try
        {
            await repository.AddAsync(record, cancellationToken);
            logger.LogDebug(""Persisted telemetry record: {Source}/{MetricName}"", record.Source, record.MetricName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ""Error persisting telemetry record: {Source}/{MetricName}"", record.Source, record.MetricName);
            throw;
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "PersistBatchAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "records", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryRecord")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"try
        {
            await repository.AddRangeAsync(records, cancellationToken);
            logger.LogDebug(""Persisted batch of {Count} telemetry records"", records.Count());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ""Error persisting batch of telemetry records"");
            throw;
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "TelemetryPersistenceService", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateTelemetryListenerServiceFile(string directory)
    {
        var classModel = new ClassModel("TelemetryListenerService");

        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Entities"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Options"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Hosting"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Options"));
        classModel.Usings.Add(new UsingModel("System.Text.Json"));

        classModel.Implements.Add(new TypeModel("BackgroundService"));

        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("TelemetryListenerService")] }, AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "persistenceService", Type = new TypeModel("ITelemetryPersistenceService"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "options", Type = new TypeModel("HistoricalTelemetryOptions"), AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "TelemetryListenerService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("TelemetryListenerService")] } },
                new ParamModel { Name = "persistenceService", Type = new TypeModel("ITelemetryPersistenceService") },
                new ParamModel { Name = "options", Type = new TypeModel("IOptions") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryOptions")] } }
            ],
            Body = new ExpressionModel(@"this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.persistenceService = persistenceService ?? throw new ArgumentNullException(nameof(persistenceService));
        this.options = options?.Value ?? new HistoricalTelemetryOptions();")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "ExecuteAsync",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params = [new ParamModel { Name = "stoppingToken", Type = new TypeModel("CancellationToken") }],
            Body = new ExpressionModel(@"logger.LogInformation(""Telemetry Listener Service is starting. Channel: {Channel}, Redis: {Redis}"",
            options.TelemetryChannel, options.RedisConnectionString);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(options.BatchIntervalMs, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ""Error occurred while listening for telemetry messages"");
                await Task.Delay(1000, stoppingToken);
            }
        }

        logger.LogInformation(""Telemetry Listener Service is stopping"");")
        });

        return new CodeFileModel<ClassModel>(classModel, "TelemetryListenerService", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Infrastructure.BackgroundServices"
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Options"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Infrastructure.BackgroundServices"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Infrastructure.Services"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.DependencyInjection"));

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
            Body = new ExpressionModel(@"// Options pattern per REQ-HIST-006
        services.Configure<HistoricalTelemetryOptions>(
            configuration.GetSection(HistoricalTelemetryOptions.SectionName));

        // Database connection using Options pattern
        var connectionString = configuration.GetConnectionString(""DefaultConnection"");
        services.AddDbContext<HistoricalTelemetryDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IHistoricalTelemetryRepository, HistoricalTelemetryRepository>();
        services.AddScoped<ITelemetryPersistenceService, TelemetryPersistenceService>();
        services.AddHostedService<TelemetryListenerService>();

        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Infrastructure"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateHistoricalTelemetryControllerFile(string directory)
    {
        var classModel = new ClassModel("HistoricalTelemetryController");

        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("EventMonitoring.HistoricalTelemetry.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("IHistoricalTelemetryRepository"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryController")] }, AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "HistoricalTelemetryController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "repository", Type = new TypeModel("IHistoricalTelemetryRepository") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("HistoricalTelemetryController")] } }
            ],
            Body = new ExpressionModel(@"this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));")
        };
        classModel.Constructors.Add(constructor);

        var queryMethod = new MethodModel
        {
            Name = "Query",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("PagedTelemetryResponse")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("TelemetryQueryRequest"), Attribute = "[FromQuery]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"logger.LogInformation(""Querying historical telemetry: Source={Source}, Metric={MetricName}, Page={Page}"",
            request.Source, request.MetricName, request.Page);

        IEnumerable<Core.Entities.HistoricalTelemetryRecord> records;

        if (!string.IsNullOrEmpty(request.Source) && !string.IsNullOrEmpty(request.MetricName))
        {
            records = await repository.GetBySourceAndMetricAsync(
                request.Source,
                request.MetricName,
                request.StartTime,
                request.EndTime,
                request.Page,
                request.PageSize,
                cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.Source))
        {
            records = await repository.GetBySourceAsync(
                request.Source,
                request.StartTime,
                request.EndTime,
                request.Page,
                request.PageSize,
                cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.MetricName))
        {
            records = await repository.GetByMetricAsync(
                request.MetricName,
                request.StartTime,
                request.EndTime,
                request.Page,
                request.PageSize,
                cancellationToken);
        }
        else
        {
            return BadRequest(""Either Source or MetricName must be specified"");
        }

        var totalCount = await repository.GetCountAsync(
            request.Source,
            request.MetricName,
            request.StartTime,
            request.EndTime,
            cancellationToken);

        var dtos = records.Select(r => new HistoricalTelemetryDto
        {
            RecordId = r.RecordId,
            Source = r.Source,
            MetricName = r.MetricName,
            Value = r.Value,
            Unit = r.Unit,
            Timestamp = r.Timestamp,
            StoredAt = r.StoredAt
        });

        var response = new PagedTelemetryResponse
        {
            Data = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return Ok(response);")
        };
        queryMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet" });
        classModel.Methods.Add(queryMethod);

        return new CodeFileModel<ClassModel>(classModel, "HistoricalTelemetryController", directory, CSharp)
        {
            Namespace = "EventMonitoring.HistoricalTelemetry.Api.Controllers"
        };
    }

    private static FileModel CreateAppSettingsFile(string directory)
    {
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=HistoricalTelemetry;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
                  },
                  "HistoricalTelemetry": {
                    "RedisConnectionString": "localhost:6379",
                    "TelemetryChannel": "telemetry",
                    "BatchSize": 100,
                    "BatchIntervalMs": 1000
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information",
                      "Microsoft.AspNetCore": "Warning",
                      "Microsoft.EntityFrameworkCore": "Warning"
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

                using EventMonitoring.HistoricalTelemetry.Infrastructure;
                using EventMonitoring.HistoricalTelemetry.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddInfrastructureServices(builder.Configuration);

                var app = builder.Build();

                // Apply migrations
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<HistoricalTelemetryDbContext>();
                    dbContext.Database.Migrate();
                }

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
                """
        };
    }

    #endregion
}
