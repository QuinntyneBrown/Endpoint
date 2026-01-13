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

namespace Endpoint.Engineering.Microservices.Audit;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

/// <summary>
/// Factory for creating Audit microservice artifacts according to audit-microservice.spec.md.
/// </summary>
public class AuditArtifactFactory : IAuditArtifactFactory
{
    private readonly ILogger<AuditArtifactFactory> logger;

    public AuditArtifactFactory(ILogger<AuditArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Audit.Core files");

        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");

        // Entities
        project.Files.Add(CreateIAggregateRootFile(entitiesDir));
        project.Files.Add(CreateAuditEntryEntityFile(entitiesDir));
        project.Files.Add(CreateChangeLogEntityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIDomainEventFile(interfacesDir));
        project.Files.Add(CreateIAuditRepositoryFile(interfacesDir));
        project.Files.Add(CreateIAuditServiceFile(interfacesDir));

        // Events
        project.Files.Add(CreateAuditEntryCreatedEventFile(eventsDir));

        // DTOs
        var dtosDir = Path.Combine(project.Directory, "DTOs");
        project.Files.Add(CreateAuditEntryDtoFile(dtosDir));
        project.Files.Add(CreateChangeLogDtoFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Audit.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // DbContext
        project.Files.Add(CreateAuditDbContextFile(dataDir));

        // Entity Configurations
        project.Files.Add(CreateAuditEntryConfigurationFile(configurationsDir));
        project.Files.Add(CreateChangeLogConfigurationFile(configurationsDir));

        // Repositories
        project.Files.Add(CreateAuditRepositoryFile(repositoriesDir));

        // Services
        project.Files.Add(CreateAuditServiceFile(servicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Audit.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Controllers
        project.Files.Add(CreateAuditControllerFile(controllersDir));

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
            Namespace = "Audit.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateAuditEntryEntityFile(string directory)
    {
        var classModel = new ClassModel("AuditEntry");

        classModel.Implements.Add(new TypeModel("IAggregateRoot"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AuditEntryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Action", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "UserName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "IpAddress", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "OldValues", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "NewValues", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "AffectedColumns", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Timestamp", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("ChangeLog")] }, "ChangeLogs", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<ChangeLog>()" });

        return new CodeFileModel<ClassModel>(classModel, "AuditEntry", directory, CSharp)
        {
            Namespace = "Audit.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateChangeLogEntityFile(string directory)
    {
        var classModel = new ClassModel("ChangeLog");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ChangeLogId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AuditEntryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("AuditEntry"), "AuditEntry", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "PropertyName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "OldValue", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "NewValue", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "PropertyType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));

        return new CodeFileModel<ClassModel>(classModel, "ChangeLog", directory, CSharp)
        {
            Namespace = "Audit.Core.Entities"
        };
    }

    private static FileModel CreateIDomainEventFile(string directory)
    {
        return new FileModel("IDomainEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Audit.Core.Interfaces;

                /// <summary>
                /// Interface for domain events.
                /// </summary>
                public interface IDomainEvent
                {
                    Guid AggregateId { get; }

                    string AggregateType { get; }

                    DateTime OccurredAt { get; }

                    string CorrelationId { get; }
                }
                """
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIAuditRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IAuditRepository");

        interfaceModel.Usings.Add(new UsingModel("Audit.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "auditEntryId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByEntityIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByEntityTypeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
            Params =
            [
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByDateRangeAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
            Params =
            [
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
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
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry")] },
            Params =
            [
                new ParamModel { Name = "auditEntry", Type = new TypeModel("AuditEntry") },
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
                new ParamModel { Name = "auditEntries", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] } },
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
                new ParamModel { Name = "entityType", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IAuditRepository", directory, CSharp)
        {
            Namespace = "Audit.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIAuditServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IAuditService");

        interfaceModel.Usings.Add(new UsingModel("Audit.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "LogAuditAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry")] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "action", Type = new TypeModel("string") },
                new ParamModel { Name = "userId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "userName", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "ipAddress", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "oldValues", Type = new TypeModel("object") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "newValues", Type = new TypeModel("object") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "affectedColumns", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")], Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "correlationId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "LogCreateAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry")] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "newValues", Type = new TypeModel("object") },
                new ParamModel { Name = "userId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "userName", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "ipAddress", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "correlationId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "LogUpdateAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry")] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "oldValues", Type = new TypeModel("object") },
                new ParamModel { Name = "newValues", Type = new TypeModel("object") },
                new ParamModel { Name = "affectedColumns", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")], Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "userId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "userName", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "ipAddress", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "correlationId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "LogDeleteAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry")] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "oldValues", Type = new TypeModel("object") },
                new ParamModel { Name = "userId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "userName", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "ipAddress", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "correlationId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetEntityHistoryAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IAuditService", directory, CSharp)
        {
            Namespace = "Audit.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateAuditEntryCreatedEventFile(string directory)
    {
        var classModel = new ClassModel("AuditEntryCreatedEvent")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("Audit.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("IDomainEvent"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Action", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "AuditEntryCreatedEvent", directory, CSharp)
        {
            Namespace = "Audit.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateAuditEntryDtoFile(string directory)
    {
        var classModel = new ClassModel("AuditEntryDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "AuditEntryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Action", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "UserName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "IpAddress", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "OldValues", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "NewValues", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "AffectedColumns", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Timestamp", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("IReadOnlyList") { GenericTypeParameters = [new TypeModel("ChangeLogDto")] }, "ChangeLogs", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "Array.Empty<ChangeLogDto>()" });

        return new CodeFileModel<ClassModel>(classModel, "AuditEntryDto", directory, CSharp)
        {
            Namespace = "Audit.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateChangeLogDtoFile(string directory)
    {
        var classModel = new ClassModel("ChangeLogDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ChangeLogId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "PropertyName", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "OldValue", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "NewValue", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "PropertyType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));

        return new CodeFileModel<ClassModel>(classModel, "ChangeLogDto", directory, CSharp)
        {
            Namespace = "Audit.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateAuditDbContextFile(string directory)
    {
        var classModel = new ClassModel("AuditDbContext");

        classModel.Usings.Add(new UsingModel("Audit.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "AuditDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("AuditDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("AuditEntry")] }, "AuditEntries", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("ChangeLog")] }, "ChangeLogs", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);")
        });

        return new CodeFileModel<ClassModel>(classModel, "AuditDbContext", directory, CSharp)
        {
            Namespace = "Audit.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateAuditEntryConfigurationFile(string directory)
    {
        var classModel = new ClassModel("AuditEntryConfiguration");

        classModel.Usings.Add(new UsingModel("Audit.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("AuditEntry")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("AuditEntry")] } }],
            Body = new ExpressionModel(@"builder.HasKey(a => a.AuditEntryId);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.UserId)
            .HasMaxLength(100);

        builder.Property(a => a.UserName)
            .HasMaxLength(200);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.OldValues)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(a => a.NewValues)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(a => a.AffectedColumns)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.CorrelationId)
            .HasMaxLength(100);

        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.CorrelationId);")
        });

        return new CodeFileModel<ClassModel>(classModel, "AuditEntryConfiguration", directory, CSharp)
        {
            Namespace = "Audit.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateChangeLogConfigurationFile(string directory)
    {
        var classModel = new ClassModel("ChangeLogConfiguration");

        classModel.Usings.Add(new UsingModel("Audit.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("ChangeLog")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("ChangeLog")] } }],
            Body = new ExpressionModel(@"builder.HasKey(c => c.ChangeLogId);

        builder.Property(c => c.PropertyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.OldValue)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(c => c.NewValue)
            .HasColumnType(""nvarchar(max)"");

        builder.Property(c => c.PropertyType)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(c => c.AuditEntry)
            .WithMany(a => a.ChangeLogs)
            .HasForeignKey(c => c.AuditEntryId)
            .OnDelete(DeleteBehavior.Cascade);")
        });

        return new CodeFileModel<ClassModel>(classModel, "ChangeLogConfiguration", directory, CSharp)
        {
            Namespace = "Audit.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateAuditRepositoryFile(string directory)
    {
        var classModel = new ClassModel("AuditRepository");

        classModel.Usings.Add(new UsingModel("Audit.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Audit.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Audit.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("IAuditRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("AuditDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "AuditRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("AuditDbContext") }],
            Body = new ExpressionModel("this.context = context ?? throw new ArgumentNullException(nameof(context));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "auditEntryId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.AuditEntries
            .Include(a => a.ChangeLogs)
            .FirstOrDefaultAsync(a => a.AuditEntryId == auditEntryId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByEntityIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.AuditEntries
            .Include(a => a.ChangeLogs)
            .Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByEntityTypeAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
            Params =
            [
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.AuditEntries
            .Include(a => a.ChangeLogs)
            .Where(a => a.EntityType == entityType)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.AuditEntries
            .Include(a => a.ChangeLogs)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByDateRangeAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
            Params =
            [
                new ParamModel { Name = "startDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "endDate", Type = new TypeModel("DateTime") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.AuditEntries
            .Include(a => a.ChangeLogs)
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
            Params =
            [
                new ParamModel { Name = "skip", Type = new TypeModel("int"), DefaultValue = "0" },
                new ParamModel { Name = "take", Type = new TypeModel("int"), DefaultValue = "100" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.AuditEntries
            .Include(a => a.ChangeLogs)
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry")] },
            Params =
            [
                new ParamModel { Name = "auditEntry", Type = new TypeModel("AuditEntry") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"auditEntry.AuditEntryId = Guid.NewGuid();
        auditEntry.Timestamp = DateTime.UtcNow;
        await context.AuditEntries.AddAsync(auditEntry, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return auditEntry;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddBatchAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "auditEntries", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"foreach (var auditEntry in auditEntries)
        {
            auditEntry.AuditEntryId = Guid.NewGuid();
            auditEntry.Timestamp = DateTime.UtcNow;
        }

        await context.AuditEntries.AddRangeAsync(auditEntries, cancellationToken);
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
                new ParamModel { Name = "entityType", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var query = context.AuditEntries.AsQueryable();

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        return await query.LongCountAsync(cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "AuditRepository", directory, CSharp)
        {
            Namespace = "Audit.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateAuditServiceFile(string directory)
    {
        var classModel = new ClassModel("AuditService");

        classModel.Usings.Add(new UsingModel("System.Text.Json"));
        classModel.Usings.Add(new UsingModel("Audit.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Audit.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));

        classModel.Implements.Add(new TypeModel("IAuditService"));

        classModel.Fields.Add(new FieldModel { Name = "auditRepository", Type = new TypeModel("IAuditRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("AuditService")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "AuditService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "auditRepository", Type = new TypeModel("IAuditRepository") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("AuditService")] } }
            ],
            Body = new ExpressionModel(@"this.auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "LogAuditAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry")] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "action", Type = new TypeModel("string") },
                new ParamModel { Name = "userId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "userName", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "ipAddress", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "oldValues", Type = new TypeModel("object") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "newValues", Type = new TypeModel("object") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "affectedColumns", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")], Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "correlationId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var auditEntry = new AuditEntry
        {
            EntityId = entityId,
            EntityType = entityType,
            Action = action,
            UserId = userId,
            UserName = userName,
            IpAddress = ipAddress,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            AffectedColumns = affectedColumns != null ? string.Join("","", affectedColumns) : null,
            CorrelationId = correlationId ?? Guid.NewGuid().ToString()
        };

        var createdEntry = await auditRepository.AddAsync(auditEntry, cancellationToken);

        logger.LogInformation(
            ""Audit entry created: {Action} on {EntityType} ({EntityId}) by {UserId}"",
            action,
            entityType,
            entityId,
            userId);

        return createdEntry;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "LogCreateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry")] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "newValues", Type = new TypeModel("object") },
                new ParamModel { Name = "userId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "userName", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "ipAddress", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "correlationId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await LogAuditAsync(
            entityId,
            entityType,
            ""Create"",
            userId,
            userName,
            ipAddress,
            null,
            newValues,
            null,
            correlationId,
            cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "LogUpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry")] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "oldValues", Type = new TypeModel("object") },
                new ParamModel { Name = "newValues", Type = new TypeModel("object") },
                new ParamModel { Name = "affectedColumns", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")], Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "userId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "userName", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "ipAddress", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "correlationId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await LogAuditAsync(
            entityId,
            entityType,
            ""Update"",
            userId,
            userName,
            ipAddress,
            oldValues,
            newValues,
            affectedColumns,
            correlationId,
            cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "LogDeleteAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("AuditEntry")] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "entityType", Type = new TypeModel("string") },
                new ParamModel { Name = "oldValues", Type = new TypeModel("object") },
                new ParamModel { Name = "userId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "userName", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "ipAddress", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "correlationId", Type = new TypeModel("string") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await LogAuditAsync(
            entityId,
            entityType,
            ""Delete"",
            userId,
            userName,
            ipAddress,
            oldValues,
            null,
            null,
            correlationId,
            cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetEntityHistoryAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntry")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await auditRepository.GetByEntityIdAsync(entityId, cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "AuditService", directory, CSharp)
        {
            Namespace = "Audit.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Audit.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Audit.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Audit.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Audit.Infrastructure.Services"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.DependencyInjection"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAuditInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<AuditDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(""AuditDb"") ??
                @""Server=.\SQLEXPRESS;Database=AuditDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IAuditService, AuditService>();

        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateAuditControllerFile(string directory)
    {
        var classModel = new ClassModel("AuditController");

        classModel.Usings.Add(new UsingModel("Audit.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Audit.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Authorization"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/audit\"" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Authorize" });

        classModel.Fields.Add(new FieldModel { Name = "auditRepository", Type = new TypeModel("IAuditRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("AuditController")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "AuditController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "auditRepository", Type = new TypeModel("IAuditRepository") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("AuditController")] } }
            ],
            Body = new ExpressionModel(@"this.auditRepository = auditRepository;
        this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        var getEntriesMethod = new MethodModel
        {
            Name = "GetEntries",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntryDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "skip", Type = new TypeModel("int"), Attribute = new AttributeModel() { Name = "FromQuery" }, DefaultValue = "0" },
                new ParamModel { Name = "take", Type = new TypeModel("int"), Attribute = new AttributeModel() { Name = "FromQuery" }, DefaultValue = "100" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var entries = await auditRepository.GetAllAsync(skip, take, cancellationToken);

        var entryDtos = entries.Select(e => new AuditEntryDto
        {
            AuditEntryId = e.AuditEntryId,
            EntityId = e.EntityId,
            EntityType = e.EntityType,
            Action = e.Action,
            UserId = e.UserId,
            UserName = e.UserName,
            IpAddress = e.IpAddress,
            OldValues = e.OldValues,
            NewValues = e.NewValues,
            AffectedColumns = e.AffectedColumns,
            Timestamp = e.Timestamp,
            CorrelationId = e.CorrelationId,
            ChangeLogs = e.ChangeLogs.Select(c => new ChangeLogDto
            {
                ChangeLogId = c.ChangeLogId,
                PropertyName = c.PropertyName,
                OldValue = c.OldValue,
                NewValue = c.NewValue,
                PropertyType = c.PropertyType
            }).ToList()
        });

        return Ok(entryDtos);")
        };
        getEntriesMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"entries\"" });
        getEntriesMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(IEnumerable<AuditEntryDto>), StatusCodes.Status200OK" });
        classModel.Methods.Add(getEntriesMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("AuditEntryDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var entry = await auditRepository.GetByIdAsync(id, cancellationToken);

        if (entry == null)
        {
            return NotFound();
        }

        return Ok(new AuditEntryDto
        {
            AuditEntryId = entry.AuditEntryId,
            EntityId = entry.EntityId,
            EntityType = entry.EntityType,
            Action = entry.Action,
            UserId = entry.UserId,
            UserName = entry.UserName,
            IpAddress = entry.IpAddress,
            OldValues = entry.OldValues,
            NewValues = entry.NewValues,
            AffectedColumns = entry.AffectedColumns,
            Timestamp = entry.Timestamp,
            CorrelationId = entry.CorrelationId,
            ChangeLogs = entry.ChangeLogs.Select(c => new ChangeLogDto
            {
                ChangeLogId = c.ChangeLogId,
                PropertyName = c.PropertyName,
                OldValue = c.OldValue,
                NewValue = c.NewValue,
                PropertyType = c.PropertyType
            }).ToList()
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"entries/{id:guid}\"" });
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(AuditEntryDto), StatusCodes.Status200OK" });
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(getByIdMethod);

        var getByEntityIdMethod = new MethodModel
        {
            Name = "GetByEntityId",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("AuditEntryDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var entries = await auditRepository.GetByEntityIdAsync(entityId, cancellationToken);

        var entryDtos = entries.Select(e => new AuditEntryDto
        {
            AuditEntryId = e.AuditEntryId,
            EntityId = e.EntityId,
            EntityType = e.EntityType,
            Action = e.Action,
            UserId = e.UserId,
            UserName = e.UserName,
            IpAddress = e.IpAddress,
            OldValues = e.OldValues,
            NewValues = e.NewValues,
            AffectedColumns = e.AffectedColumns,
            Timestamp = e.Timestamp,
            CorrelationId = e.CorrelationId,
            ChangeLogs = e.ChangeLogs.Select(c => new ChangeLogDto
            {
                ChangeLogId = c.ChangeLogId,
                PropertyName = c.PropertyName,
                OldValue = c.OldValue,
                NewValue = c.NewValue,
                PropertyType = c.PropertyType
            }).ToList()
        });

        return Ok(entryDtos);")
        };
        getByEntityIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"entity/{entityId}\"" });
        getByEntityIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(IEnumerable<AuditEntryDto>), StatusCodes.Status200OK" });
        classModel.Methods.Add(getByEntityIdMethod);

        return new CodeFileModel<ClassModel>(classModel, "AuditController", directory, CSharp)
        {
            Namespace = "Audit.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
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
                builder.Services.AddAuditInfrastructure(builder.Configuration);

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
                        Title = "Audit API",
                        Version = "v1",
                        Description = "Audit microservice for tracking entity changes and maintaining audit logs"
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
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "AuditDb": "Server=.\\SQLEXPRESS;Database=AuditDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Jwt": {
                    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
                    "Issuer": "Audit.Api",
                    "Audience": "Audit.Api",
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
