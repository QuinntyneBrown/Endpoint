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

namespace Endpoint.Engineering.Microservices.Notification;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class NotificationArtifactFactory : INotificationArtifactFactory
{
    private readonly ILogger<NotificationArtifactFactory> logger;

    public NotificationArtifactFactory(ILogger<NotificationArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Notification.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateNotificationFile(entitiesDir));
        project.Files.Add(CreateNotificationEnumsFile(entitiesDir));
        project.Files.Add(CreateNotificationTemplateFile(entitiesDir));
        project.Files.Add(CreateNotificationPreferenceFile(entitiesDir));
        project.Files.Add(CreateNotificationHistoryFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateINotificationRepositoryFile(interfacesDir));
        project.Files.Add(CreateINotificationServiceFile(interfacesDir));

        // Events
        project.Files.Add(CreateNotificationSentEventFile(eventsDir));
        project.Files.Add(CreateNotificationDeliveredEventFile(eventsDir));
        project.Files.Add(CreateNotificationFailedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateNotificationDtoFile(dtosDir));
        project.Files.Add(CreateSendNotificationRequestFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Notification.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateNotificationDbContextFile(dataDir));
        project.Files.Add(CreateNotificationRepositoryFile(repositoriesDir));
        project.Files.Add(CreateNotificationServiceFile(servicesDir));
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Notification.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateNotificationsControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files

    private static CodeFileModel<ClassModel> CreateNotificationFile(string directory)
    {
        var classModel = new ClassModel("Notification");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "NotificationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "TemplateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Title", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Body", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("NotificationType"), "Type", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "NotificationType.Info" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("NotificationStatus"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "NotificationStatus.Pending" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "SentAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "DeliveredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("NotificationTemplate") { Nullable = true }, "Template", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "Notification", directory, CSharp)
        {
            Namespace = "Notification.Core.Entities"
        };
    }

