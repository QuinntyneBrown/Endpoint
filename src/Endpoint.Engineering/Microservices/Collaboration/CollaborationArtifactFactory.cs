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

namespace Endpoint.Engineering.Microservices.Collaboration;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

/// <summary>
/// Factory for creating Collaboration microservice artifacts.
/// Manages sharing, comments, mentions, and activity tracking.
/// </summary>
public class CollaborationArtifactFactory : ICollaborationArtifactFactory
{
    private readonly ILogger<CollaborationArtifactFactory> logger;

    public CollaborationArtifactFactory(ILogger<CollaborationArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Collaboration.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateShareFile(entitiesDir));
        project.Files.Add(CreateCommentFile(entitiesDir));
        project.Files.Add(CreateMentionFile(entitiesDir));
        project.Files.Add(CreateActivityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIShareRepositoryFile(interfacesDir));
        project.Files.Add(CreateICommentRepositoryFile(interfacesDir));
        project.Files.Add(CreateICollaborationServiceFile(interfacesDir));

        // Events
        project.Files.Add(CreateEntitySharedEventFile(eventsDir));
        project.Files.Add(CreateCommentAddedEventFile(eventsDir));
        project.Files.Add(CreateMentionCreatedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateShareDtoFile(dtosDir));
        project.Files.Add(CreateCreateShareRequestFile(dtosDir));
        project.Files.Add(CreateCommentDtoFile(dtosDir));
        project.Files.Add(CreateCreateCommentRequestFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Collaboration.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // DbContext
        project.Files.Add(CreateCollaborationDbContextFile(dataDir));

        // Repositories
        project.Files.Add(CreateShareRepositoryFile(repositoriesDir));
        project.Files.Add(CreateCommentRepositoryFile(repositoriesDir));

        // Services
        project.Files.Add(CreateCollaborationServiceFile(servicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Collaboration.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Controllers
        project.Files.Add(CreateSharesControllerFile(controllersDir));
        project.Files.Add(CreateCommentsControllerFile(controllersDir));

        // Program.cs
        project.Files.Add(CreateProgramFile(project.Directory));

        // appsettings.json
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files - Entities

    private static FileModel CreateShareFile(string directory)
    {
        // Contains enum SharePermission, keeping as FileModel
        return new FileModel("Share", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Collaboration.Core.Entities;

                public class Share
                {
                    public Guid ShareId { get; set; }
                    public Guid EntityId { get; set; }
                    public required string EntityType { get; set; }
                    public Guid SharedByUserId { get; set; }
                    public Guid SharedWithUserId { get; set; }
                    public SharePermission Permission { get; set; } = SharePermission.View;
                    public DateTime SharedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? ExpiresAt { get; set; }
                    public bool IsActive { get; set; } = true;
                }

                public enum SharePermission { View, Edit, Admin }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateCommentFile(string directory)
    {
        var classModel = new ClassModel("Comment");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "CommentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Content", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "ParentCommentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Comment") { Nullable = true }, "ParentComment", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("Comment")] }, "Replies", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<Comment>()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("Mention")] }, "Mentions", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new List<Mention>()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsDeleted", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "false" });

        return new CodeFileModel<ClassModel>(classModel, "Comment", directory, CSharp)
        {
            Namespace = "Collaboration.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateMentionFile(string directory)
    {
        var classModel = new ClassModel("Mention");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MentionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "CommentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Comment"), "Comment", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MentionedUserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsRead", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "false" });

        return new CodeFileModel<ClassModel>(classModel, "Mention", directory, CSharp)
        {
            Namespace = "Collaboration.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateActivityFile(string directory)
    {
        var classModel = new ClassModel("Activity");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ActivityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ActionType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Metadata", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "Activity", directory, CSharp)
        {
            Namespace = "Collaboration.Core.Entities"
        };
    }

    #endregion

    #region Core Layer Files - Interfaces

    private static CodeFileModel<InterfaceModel> CreateIShareRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IShareRepository");

        interfaceModel.Usings.Add(new UsingModel("Collaboration.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Share") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "shareId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByEntityIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Share")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Share")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Share")] },
            Params =
            [
                new ParamModel { Name = "share", Type = new TypeModel("Share") },
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
                new ParamModel { Name = "share", Type = new TypeModel("Share") },
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
                new ParamModel { Name = "shareId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IShareRepository", directory, CSharp)
        {
            Namespace = "Collaboration.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateICommentRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ICommentRepository");

        interfaceModel.Usings.Add(new UsingModel("Collaboration.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Comment") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "commentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByEntityIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Comment")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Comment")] },
            Params =
            [
                new ParamModel { Name = "comment", Type = new TypeModel("Comment") },
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
                new ParamModel { Name = "comment", Type = new TypeModel("Comment") },
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
                new ParamModel { Name = "commentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ICommentRepository", directory, CSharp)
        {
            Namespace = "Collaboration.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateICollaborationServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ICollaborationService");

        interfaceModel.Usings.Add(new UsingModel("Collaboration.Core.DTOs"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ShareEntityAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ShareDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateShareRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetSharesByEntityIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ShareDto")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "RevokeShareAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "shareId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddCommentAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("CommentDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateCommentRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetCommentsByEntityIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("CommentDto")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "DeleteCommentAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "commentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ICollaborationService", directory, CSharp)
        {
            Namespace = "Collaboration.Core.Interfaces"
        };
    }

    #endregion

    #region Core Layer Files - Events

    private static CodeFileModel<ClassModel> CreateEntitySharedEventFile(string directory)
    {
        var classModel = new ClassModel("EntitySharedEvent") { Sealed = true };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ShareId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SharedByUserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SharedWithUserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Permission", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "EntitySharedEvent", directory, CSharp)
        {
            Namespace = "Collaboration.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateCommentAddedEventFile(string directory)
    {
        var classModel = new ClassModel("CommentAddedEvent") { Sealed = true };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "CommentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Content", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "CommentAddedEvent", directory, CSharp)
        {
            Namespace = "Collaboration.Core.Events"
        };
    }

    private static CodeFileModel<ClassModel> CreateMentionCreatedEventFile(string directory)
    {
        var classModel = new ClassModel("MentionCreatedEvent") { Sealed = true };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MentionId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "CommentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MentionedUserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MentionedByUserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "MentionCreatedEvent", directory, CSharp)
        {
            Namespace = "Collaboration.Core.Events"
        };
    }

    #endregion

    #region Core Layer Files - DTOs

    private static CodeFileModel<ClassModel> CreateShareDtoFile(string directory)
    {
        var classModel = new ClassModel("ShareDto") { Sealed = true };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "ShareId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SharedByUserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "SharedWithUserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Permission", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "\"View\"" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "SharedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "ExpiresAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsActive", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "ShareDto", directory, CSharp)
        {
            Namespace = "Collaboration.Core.DTOs"
        };
    }

    private static FileModel CreateCreateShareRequestFile(string directory)
    {
        // Contains [Required] attributes, keeping as FileModel
        return new FileModel("CreateShareRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Collaboration.Core.DTOs;

                public sealed class CreateShareRequest
                {
                    [Required]
                    public Guid EntityId { get; init; }

                    [Required]
                    public required string EntityType { get; init; }

                    [Required]
                    public Guid SharedByUserId { get; init; }

                    [Required]
                    public Guid SharedWithUserId { get; init; }

                    public string Permission { get; init; } = "View";

                    public DateTime? ExpiresAt { get; init; }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateCommentDtoFile(string directory)
    {
        var classModel = new ClassModel("CommentDto") { Sealed = true };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "CommentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntityId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "EntityType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "UserId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Content", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "ParentCommentId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("CommentDto")] }, "Replies", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "Enumerable.Empty<CommentDto>()" });

        return new CodeFileModel<ClassModel>(classModel, "CommentDto", directory, CSharp)
        {
            Namespace = "Collaboration.Core.DTOs"
        };
    }

    private static FileModel CreateCreateCommentRequestFile(string directory)
    {
        // Contains [Required] attributes, keeping as FileModel
        return new FileModel("CreateCommentRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Collaboration.Core.DTOs;

                public sealed class CreateCommentRequest
                {
                    [Required]
                    public Guid EntityId { get; init; }

                    [Required]
                    public required string EntityType { get; init; }

                    [Required]
                    public Guid UserId { get; init; }

                    [Required]
                    public required string Content { get; init; }

                    public Guid? ParentCommentId { get; init; }

                    public IEnumerable<Guid> MentionedUserIds { get; init; } = Enumerable.Empty<Guid>();
                }
                """
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateCollaborationDbContextFile(string directory)
    {
        var classModel = new ClassModel("CollaborationDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Collaboration.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "CollaborationDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("CollaborationDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Share")] }, "Shares", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Comment")] }, "Comments", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Mention")] }, "Mentions", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Activity")] }, "Activities", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<Share>(entity =>
        {
            entity.HasKey(s => s.ShareId);
            entity.Property(s => s.EntityType).IsRequired().HasMaxLength(100);
            entity.HasIndex(s => new { s.EntityId, s.SharedWithUserId }).IsUnique();
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.CommentId);
            entity.Property(c => c.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Content).IsRequired();
            entity.HasOne(c => c.ParentComment).WithMany(c => c.Replies).HasForeignKey(c => c.ParentCommentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Mention>(entity =>
        {
            entity.HasKey(m => m.MentionId);
            entity.HasOne(m => m.Comment).WithMany(c => c.Mentions).HasForeignKey(m => m.CommentId);
            entity.HasIndex(m => new { m.CommentId, m.MentionedUserId }).IsUnique();
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(a => a.ActivityId);
            entity.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(a => a.ActionType).IsRequired().HasMaxLength(50);
            entity.HasIndex(a => new { a.EntityId, a.OccurredAt });
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "CollaborationDbContext", directory, CSharp)
        {
            Namespace = "Collaboration.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateShareRepositoryFile(string directory)
    {
        var classModel = new ClassModel("ShareRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Collaboration.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Collaboration.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Collaboration.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("IShareRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("CollaborationDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "ShareRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("CollaborationDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Share") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "shareId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Shares.FirstOrDefaultAsync(s => s.ShareId == shareId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByEntityIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Share")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Shares.Where(s => s.EntityId == entityId && s.IsActive).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByUserIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Share")] }] },
            Params =
            [
                new ParamModel { Name = "userId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Shares.Where(s => s.SharedWithUserId == userId && s.IsActive).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Share")] },
            Params =
            [
                new ParamModel { Name = "share", Type = new TypeModel("Share") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"share.ShareId = Guid.NewGuid();
        await context.Shares.AddAsync(share, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return share;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "share", Type = new TypeModel("Share") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.Shares.Update(share);
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
                new ParamModel { Name = "shareId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var share = await context.Shares.FindAsync(new object[] { shareId }, cancellationToken);
        if (share != null)
        {
            share.IsActive = false;
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "ShareRepository", directory, CSharp)
        {
            Namespace = "Collaboration.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateCommentRepositoryFile(string directory)
    {
        var classModel = new ClassModel("CommentRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Collaboration.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Collaboration.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Collaboration.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ICommentRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("CollaborationDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "CommentRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("CollaborationDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Comment") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "commentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Comments.Include(c => c.Mentions).Include(c => c.Replies).FirstOrDefaultAsync(c => c.CommentId == commentId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByEntityIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Comment")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("return await context.Comments.Include(c => c.Mentions).Include(c => c.Replies).Where(c => c.EntityId == entityId && !c.IsDeleted && c.ParentCommentId == null).OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Comment")] },
            Params =
            [
                new ParamModel { Name = "comment", Type = new TypeModel("Comment") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"comment.CommentId = Guid.NewGuid();
        await context.Comments.AddAsync(comment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return comment;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "comment", Type = new TypeModel("Comment") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"comment.UpdatedAt = DateTime.UtcNow;
        context.Comments.Update(comment);
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
                new ParamModel { Name = "commentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var comment = await context.Comments.FindAsync(new object[] { commentId }, cancellationToken);
        if (comment != null)
        {
            comment.IsDeleted = true;
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        return new CodeFileModel<ClassModel>(classModel, "CommentRepository", directory, CSharp)
        {
            Namespace = "Collaboration.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateCollaborationServiceFile(string directory)
    {
        var classModel = new ClassModel("CollaborationService");

        classModel.Usings.Add(new UsingModel("Collaboration.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Collaboration.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Collaboration.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Collaboration.Infrastructure.Data"));

        classModel.Implements.Add(new TypeModel("ICollaborationService"));

        classModel.Fields.Add(new FieldModel { Name = "shareRepository", Type = new TypeModel("IShareRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "commentRepository", Type = new TypeModel("ICommentRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "context", Type = new TypeModel("CollaborationDbContext"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "CollaborationService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "shareRepository", Type = new TypeModel("IShareRepository") },
                new ParamModel { Name = "commentRepository", Type = new TypeModel("ICommentRepository") },
                new ParamModel { Name = "context", Type = new TypeModel("CollaborationDbContext") }
            ],
            Body = new ExpressionModel(@"this.shareRepository = shareRepository;
        this.commentRepository = commentRepository;
        this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "ShareEntityAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ShareDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateShareRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var share = new Share
        {
            EntityId = request.EntityId,
            EntityType = request.EntityType,
            SharedByUserId = request.SharedByUserId,
            SharedWithUserId = request.SharedWithUserId,
            Permission = Enum.Parse<SharePermission>(request.Permission, true),
            ExpiresAt = request.ExpiresAt
        };

        var created = await shareRepository.AddAsync(share, cancellationToken);

        return new ShareDto
        {
            ShareId = created.ShareId,
            EntityId = created.EntityId,
            EntityType = created.EntityType,
            SharedByUserId = created.SharedByUserId,
            SharedWithUserId = created.SharedWithUserId,
            Permission = created.Permission.ToString(),
            SharedAt = created.SharedAt,
            ExpiresAt = created.ExpiresAt,
            IsActive = created.IsActive
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetSharesByEntityIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ShareDto")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var shares = await shareRepository.GetByEntityIdAsync(entityId, cancellationToken);
        return shares.Select(s => new ShareDto
        {
            ShareId = s.ShareId,
            EntityId = s.EntityId,
            EntityType = s.EntityType,
            SharedByUserId = s.SharedByUserId,
            SharedWithUserId = s.SharedWithUserId,
            Permission = s.Permission.ToString(),
            SharedAt = s.SharedAt,
            ExpiresAt = s.ExpiresAt,
            IsActive = s.IsActive
        });")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "RevokeShareAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "shareId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("await shareRepository.DeleteAsync(shareId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddCommentAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("CommentDto")] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateCommentRequest") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var comment = new Comment
        {
            EntityId = request.EntityId,
            EntityType = request.EntityType,
            UserId = request.UserId,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId
        };

        var created = await commentRepository.AddAsync(comment, cancellationToken);

        // Add mentions
        foreach (var mentionedUserId in request.MentionedUserIds)
        {
            var mention = new Mention
            {
                MentionId = Guid.NewGuid(),
                CommentId = created.CommentId,
                MentionedUserId = mentionedUserId
            };
            await context.Mentions.AddAsync(mention, cancellationToken);
        }
        await context.SaveChangesAsync(cancellationToken);

        return new CommentDto
        {
            CommentId = created.CommentId,
            EntityId = created.EntityId,
            EntityType = created.EntityType,
            UserId = created.UserId,
            Content = created.Content,
            ParentCommentId = created.ParentCommentId,
            CreatedAt = created.CreatedAt
        };")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetCommentsByEntityIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("CommentDto")] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var comments = await commentRepository.GetByEntityIdAsync(entityId, cancellationToken);
        return comments.Select(MapCommentToDto);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteCommentAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "commentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("await commentRepository.DeleteAsync(commentId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "MapCommentToDto",
            AccessModifier = AccessModifier.Private,
            Static = true,
            ReturnType = new TypeModel("CommentDto"),
            Params = [new ParamModel { Name = "comment", Type = new TypeModel("Comment") }],
            Body = new ExpressionModel(@"return new CommentDto
        {
            CommentId = comment.CommentId,
            EntityId = comment.EntityId,
            EntityType = comment.EntityType,
            UserId = comment.UserId,
            Content = comment.Content,
            ParentCommentId = comment.ParentCommentId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            Replies = comment.Replies.Where(r => !r.IsDeleted).Select(MapCommentToDto)
        };")
        });

        return new CodeFileModel<ClassModel>(classModel, "CollaborationService", directory, CSharp)
        {
            Namespace = "Collaboration.Infrastructure.Services"
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
        classModel.Usings.Add(new UsingModel("Collaboration.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Collaboration.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Collaboration.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Collaboration.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddCollaborationInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<CollaborationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""CollaborationDb"") ??
                @""Server=.\SQLEXPRESS;Database=CollaborationDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<IShareRepository, ShareRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ICollaborationService, CollaborationService>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateSharesControllerFile(string directory)
    {
        var classModel = new ClassModel("SharesController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Collaboration.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Collaboration.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "service", Type = new TypeModel("ICollaborationService"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "SharesController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "service", Type = new TypeModel("ICollaborationService") }],
            Body = new ExpressionModel("this.service = service;")
        };
        classModel.Constructors.Add(constructor);

        var createMethod = new MethodModel
        {
            Name = "Create",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("ShareDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateShareRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var share = await service.ShareEntityAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByEntityId), new { entityId = share.EntityId }, share);")
        };
        createMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        classModel.Methods.Add(createMethod);

        var getMethod = new MethodModel
        {
            Name = "GetByEntityId",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("ShareDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var shares = await service.GetSharesByEntityIdAsync(entityId, cancellationToken);
        return Ok(shares);")
        };
        getMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{entityId:guid}\"" });
        classModel.Methods.Add(getMethod);

        var revokeMethod = new MethodModel
        {
            Name = "Revoke",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "shareId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"await service.RevokeShareAsync(shareId, cancellationToken);
        return NoContent();")
        };
        revokeMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpDelete", Template = "\"{shareId:guid}\"" });
        classModel.Methods.Add(revokeMethod);

        return new CodeFileModel<ClassModel>(classModel, "SharesController", directory, CSharp)
        {
            Namespace = "Collaboration.Api.Controllers"
        };
    }

    private static CodeFileModel<ClassModel> CreateCommentsControllerFile(string directory)
    {
        var classModel = new ClassModel("CommentsController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Collaboration.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Collaboration.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "service", Type = new TypeModel("ICollaborationService"), AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "CommentsController")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "service", Type = new TypeModel("ICollaborationService") }],
            Body = new ExpressionModel("this.service = service;")
        };
        classModel.Constructors.Add(constructor);

        var createMethod = new MethodModel
        {
            Name = "Create",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("CommentDto")] }] },
            Params =
            [
                new ParamModel { Name = "request", Type = new TypeModel("CreateCommentRequest"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var comment = await service.AddCommentAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByEntityId), new { entityId = comment.EntityId }, comment);")
        };
        createMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        classModel.Methods.Add(createMethod);

        var getMethod = new MethodModel
        {
            Name = "GetByEntityId",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("CommentDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "entityId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var comments = await service.GetCommentsByEntityIdAsync(entityId, cancellationToken);
        return Ok(comments);")
        };
        getMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{entityId:guid}\"" });
        classModel.Methods.Add(getMethod);

        var deleteMethod = new MethodModel
        {
            Name = "Delete",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "commentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"await service.DeleteCommentAsync(commentId, cancellationToken);
        return NoContent();")
        };
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpDelete", Template = "\"{commentId:guid}\"" });
        classModel.Methods.Add(deleteMethod);

        return new CodeFileModel<ClassModel>(classModel, "CommentsController", directory, CSharp)
        {
            Namespace = "Collaboration.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        // Top-level statements, keeping as FileModel
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddCollaborationInfrastructure(builder.Configuration);
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
        // JSON file, keeping as FileModel
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "CollaborationDb": "Server=.\\SQLEXPRESS;Database=CollaborationDb;Trusted_Connection=True;TrustServerCertificate=True"
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
