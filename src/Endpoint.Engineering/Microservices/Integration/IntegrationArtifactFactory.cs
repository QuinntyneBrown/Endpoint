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

namespace Endpoint.Engineering.Microservices.Integration;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class IntegrationArtifactFactory : IIntegrationArtifactFactory
{
    private readonly ILogger<IntegrationArtifactFactory> logger;

    public IntegrationArtifactFactory(ILogger<IntegrationArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Integration.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateIntegrationEntityFile(entitiesDir));
        project.Files.Add(CreateWebhookFile(entitiesDir));
        project.Files.Add(CreateSyncJobFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIIntegrationRepositoryFile(interfacesDir));
        project.Files.Add(CreateIWebhookHandlerFile(interfacesDir));
        project.Files.Add(CreateISyncServiceFile(interfacesDir));

        // Events
        project.Files.Add(CreateIntegrationConnectedEventFile(eventsDir));
        project.Files.Add(CreateWebhookReceivedEventFile(eventsDir));
        project.Files.Add(CreateSyncCompletedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateIntegrationDtoFile(dtosDir));
        project.Files.Add(CreateCreateIntegrationRequestFile(dtosDir));
        project.Files.Add(CreateWebhookDtoFile(dtosDir));
        project.Files.Add(CreateWebhookPayloadDtoFile(dtosDir));
        project.Files.Add(CreateWebhookResponseDtoFile(dtosDir));
        project.Files.Add(CreateRegisterWebhookRequestFile(dtosDir));
        project.Files.Add(CreateSyncJobDtoFile(dtosDir));
        project.Files.Add(CreateStartSyncRequestFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Integration.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateIntegrationDbContextFile(dataDir));
        project.Files.Add(CreateIntegrationRepositoryFile(repositoriesDir));
        project.Files.Add(CreateWebhookHandlerFile(servicesDir));
        project.Files.Add(CreateSyncServiceFile(servicesDir));
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Integration.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateIntegrationsControllerFile(controllersDir));
        project.Files.Add(CreateWebhooksControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files - Entities

    private static FileModel CreateIntegrationEntityFile(string directory)
    {
        // Keep as FileModel due to embedded enum
        return new FileModel("Integration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.Entities;

                public class Integration
                {
                    public Guid IntegrationId { get; set; }
                    public Guid TenantId { get; set; }
                    public required string Name { get; set; }
                    public required string Provider { get; set; }
                    public IntegrationStatus Status { get; set; } = IntegrationStatus.Pending;
                    public string? Configuration { get; set; }
                    public string? Credentials { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? ConnectedAt { get; set; }
                    public DateTime? LastSyncAt { get; set; }
                    public ICollection<Webhook> Webhooks { get; set; } = new List<Webhook>();
                    public ICollection<SyncJob> SyncJobs { get; set; } = new List<SyncJob>();
                }

                public enum IntegrationStatus { Pending, Connected, Disconnected, Error }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateWebhookFile(string directory)
    {
        var classModel = new ClassModel("Webhook");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "WebhookId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "IntegrationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Integration"), "Integration", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Endpoint", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Secret", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "EventTypes", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "LastTriggeredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "Webhook", directory, CSharp)
        {
            Namespace = "Integration.Core.Entities"
        };
    }

    private static FileModel CreateSyncJobFile(string directory)
    {
        // Keep as FileModel due to embedded enum
        return new FileModel("SyncJob", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.Entities;

                public class SyncJob
                {
                    public Guid SyncJobId { get; set; }
                    public Guid IntegrationId { get; set; }
                    public Integration Integration { get; set; } = null!;
                    public required string JobType { get; set; }
                    public SyncJobStatus Status { get; set; } = SyncJobStatus.Pending;
                    public string? Parameters { get; set; }
                    public string? Result { get; set; }
                    public int RecordsProcessed { get; set; }
                    public int RecordsFailed { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? StartedAt { get; set; }
                    public DateTime? CompletedAt { get; set; }
                }

                public enum SyncJobStatus { Pending, Running, Completed, Failed, Cancelled }
                """
        };
    }

    #endregion

    #region Core Layer Files - Interfaces

    private static CodeFileModel<InterfaceModel> CreateIIntegrationRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IIntegrationRepository");

        interfaceModel.Usings.Add(new UsingModel("Integration.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Integration") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByTenantIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Entities.Integration")] }] },
            Params =
            [
                new ParamModel { Name = "tenantId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Entities.Integration")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Integration")] },
            Params =
            [
                new ParamModel { Name = "integration", Type = new TypeModel("Entities.Integration") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "integration", Type = new TypeModel("Entities.Integration") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IIntegrationRepository", directory, CSharp)
        {
            Namespace = "Integration.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIWebhookHandlerFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IWebhookHandler");

        interfaceModel.Usings.Add(new UsingModel("Integration.Core.DTOs"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "HandleAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WebhookResponseDto")] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "payload", Type = new TypeModel("WebhookPayloadDto") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ValidateSignatureAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "signature", Type = new TypeModel("string") },
                new ParamModel { Name = "payload", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RegisterAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WebhookDto")] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "request", Type = new TypeModel("RegisterWebhookRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IWebhookHandler", directory, CSharp)
        {
            Namespace = "Integration.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateISyncServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ISyncService");

        interfaceModel.Usings.Add(new UsingModel("Integration.Core.DTOs"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "StartSyncAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SyncJobDto")] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "request", Type = new TypeModel("StartSyncRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetSyncJobAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SyncJobDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "syncJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetSyncJobsByIntegrationAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("SyncJobDto")] }] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "CancelSyncAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "syncJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ISyncService", directory, CSharp)
        {
            Namespace = "Integration.Core.Interfaces"
        };
    }

    #endregion

    #region Core Layer Files - Events

    private static CodeFileModel<ClassModel> CreateIntegrationConnectedEventFile(string directory)
    {
        var classModel = new ClassModel("IntegrationConnectedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "IntegrationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Provider", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "ConnectedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "IntegrationConnectedEvent", directory, CSharp)
        {
            Namespace = "Integration.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateWebhookReceivedEventFile(string directory)
    {
        var classModel = new ClassModel("WebhookReceivedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "WebhookId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "IntegrationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EventType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Payload", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "ReceivedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "WebhookReceivedEvent", directory, CSharp)
        {
            Namespace = "Integration.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateSyncCompletedEventFile(string directory)
    {
        var classModel = new ClassModel("SyncCompletedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SyncJobId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "IntegrationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "JobType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RecordsProcessed", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RecordsFailed", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "SyncCompletedEvent", directory, CSharp)
        {
            Namespace = "Integration.Core.Events"
        };
    }

    #endregion

    #region Core Layer Files - DTOs

    private static CodeFileModel<ClassModel> CreateIntegrationDtoFile(string directory)
    {
        var classModel = new ClassModel("IntegrationDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "IntegrationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Provider", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "\"Pending\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "ConnectedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "LastSyncAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));

        return new CodeFileModel<ClassModel>(classModel, "IntegrationDto", directory, CSharp)
        {
            Namespace = "Integration.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateCreateIntegrationRequestFile(string directory)
    {
        var classModel = new ClassModel("CreateIntegrationRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var tenantIdProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]);
        tenantIdProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(tenantIdProp);

        var nameProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true);
        nameProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(nameProp);

        var providerProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Provider", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true);
        providerProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(providerProp);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Configuration", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));

        return new CodeFileModel<ClassModel>(classModel, "CreateIntegrationRequest", directory, CSharp)
        {
            Namespace = "Integration.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateWebhookDtoFile(string directory)
    {
        var classModel = new ClassModel("WebhookDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "WebhookId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "IntegrationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Endpoint", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "EventTypes", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "LastTriggeredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));

        return new CodeFileModel<ClassModel>(classModel, "WebhookDto", directory, CSharp)
        {
            Namespace = "Integration.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateWebhookPayloadDtoFile(string directory)
    {
        var classModel = new ClassModel("WebhookPayloadDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EventType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Payload", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Signature", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "Timestamp", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "WebhookPayloadDto", directory, CSharp)
        {
            Namespace = "Integration.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateWebhookResponseDtoFile(string directory)
    {
        var classModel = new ClassModel("WebhookResponseDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "Success", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Message", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "ProcessedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "WebhookResponseDto", directory, CSharp)
        {
            Namespace = "Integration.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateRegisterWebhookRequestFile(string directory)
    {
        var classModel = new ClassModel("RegisterWebhookRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var endpointProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Endpoint", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true);
        endpointProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(endpointProp);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "EventTypes", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));

        return new CodeFileModel<ClassModel>(classModel, "RegisterWebhookRequest", directory, CSharp)
        {
            Namespace = "Integration.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateSyncJobDtoFile(string directory)
    {
        var classModel = new ClassModel("SyncJobDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SyncJobId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "IntegrationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "JobType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]) { DefaultValue = "\"Pending\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RecordsProcessed", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "RecordsFailed", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "StartedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "CompletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));

        return new CodeFileModel<ClassModel>(classModel, "SyncJobDto", directory, CSharp)
        {
            Namespace = "Integration.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateStartSyncRequestFile(string directory)
    {
        var classModel = new ClassModel("StartSyncRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var jobTypeProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "JobType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)], required: true);
        jobTypeProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(jobTypeProp);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Parameters", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Init, null)]));

        return new CodeFileModel<ClassModel>(classModel, "StartSyncRequest", directory, CSharp)
        {
            Namespace = "Integration.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateIntegrationDbContextFile(string directory)
    {
        var classModel = new ClassModel("IntegrationDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Integration.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "IntegrationDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("IntegrationDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Core.Entities.Integration")] }, "Integrations", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<Core.Entities.Integration>()")]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Webhook")] }, "Webhooks", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<Webhook>()")]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("SyncJob")] }, "SyncJobs", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<SyncJob>()")]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<Core.Entities.Integration>(entity =>
        {
            entity.HasKey(i => i.IntegrationId);
            entity.Property(i => i.Name).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Provider).IsRequired().HasMaxLength(100);
            entity.HasMany(i => i.Webhooks).WithOne(w => w.Integration).HasForeignKey(w => w.IntegrationId);
            entity.HasMany(i => i.SyncJobs).WithOne(s => s.Integration).HasForeignKey(s => s.IntegrationId);
        });

        modelBuilder.Entity<Webhook>(entity =>
        {
            entity.HasKey(w => w.WebhookId);
            entity.Property(w => w.Endpoint).IsRequired().HasMaxLength(500);
            entity.Property(w => w.Secret).IsRequired().HasMaxLength(256);
        });

        modelBuilder.Entity<SyncJob>(entity =>
        {
            entity.HasKey(s => s.SyncJobId);
            entity.Property(s => s.JobType).IsRequired().HasMaxLength(100);
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "IntegrationDbContext", directory, CSharp)
        {
            Namespace = "Integration.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateIntegrationRepositoryFile(string directory)
    {
        var classModel = new ClassModel("IntegrationRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Integration.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Integration.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("IIntegrationRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("IntegrationDbContext"),
            AccessModifier = AccessModifier.Private,
            Readonly = true
        });

        var constructor = new ConstructorModel(classModel, "IntegrationRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("IntegrationDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Integration") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Integrations.Include(i => i.Webhooks).Include(i => i.SyncJobs).FirstOrDefaultAsync(i => i.IntegrationId == integrationId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByTenantIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Core.Entities.Integration")] }] },
            Params =
            [
                new ParamModel { Name = "tenantId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Integrations.Include(i => i.Webhooks).Where(i => i.TenantId == tenantId).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Core.Entities.Integration")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Integrations.Include(i => i.Webhooks).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Integration")] },
            Params =
            [
                new ParamModel { Name = "integration", Type = new TypeModel("Core.Entities.Integration") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"integration.IntegrationId = Guid.NewGuid();
        await context.Integrations.AddAsync(integration, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return integration;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "integration", Type = new TypeModel("Core.Entities.Integration") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.Integrations.Update(integration);
        await context.SaveChangesAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var integration = await context.Integrations.FindAsync(new object[] { integrationId }, cancellationToken);
        if (integration != null)
        {
            context.Integrations.Remove(integration);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "IntegrationRepository", directory, CSharp)
        {
            Namespace = "Integration.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateWebhookHandlerFile(string directory)
    {
        var classModel = new ClassModel("WebhookHandler");

        classModel.Usings.Add(new UsingModel("System.Security.Cryptography"));
        classModel.Usings.Add(new UsingModel("System.Text"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Integration.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Integration.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Integration.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Integration.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("IWebhookHandler"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("IntegrationDbContext"),
            AccessModifier = AccessModifier.Private,
            Readonly = true
        });

        var constructor = new ConstructorModel(classModel, "WebhookHandler")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("IntegrationDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "HandleAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WebhookResponseDto")] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "payload", Type = new TypeModel("WebhookPayloadDto") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var integration = await context.Integrations.Include(i => i.Webhooks).FirstOrDefaultAsync(i => i.IntegrationId == integrationId, cancellationToken);
        if (integration == null)
        {
            return new WebhookResponseDto { Success = false, Message = ""Integration not found"" };
        }

        var webhook = integration.Webhooks.FirstOrDefault(w => w.IsActive);
        if (webhook != null)
        {
            webhook.LastTriggeredAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        return new WebhookResponseDto { Success = true, Message = ""Webhook processed successfully"" };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ValidateSignatureAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "signature", Type = new TypeModel("string") },
                new ParamModel { Name = "payload", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var webhook = await context.Webhooks.FirstOrDefaultAsync(w => w.IntegrationId == integrationId && w.IsActive, cancellationToken);
        if (webhook == null) return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhook.Secret));
        var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
        return computedHash == signature;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "RegisterAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("WebhookDto")] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "request", Type = new TypeModel("RegisterWebhookRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var webhook = new Webhook
        {
            WebhookId = Guid.NewGuid(),
            IntegrationId = integrationId,
            Endpoint = request.Endpoint,
            Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            EventTypes = request.EventTypes
        };

        await context.Webhooks.AddAsync(webhook, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new WebhookDto
        {
            WebhookId = webhook.WebhookId,
            IntegrationId = webhook.IntegrationId,
            Endpoint = webhook.Endpoint,
            EventTypes = webhook.EventTypes,
            IsActive = webhook.IsActive,
            CreatedAt = webhook.CreatedAt
        };")
        });

        return new CodeFileModel<ClassModel>(classModel, "WebhookHandler", directory, CSharp)
        {
            Namespace = "Integration.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateSyncServiceFile(string directory)
    {
        var classModel = new ClassModel("SyncService");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Integration.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Integration.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Integration.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Integration.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ISyncService"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("IntegrationDbContext"),
            AccessModifier = AccessModifier.Private,
            Readonly = true
        });

        var constructor = new ConstructorModel(classModel, "SyncService")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("IntegrationDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "StartSyncAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SyncJobDto")] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "request", Type = new TypeModel("StartSyncRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var syncJob = new SyncJob
        {
            SyncJobId = Guid.NewGuid(),
            IntegrationId = integrationId,
            JobType = request.JobType,
            Parameters = request.Parameters,
            Status = SyncJobStatus.Running,
            StartedAt = DateTime.UtcNow
        };

        await context.SyncJobs.AddAsync(syncJob, cancellationToken);

        var integration = await context.Integrations.FindAsync(new object[] { integrationId }, cancellationToken);
        if (integration != null)
        {
            integration.LastSyncAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SyncJobDto
        {
            SyncJobId = syncJob.SyncJobId,
            IntegrationId = syncJob.IntegrationId,
            JobType = syncJob.JobType,
            Status = syncJob.Status.ToString(),
            CreatedAt = syncJob.CreatedAt,
            StartedAt = syncJob.StartedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetSyncJobAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("SyncJobDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "syncJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var syncJob = await context.SyncJobs.FindAsync(new object[] { syncJobId }, cancellationToken);
        if (syncJob == null) return null;

        return new SyncJobDto
        {
            SyncJobId = syncJob.SyncJobId,
            IntegrationId = syncJob.IntegrationId,
            JobType = syncJob.JobType,
            Status = syncJob.Status.ToString(),
            RecordsProcessed = syncJob.RecordsProcessed,
            RecordsFailed = syncJob.RecordsFailed,
            CreatedAt = syncJob.CreatedAt,
            StartedAt = syncJob.StartedAt,
            CompletedAt = syncJob.CompletedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetSyncJobsByIntegrationAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("SyncJobDto")] }] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var syncJobs = await context.SyncJobs.Where(s => s.IntegrationId == integrationId).OrderByDescending(s => s.CreatedAt).ToListAsync(cancellationToken);
        return syncJobs.Select(s => new SyncJobDto
        {
            SyncJobId = s.SyncJobId,
            IntegrationId = s.IntegrationId,
            JobType = s.JobType,
            Status = s.Status.ToString(),
            RecordsProcessed = s.RecordsProcessed,
            RecordsFailed = s.RecordsFailed,
            CreatedAt = s.CreatedAt,
            StartedAt = s.StartedAt,
            CompletedAt = s.CompletedAt
        });")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CancelSyncAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "syncJobId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var syncJob = await context.SyncJobs.FindAsync(new object[] { syncJobId }, cancellationToken);
        if (syncJob != null && syncJob.Status == SyncJobStatus.Running)
        {
            syncJob.Status = SyncJobStatus.Cancelled;
            syncJob.CompletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "SyncService", directory, CSharp)
        {
            Namespace = "Integration.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Integration.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Integration.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Integration.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Integration.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddIntegrationInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<IntegrationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""IntegrationDb"") ??
                @""Server=.\SQLEXPRESS;Database=IntegrationDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<IIntegrationRepository, IntegrationRepository>();
        services.AddScoped<IWebhookHandler, WebhookHandler>();
        services.AddScoped<ISyncService, SyncService>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateIntegrationsControllerFile(string directory)
    {
        var classModel = new ClassModel("IntegrationsController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Integration.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Integration.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Integration.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel
        {
            Name = "repository",
            Type = new TypeModel("IIntegrationRepository"),
            AccessModifier = AccessModifier.Private,
            Readonly = true
        });

        var constructor = new ConstructorModel(classModel, "IntegrationsController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "repository", Type = new TypeModel("IIntegrationRepository") }],
            Body = new ExpressionModel("this.repository = repository;")
        };
        classModel.Constructors.Add(constructor);

        var createMethod = new MethodModel
        {
            Name = "Create",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IntegrationDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateIntegrationRequest"), Attribute = "[FromBody]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var integration = new Core.Entities.Integration
        {
            TenantId = request.TenantId,
            Name = request.Name,
            Provider = request.Provider,
            Configuration = request.Configuration
        };

        var created = await repository.AddAsync(integration, cancellationToken);

        var dto = new IntegrationDto
        {
            IntegrationId = created.IntegrationId,
            TenantId = created.TenantId,
            Name = created.Name,
            Provider = created.Provider,
            Status = created.Status.ToString(),
            CreatedAt = created.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = dto.IntegrationId }, dto);")
        };
        createMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        classModel.Methods.Add(createMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IntegrationDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var integration = await repository.GetByIdAsync(id, cancellationToken);
        if (integration == null) return NotFound();

        return Ok(new IntegrationDto
        {
            IntegrationId = integration.IntegrationId,
            TenantId = integration.TenantId,
            Name = integration.Name,
            Provider = integration.Provider,
            Status = integration.Status.ToString(),
            CreatedAt = integration.CreatedAt,
            ConnectedAt = integration.ConnectedAt,
            LastSyncAt = integration.LastSyncAt
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        classModel.Methods.Add(getByIdMethod);

        return new CodeFileModel<ClassModel>(classModel, "IntegrationsController", directory, CSharp)
        {
            Namespace = "Integration.Api.Controllers"
        };
    }

    private static CodeFileModel<ClassModel> CreateWebhooksControllerFile(string directory)
    {
        var classModel = new ClassModel("WebhooksController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Integration.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Integration.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel
        {
            Name = "webhookHandler",
            Type = new TypeModel("IWebhookHandler"),
            AccessModifier = AccessModifier.Private,
            Readonly = true
        });

        var constructor = new ConstructorModel(classModel, "WebhooksController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "webhookHandler", Type = new TypeModel("IWebhookHandler") }],
            Body = new ExpressionModel("this.webhookHandler = webhookHandler;")
        };
        classModel.Constructors.Add(constructor);

        var handleMethod = new MethodModel
        {
            Name = "Handle",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("WebhookResponseDto")] }] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "payload", Type = new TypeModel("WebhookPayloadDto"), Attribute = "[FromBody]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var response = await webhookHandler.HandleAsync(integrationId, payload, cancellationToken);
        if (!response.Success) return BadRequest(response);
        return Ok(response);")
        };
        handleMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"{integrationId:guid}\"" });
        classModel.Methods.Add(handleMethod);

        var registerMethod = new MethodModel
        {
            Name = "Register",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("WebhookDto")] }] },
            Params =
            [
                new ParamModel { Name = "integrationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "request", Type = new TypeModel("RegisterWebhookRequest"), Attribute = "[FromBody]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var webhook = await webhookHandler.RegisterAsync(integrationId, request, cancellationToken);
        return Created($""/api/webhooks/{integrationId}"", webhook);")
        };
        registerMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"{integrationId:guid}/register\"" });
        classModel.Methods.Add(registerMethod);

        return new CodeFileModel<ClassModel>(classModel, "WebhooksController", directory, CSharp)
        {
            Namespace = "Integration.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        // Keep as FileModel for top-level statements
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddIntegrationInfrastructure(builder.Configuration);
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddHealthChecks();

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.MapControllers();
                app.MapHealthChecks("/health");
                app.Run();
                """
        };
    }

    private static FileModel CreateAppSettingsFile(string directory)
    {
        // Keep as FileModel for JSON
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "IntegrationDb": "Server=.\\SQLEXPRESS;Database=IntegrationDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information"
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        };
    }

    #endregion
}