    private static FileModel CreateNotificationEnumsFile(string directory)
    {
        return new FileModel("NotificationEnums", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Notification.Core.Entities;

                public enum NotificationType { Info, Warning, Error, Success }
                public enum NotificationStatus { Pending, Sent, Delivered, Failed }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateNotificationTemplateFile(string directory)
    {
        var classModel = new ClassModel("NotificationTemplate");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TemplateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Subject", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "BodyTemplate", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("NotificationType"), "Type", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "NotificationType.Info" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "NotificationTemplate", directory, CSharp)
        {
            Namespace = "Notification.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateNotificationPreferenceFile(string directory)
    {
        var classModel = new ClassModel("NotificationPreference");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "PreferenceId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "EmailEnabled", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "PushEnabled", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "SmsEnabled", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "false" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "InAppEnabled", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "true" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));

        return new CodeFileModel<ClassModel>(classModel, "NotificationPreference", directory, CSharp)
        {
            Namespace = "Notification.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateNotificationHistoryFile(string directory)
    {
        var classModel = new ClassModel("NotificationHistory");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "HistoryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "NotificationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Notification"), "Notification", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Action", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Details", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "NotificationHistory", directory, CSharp)
        {
            Namespace = "Notification.Core.Entities"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateINotificationRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("INotificationRepository");

        interfaceModel.Usings.Add(new UsingModel("Notification.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Notification") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "notificationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Entities.Notification")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Entities.Notification")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Entities.Notification")] },
            Params =
            [
                new ParamModel { Name = "notification", Type = new TypeModel("Entities.Notification") },
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
                new ParamModel { Name = "notification", Type = new TypeModel("Entities.Notification") },
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
                new ParamModel { Name = "notificationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "INotificationRepository", directory, CSharp)
        {
            Namespace = "Notification.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateINotificationServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("INotificationService");

        interfaceModel.Usings.Add(new UsingModel("Notification.Core.DTOs"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "SendAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("NotificationDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("SendNotificationRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("NotificationDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "notificationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("NotificationDto")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "MarkAsDeliveredAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "notificationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "INotificationService", directory, CSharp)
        {
            Namespace = "Notification.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateNotificationSentEventFile(string directory)
    {
        var classModel = new ClassModel("NotificationSentEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "NotificationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Title", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "NotificationSentEvent", directory, CSharp)
        {
            Namespace = "Notification.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateNotificationDeliveredEventFile(string directory)
    {
        var classModel = new ClassModel("NotificationDeliveredEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "NotificationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "DeliveredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "NotificationDeliveredEvent", directory, CSharp)
        {
            Namespace = "Notification.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateNotificationFailedEventFile(string directory)
    {
        var classModel = new ClassModel("NotificationFailedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "NotificationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Reason", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "NotificationFailedEvent", directory, CSharp)
        {
            Namespace = "Notification.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateNotificationDtoFile(string directory)
    {
        var classModel = new ClassModel("NotificationDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "NotificationId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Title", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Body", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Type", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "\"Info\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Status", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "\"Pending\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "SentAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "DeliveredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "NotificationDto", directory, CSharp)
        {
            Namespace = "Notification.Core.DTOs"
        };
    }

    private static CodeFileModel<ClassModel> CreateSendNotificationRequestFile(string directory)
    {
        var classModel = new ClassModel("SendNotificationRequest")
        {
            Sealed = true
        };

        classModel.Usings.Add(new UsingModel("System.ComponentModel.DataAnnotations"));

        var userIdProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]);
        userIdProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(userIdProp);

        var titleProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Title", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        titleProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(titleProp);

        var bodyProp = new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Body", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true);
        bodyProp.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Required" });
        classModel.Properties.Add(bodyProp);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "TemplateId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "SendNotificationRequest", directory, CSharp)
        {
            Namespace = "Notification.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateNotificationDbContextFile(string directory)
    {
        var classModel = new ClassModel("NotificationDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Notification.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "NotificationDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("NotificationDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Core.Entities.Notification")] }, "Notifications", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("NotificationTemplate")] }, "NotificationTemplates", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("NotificationPreference")] }, "NotificationPreferences", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("NotificationHistory")] }, "NotificationHistories", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<Core.Entities.Notification>(entity =>
        {
            entity.HasKey(n => n.NotificationId);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Body).IsRequired();
            entity.HasOne(n => n.Template).WithMany().HasForeignKey(n => n.TemplateId);
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(t => t.TemplateId);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.Name).IsUnique();
        });

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.HasKey(p => p.PreferenceId);
            entity.HasIndex(p => p.UserId).IsUnique();
        });

        modelBuilder.Entity<NotificationHistory>(entity =>
        {
            entity.HasKey(h => h.HistoryId);
            entity.HasOne(h => h.Notification).WithMany().HasForeignKey(h => h.NotificationId);
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "NotificationDbContext", directory, CSharp)
        {
            Namespace = "Notification.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateNotificationRepositoryFile(string directory)
    {
        var classModel = new ClassModel("NotificationRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Notification.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Notification.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("INotificationRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("NotificationDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "NotificationRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("NotificationDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Notification") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "notificationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Notifications.Include(n => n.Template).FirstOrDefaultAsync(n => n.NotificationId == notificationId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Core.Entities.Notification")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Notifications.Include(n => n.Template).Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Core.Entities.Notification")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Notifications.Include(n => n.Template).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Core.Entities.Notification")] },
            Params =
            [
                new ParamModel { Name = "notification", Type = new TypeModel("Core.Entities.Notification") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"notification.NotificationId = Guid.NewGuid();
        await context.Notifications.AddAsync(notification, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return notification;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "notification", Type = new TypeModel("Core.Entities.Notification") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.Notifications.Update(notification);
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
                new ParamModel { Name = "notificationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var notification = await context.Notifications.FindAsync(new object[] { notificationId }, cancellationToken);
        if (notification != null)
        {
            context.Notifications.Remove(notification);
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "NotificationRepository", directory, CSharp)
        {
            Namespace = "Notification.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateNotificationServiceFile(string directory)
    {
        var classModel = new ClassModel("NotificationService");

        classModel.Usings.Add(new UsingModel("Notification.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Notification.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Notification.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("INotificationService"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "repository",
            Type = new TypeModel("INotificationRepository"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "NotificationService")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "repository", Type = new TypeModel("INotificationRepository") }],
            Body = new ExpressionModel("this.repository = repository;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "SendAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("NotificationDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("SendNotificationRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var notification = new Core.Entities.Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Body = request.Body,
            TemplateId = request.TemplateId,
            Status = NotificationStatus.Sent,
            SentAt = DateTime.UtcNow
        };

        var created = await repository.AddAsync(notification, cancellationToken);

        return new NotificationDto
        {
            NotificationId = created.NotificationId,
            UserId = created.UserId,
            Title = created.Title,
            Body = created.Body,
            Type = created.Type.ToString(),
            Status = created.Status.ToString(),
            CreatedAt = created.CreatedAt,
            SentAt = created.SentAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("NotificationDto") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "notificationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var notification = await repository.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null) return null;

        return new NotificationDto
        {
            NotificationId = notification.NotificationId,
            UserId = notification.UserId,
            Title = notification.Title,
            Body = notification.Body,
            Type = notification.Type.ToString(),
            Status = notification.Status.ToString(),
            CreatedAt = notification.CreatedAt,
            SentAt = notification.SentAt,
            DeliveredAt = notification.DeliveredAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("NotificationDto")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var notifications = await repository.GetByUserIdAsync(userId, cancellationToken);
        return notifications.Select(n => new NotificationDto
        {
            NotificationId = n.NotificationId,
            UserId = n.UserId,
            Title = n.Title,
            Body = n.Body,
            Type = n.Type.ToString(),
            Status = n.Status.ToString(),
            CreatedAt = n.CreatedAt,
            SentAt = n.SentAt,
            DeliveredAt = n.DeliveredAt
        });")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "MarkAsDeliveredAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "notificationId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var notification = await repository.GetByIdAsync(notificationId, cancellationToken);
        if (notification != null)
        {
            notification.Status = NotificationStatus.Delivered;
            notification.DeliveredAt = DateTime.UtcNow;
            await repository.UpdateAsync(notification, cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "NotificationService", directory, CSharp)
        {
            Namespace = "Notification.Infrastructure.Services"
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
        classModel.Usings.Add(new UsingModel("Notification.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Notification.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Notification.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Notification.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddNotificationInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""NotificationDb"") ??
                @""Server=.\SQLEXPRESS;Database=NotificationDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateNotificationsControllerFile(string directory)
    {
        var classModel = new ClassModel("NotificationsController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Notification.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Notification.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel
        {
            Name = "service",
            Type = new TypeModel("INotificationService"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "NotificationsController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "service", Type = new TypeModel("INotificationService") }],
            Body = new ExpressionModel("this.service = service;")
        };
        classModel.Constructors.Add(constructor);

        var sendMethod = new MethodModel
        {
            Name = "Send",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("NotificationDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("SendNotificationRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var notification = await service.SendAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = notification.NotificationId }, notification);")
        };
        sendMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"send\"" });
        classModel.Methods.Add(sendMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("NotificationDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var notification = await service.GetByIdAsync(id, cancellationToken);
        if (notification == null) return NotFound();
        return Ok(notification);")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        classModel.Methods.Add(getByIdMethod);

        var getByUserIdMethod = new MethodModel
        {
            Name = "GetByUserId",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("NotificationDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var notifications = await service.GetByUserIdAsync(userId, cancellationToken);
        return Ok(notifications);")
        };
        getByUserIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"user/{userId:guid}\"" });
        classModel.Methods.Add(getByUserIdMethod);

        return new CodeFileModel<ClassModel>(classModel, "NotificationsController", directory, CSharp)
        {
            Namespace = "Notification.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddNotificationInfrastructure(builder.Configuration);
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
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "NotificationDb": "Server=.\\SQLEXPRESS;Database=NotificationDb;Trusted_Connection=True;TrustServerCertificate=True"
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
