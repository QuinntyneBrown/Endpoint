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

namespace Endpoint.Engineering.Microservices.DocumentStorage;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

/// <summary>
/// Factory for creating DocumentStorage microservice artifacts according to document-storage-microservice.spec.md.
/// </summary>
public class DocumentStorageArtifactFactory : IDocumentStorageArtifactFactory
{
    private readonly ILogger<DocumentStorageArtifactFactory> logger;

    public DocumentStorageArtifactFactory(ILogger<DocumentStorageArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding DocumentStorage.Core files");

        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");

        // Entities
        project.Files.Add(CreateIAggregateRootFile(entitiesDir));
        project.Files.Add(CreateDocumentEntityFile(entitiesDir));
        project.Files.Add(CreateDocumentVersionEntityFile(entitiesDir));
        project.Files.Add(CreateDocumentMetadataEntityFile(entitiesDir));
        project.Files.Add(CreateDocumentTagEntityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIDomainEventFile(interfacesDir));
        project.Files.Add(CreateIDocumentRepositoryFile(interfacesDir));
        project.Files.Add(CreateIStorageProviderFile(interfacesDir));

        // Events
        project.Files.Add(CreateDocumentUploadedEventFile(eventsDir));
        project.Files.Add(CreateDocumentDeletedEventFile(eventsDir));
        project.Files.Add(CreateDocumentVersionedEventFile(eventsDir));

        // DTOs
        var dtosDir = Path.Combine(project.Directory, "DTOs");
        project.Files.Add(CreateDocumentDtoFile(dtosDir));
        project.Files.Add(CreateDocumentVersionDtoFile(dtosDir));
        project.Files.Add(CreateUploadDocumentRequestFile(dtosDir));
        project.Files.Add(CreateUploadDocumentResponseFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding DocumentStorage.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // DbContext
        project.Files.Add(CreateDocumentStorageDbContextFile(dataDir));

        // Entity Configurations
        project.Files.Add(CreateDocumentConfigurationFile(configurationsDir));
        project.Files.Add(CreateDocumentVersionConfigurationFile(configurationsDir));
        project.Files.Add(CreateDocumentMetadataConfigurationFile(configurationsDir));
        project.Files.Add(CreateDocumentTagConfigurationFile(configurationsDir));

        // Repositories
        project.Files.Add(CreateDocumentRepositoryFile(repositoriesDir));

        // Services
        project.Files.Add(CreateFileSystemStorageProviderFile(servicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding DocumentStorage.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Controllers
        project.Files.Add(CreateDocumentsControllerFile(controllersDir));

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
            Namespace = "DocumentStorage.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentEntityFile(string directory)
    {
        var classModel = new ClassModel("Document");

        classModel.Implements.Add(new TypeModel("IAggregateRoot"));

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "FileName", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "FileSizeBytes", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "StoragePath", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "TenantId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "UploadedBy", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("bool"), "IsDeleted", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "DeletedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("DocumentVersion")] }, "Versions", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "new List<DocumentVersion>()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DocumentMetadata") { Nullable = true }, "Metadata", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("ICollection") { GenericTypeParameters = [new TypeModel("DocumentTag")] }, "Tags", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "new List<DocumentTag>()" });

        return new CodeFileModel<ClassModel>(classModel, "Document", directory, CSharp)
        {
            Namespace = "DocumentStorage.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentVersionEntityFile(string directory)
    {
        var classModel = new ClassModel("DocumentVersion");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "VersionId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Document"), "Document", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("int"), "VersionNumber", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "StoragePath", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "FileSizeBytes", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid") { Nullable = true }, "CreatedBy", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ChangeDescription", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "DocumentVersion", directory, CSharp)
        {
            Namespace = "DocumentStorage.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentMetadataEntityFile(string directory)
    {
        var classModel = new ClassModel("DocumentMetadata");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "MetadataId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Document"), "Document", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Title", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Description", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Author", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "Category", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("string")] }, "CustomProperties", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "new()" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "UpdatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));

        return new CodeFileModel<ClassModel>(classModel, "DocumentMetadata", directory, CSharp)
        {
            Namespace = "DocumentStorage.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentTagEntityFile(string directory)
    {
        var classModel = new ClassModel("DocumentTag");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "TagId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "DocumentId", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Document"), "Document", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "null!" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Name", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null), new PropertyAccessorModel(PropertyAccessorType.Set, null)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "DocumentTag", directory, CSharp)
        {
            Namespace = "DocumentStorage.Core.Entities"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIDomainEventFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IDomainEvent");

        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("Guid"), "AggregateId", [new PropertyAccessorModel(PropertyAccessorType.Get, null)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("string"), "AggregateType", [new PropertyAccessorModel(PropertyAccessorType.Get, null)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get, null)]));
        interfaceModel.Properties.Add(new PropertyModel(interfaceModel, AccessModifier.Public, new TypeModel("string"), "CorrelationId", [new PropertyAccessorModel(PropertyAccessorType.Get, null)]));

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IDomainEvent", directory, CSharp)
        {
            Namespace = "DocumentStorage.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIDocumentRepositoryFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IDocumentRepository");

        interfaceModel.Usings.Add(new UsingModel("DocumentStorage.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Document") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "documentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdWithVersionsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Document") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "documentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Document")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetByTenantIdAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Document")] }] },
            Params =
            [
                new ParamModel { Name = "tenantId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Document")] },
            Params =
            [
                new ParamModel { Name = "document", Type = new TypeModel("Document") },
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
                new ParamModel { Name = "document", Type = new TypeModel("Document") },
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
                new ParamModel { Name = "documentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "AddVersionAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("DocumentVersion")] },
            Params =
            [
                new ParamModel { Name = "version", Type = new TypeModel("DocumentVersion") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "SearchByTagsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Document")] }] },
            Params =
            [
                new ParamModel { Name = "tags", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IDocumentRepository", directory, CSharp)
        {
            Namespace = "DocumentStorage.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateIStorageProviderFile(string directory)
    {
        var interfaceModel = new InterfaceModel("IStorageProvider");

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "SaveAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "content", Type = new TypeModel("Stream") },
                new ParamModel { Name = "fileName", Type = new TypeModel("string") },
                new ParamModel { Name = "contentType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Stream")] },
            Params =
            [
                new ParamModel { Name = "storagePath", Type = new TypeModel("string") },
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
                new ParamModel { Name = "storagePath", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ExistsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "storagePath", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetDownloadUrlAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "storagePath", Type = new TypeModel("string") },
                new ParamModel { Name = "expiration", Type = new TypeModel("TimeSpan") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "IStorageProvider", directory, CSharp)
        {
            Namespace = "DocumentStorage.Core.Interfaces"
        };
    }

    private static FileModel CreateDocumentUploadedEventFile(string directory)
    {
        return new FileModel("DocumentUploadedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using DocumentStorage.Core.Interfaces;

                namespace DocumentStorage.Core.Events;

                /// <summary>
                /// Event raised when a new document is uploaded.
                /// </summary>
                public sealed class DocumentUploadedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "Document";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string FileName { get; init; }

                    public required string ContentType { get; init; }

                    public long FileSizeBytes { get; init; }

                    public Guid? UploadedBy { get; init; }

                    public Guid? TenantId { get; init; }
                }
                """
        };
    }

    private static FileModel CreateDocumentDeletedEventFile(string directory)
    {
        return new FileModel("DocumentDeletedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using DocumentStorage.Core.Interfaces;

                namespace DocumentStorage.Core.Events;

                /// <summary>
                /// Event raised when a document is deleted.
                /// </summary>
                public sealed class DocumentDeletedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "Document";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string FileName { get; init; }

                    public Guid? DeletedBy { get; init; }

                    public required string Reason { get; init; }
                }
                """
        };
    }

    private static FileModel CreateDocumentVersionedEventFile(string directory)
    {
        return new FileModel("DocumentVersionedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using DocumentStorage.Core.Interfaces;

                namespace DocumentStorage.Core.Events;

                /// <summary>
                /// Event raised when a new version of a document is created.
                /// </summary>
                public sealed class DocumentVersionedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "Document";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public Guid VersionId { get; init; }

                    public int VersionNumber { get; init; }

                    public Guid? CreatedBy { get; init; }

                    public string? ChangeDescription { get; init; }
                }
                """
        };
    }

    private static FileModel CreateDocumentDtoFile(string directory)
    {
        return new FileModel("DocumentDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace DocumentStorage.Core.DTOs;

                /// <summary>
                /// Data transfer object for Document.
                /// </summary>
                public sealed class DocumentDto
                {
                    public Guid DocumentId { get; init; }

                    public required string FileName { get; init; }

                    public required string ContentType { get; init; }

                    public long FileSizeBytes { get; init; }

                    public Guid? TenantId { get; init; }

                    public Guid? UploadedBy { get; init; }

                    public DateTime CreatedAt { get; init; }

                    public DateTime? UpdatedAt { get; init; }

                    public int VersionCount { get; init; }

                    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
                }
                """
        };
    }

    private static FileModel CreateDocumentVersionDtoFile(string directory)
    {
        return new FileModel("DocumentVersionDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace DocumentStorage.Core.DTOs;

                /// <summary>
                /// Data transfer object for DocumentVersion.
                /// </summary>
                public sealed class DocumentVersionDto
                {
                    public Guid VersionId { get; init; }

                    public Guid DocumentId { get; init; }

                    public int VersionNumber { get; init; }

                    public long FileSizeBytes { get; init; }

                    public required string ContentType { get; init; }

                    public Guid? CreatedBy { get; init; }

                    public DateTime CreatedAt { get; init; }

                    public string? ChangeDescription { get; init; }
                }
                """
        };
    }

    private static FileModel CreateUploadDocumentRequestFile(string directory)
    {
        return new FileModel("UploadDocumentRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace DocumentStorage.Core.DTOs;

                /// <summary>
                /// Request model for document upload.
                /// </summary>
                public sealed class UploadDocumentRequest
                {
                    [Required]
                    public required string FileName { get; init; }

                    public string? Title { get; init; }

                    public string? Description { get; init; }

                    public Guid? TenantId { get; init; }

                    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
                }
                """
        };
    }

    private static FileModel CreateUploadDocumentResponseFile(string directory)
    {
        return new FileModel("UploadDocumentResponse", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace DocumentStorage.Core.DTOs;

                /// <summary>
                /// Response model for successful document upload.
                /// </summary>
                public sealed class UploadDocumentResponse
                {
                    public Guid DocumentId { get; init; }

                    public required string FileName { get; init; }

                    public required string ContentType { get; init; }

                    public long FileSizeBytes { get; init; }

                    public DateTime CreatedAt { get; init; }
                }
                """
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateDocumentStorageDbContextFile(string directory)
    {
        var classModel = new ClassModel("DocumentStorageDbContext");

        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "DocumentStorageDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("DocumentStorageDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("Document")] }, "Documents", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<Document>()")]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("DocumentVersion")] }, "DocumentVersions", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<DocumentVersion>()")]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("DocumentMetadata")] }, "DocumentMetadata", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<DocumentMetadata>()")]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("DocumentTag")] }, "DocumentTags", [new PropertyAccessorModel(PropertyAccessorType.Get, "Set<DocumentTag>()")]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentStorageDbContext).Assembly);")
        });

        return new CodeFileModel<ClassModel>(classModel, "DocumentStorageDbContext", directory, CSharp)
        {
            Namespace = "DocumentStorage.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentConfigurationFile(string directory)
    {
        var classModel = new ClassModel("DocumentConfiguration");

        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("Document")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("Document")] } }],
            Body = new ExpressionModel(@"builder.HasKey(d => d.DocumentId);

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.HasIndex(d => d.TenantId);
        builder.HasIndex(d => d.UploadedBy);
        builder.HasIndex(d => d.CreatedAt);

        builder.HasQueryFilter(d => !d.IsDeleted);")
        });

        return new CodeFileModel<ClassModel>(classModel, "DocumentConfiguration", directory, CSharp)
        {
            Namespace = "DocumentStorage.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentVersionConfigurationFile(string directory)
    {
        var classModel = new ClassModel("DocumentVersionConfiguration");

        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("DocumentVersion")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("DocumentVersion")] } }],
            Body = new ExpressionModel(@"builder.HasKey(v => v.VersionId);

        builder.Property(v => v.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(v => v.ContentType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(v => v.ChangeDescription)
            .HasMaxLength(500);

        builder.HasOne(v => v.Document)
            .WithMany(d => d.Versions)
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.DocumentId, v.VersionNumber })
            .IsUnique();")
        });

        return new CodeFileModel<ClassModel>(classModel, "DocumentVersionConfiguration", directory, CSharp)
        {
            Namespace = "DocumentStorage.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentMetadataConfigurationFile(string directory)
    {
        var classModel = new ClassModel("DocumentMetadataConfiguration");

        classModel.Usings.Add(new UsingModel("System.Text.Json"));
        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("DocumentMetadata")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("DocumentMetadata")] } }],
            Body = new ExpressionModel(@"builder.HasKey(m => m.MetadataId);

        builder.Property(m => m.Title)
            .HasMaxLength(500);

        builder.Property(m => m.Description)
            .HasMaxLength(2000);

        builder.Property(m => m.Author)
            .HasMaxLength(255);

        builder.Property(m => m.Category)
            .HasMaxLength(100);

        builder.Property(m => m.CustomProperties)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

        builder.HasOne(m => m.Document)
            .WithOne(d => d.Metadata)
            .HasForeignKey<DocumentMetadata>(m => m.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);")
        });

        return new CodeFileModel<ClassModel>(classModel, "DocumentMetadataConfiguration", directory, CSharp)
        {
            Namespace = "DocumentStorage.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentTagConfigurationFile(string directory)
    {
        var classModel = new ClassModel("DocumentTagConfiguration");

        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore.Metadata.Builders"));

        classModel.Implements.Add(new TypeModel("IEntityTypeConfiguration") { GenericTypeParameters = [new TypeModel("DocumentTag")] });

        classModel.Methods.Add(new MethodModel
        {
            Name = "Configure",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "builder", Type = new TypeModel("EntityTypeBuilder") { GenericTypeParameters = [new TypeModel("DocumentTag")] } }],
            Body = new ExpressionModel(@"builder.HasKey(t => t.TagId);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(t => t.Document)
            .WithMany(d => d.Tags)
            .HasForeignKey(t => t.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.DocumentId, t.Name })
            .IsUnique();

        builder.HasIndex(t => t.Name);")
        });

        return new CodeFileModel<ClassModel>(classModel, "DocumentTagConfiguration", directory, CSharp)
        {
            Namespace = "DocumentStorage.Infrastructure.Data.Configurations"
        };
    }

    private static CodeFileModel<ClassModel> CreateDocumentRepositoryFile(string directory)
    {
        var classModel = new ClassModel("DocumentRepository");

        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.Entities"));
        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("DocumentStorage.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));

        classModel.Implements.Add(new TypeModel("IDocumentRepository"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("DocumentStorageDbContext"),
            AccessModifier = AccessModifier.Private,
            Readonly = true
        });

        var constructor = new ConstructorModel(classModel, "DocumentRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("DocumentStorageDbContext") }],
            Body = new ExpressionModel("this.context = context ?? throw new ArgumentNullException(nameof(context));")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Document") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "documentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Documents
            .Include(d => d.Metadata)
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByIdWithVersionsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Document") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "documentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Documents
            .Include(d => d.Metadata)
            .Include(d => d.Tags)
            .Include(d => d.Versions.OrderByDescending(v => v.VersionNumber))
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Document")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Documents
            .Include(d => d.Metadata)
            .Include(d => d.Tags)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByTenantIdAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Document")] }] },
            Params =
            [
                new ParamModel { Name = "tenantId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"return await context.Documents
            .Include(d => d.Metadata)
            .Include(d => d.Tags)
            .Where(d => d.TenantId == tenantId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Document")] },
            Params =
            [
                new ParamModel { Name = "document", Type = new TypeModel("Document") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"document.DocumentId = Guid.NewGuid();
        document.CreatedAt = DateTime.UtcNow;
        await context.Documents.AddAsync(document, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return document;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "document", Type = new TypeModel("Document") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"document.UpdatedAt = DateTime.UtcNow;
        context.Documents.Update(document);
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
                new ParamModel { Name = "documentId", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var document = await context.Documents.FindAsync(new object[] { documentId }, cancellationToken);
        if (document != null)
        {
            document.IsDeleted = true;
            document.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddVersionAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("DocumentVersion")] },
            Params =
            [
                new ParamModel { Name = "version", Type = new TypeModel("DocumentVersion") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"version.VersionId = Guid.NewGuid();
        version.CreatedAt = DateTime.UtcNow;
        await context.DocumentVersions.AddAsync(version, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return version;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "SearchByTagsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("Document")] }] },
            Params =
            [
                new ParamModel { Name = "tags", Type = new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("string")] } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var tagList = tags.ToList();
        return await context.Documents
            .Include(d => d.Metadata)
            .Include(d => d.Tags)
            .Where(d => d.Tags.Any(t => tagList.Contains(t.Name)))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);")
        });

        return new CodeFileModel<ClassModel>(classModel, "DocumentRepository", directory, CSharp)
        {
            Namespace = "DocumentStorage.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateFileSystemStorageProviderFile(string directory)
    {
        var classModel = new ClassModel("FileSystemStorageProvider");

        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));

        classModel.Implements.Add(new TypeModel("IStorageProvider"));

        classModel.Fields.Add(new FieldModel { Name = "basePath", Type = new TypeModel("string"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("FileSystemStorageProvider")] }, AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "FileSystemStorageProvider")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("FileSystemStorageProvider")] } }
            ],
            Body = new ExpressionModel(@"this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        basePath = configuration[""Storage:BasePath""] ?? Path.Combine(Path.GetTempPath(), ""DocumentStorage"");

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "SaveAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "content", Type = new TypeModel("Stream") },
                new ParamModel { Name = "fileName", Type = new TypeModel("string") },
                new ParamModel { Name = "contentType", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var storagePath = GenerateStoragePath(fileName);
        var fullPath = Path.Combine(basePath, storagePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        logger.LogInformation(""Document saved to {StoragePath}"", storagePath);
        return storagePath;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("Stream")] },
            Params =
            [
                new ParamModel { Name = "storagePath", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var fullPath = Path.Combine(basePath, storagePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($""Document not found at path: {storagePath}"");
        }

        var memoryStream = new MemoryStream();
        await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "storagePath", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var fullPath = Path.Combine(basePath, storagePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            logger.LogInformation(""Document deleted from {StoragePath}"", storagePath);
        }

        return Task.CompletedTask;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ExistsAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "storagePath", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var fullPath = Path.Combine(basePath, storagePath);
        return Task.FromResult(File.Exists(fullPath));")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetDownloadUrlAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("string")] },
            Params =
            [
                new ParamModel { Name = "storagePath", Type = new TypeModel("string") },
                new ParamModel { Name = "expiration", Type = new TypeModel("TimeSpan") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"// For file system storage, return the relative path
        // In a real implementation, this would generate a signed URL
        return Task.FromResult($""/api/documents/download/{Uri.EscapeDataString(storagePath)}"");")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GenerateStoragePath",
            AccessModifier = AccessModifier.Private,
            Static = true,
            ReturnType = new TypeModel("string"),
            Params = [new ParamModel { Name = "fileName", Type = new TypeModel("string") }],
            Body = new ExpressionModel(@"var date = DateTime.UtcNow;
        var uniqueId = Guid.NewGuid().ToString(""N"")[..8];
        var extension = Path.GetExtension(fileName);
        var sanitizedName = Path.GetFileNameWithoutExtension(fileName);

        return Path.Combine(
            date.Year.ToString(),
            date.Month.ToString(""D2""),
            date.Day.ToString(""D2""),
            $""{sanitizedName}_{uniqueId}{extension}"");")
        });

        return new CodeFileModel<ClassModel>(classModel, "FileSystemStorageProvider", directory, CSharp)
        {
            Namespace = "DocumentStorage.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("DocumentStorage.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("DocumentStorage.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("DocumentStorage.Infrastructure.Services"));
        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddDocumentStorageInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<DocumentStorageDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString(""DocumentStorageDb"") ??
                @""Server=.\SQLEXPRESS;Database=DocumentStorageDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IStorageProvider, FileSystemStorageProvider>();

        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateDocumentsControllerFile(string directory)
    {
        var classModel = new ClassModel("DocumentsController");

        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.Entities"));
        classModel.Usings.Add(new UsingModel("DocumentStorage.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Authorization"));
        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Authorize" });

        classModel.Fields.Add(new FieldModel { Name = "documentRepository", Type = new TypeModel("IDocumentRepository"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "storageProvider", Type = new TypeModel("IStorageProvider"), AccessModifier = AccessModifier.Private, Readonly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("DocumentsController")] }, AccessModifier = AccessModifier.Private, Readonly = true });

        var constructor = new ConstructorModel(classModel, "DocumentsController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "documentRepository", Type = new TypeModel("IDocumentRepository") },
                new ParamModel { Name = "storageProvider", Type = new TypeModel("IStorageProvider") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("DocumentsController")] } }
            ],
            Body = new ExpressionModel(@"this.documentRepository = documentRepository;
        this.storageProvider = storageProvider;
        this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        var uploadMethod = new MethodModel
        {
            Name = "Upload",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("UploadDocumentResponse")] }] },
            Params =
            [
                new ParamModel { Name = "file", Type = new TypeModel("IFormFile") },
                new ParamModel { Name = "request", Type = new TypeModel("UploadDocumentRequest"), Attribute = "[FromForm]" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = ""No file provided"" });
        }

        await using var stream = file.OpenReadStream();
        var storagePath = await storageProvider.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);

        var document = new Document
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            StoragePath = storagePath,
            TenantId = request.TenantId
        };

        if (!string.IsNullOrEmpty(request.Title) || !string.IsNullOrEmpty(request.Description))
        {
            document.Metadata = new DocumentMetadata
            {
                Title = request.Title,
                Description = request.Description
            };
        }

        foreach (var tagName in request.Tags)
        {
            document.Tags.Add(new DocumentTag { Name = tagName });
        }

        var createdDocument = await documentRepository.AddAsync(document, cancellationToken);

        logger.LogInformation(""Document {FileName} uploaded with ID {DocumentId}"", file.FileName, createdDocument.DocumentId);

        var response = new UploadDocumentResponse
        {
            DocumentId = createdDocument.DocumentId,
            FileName = createdDocument.FileName,
            ContentType = createdDocument.ContentType,
            FileSizeBytes = createdDocument.FileSizeBytes,
            CreatedAt = createdDocument.CreatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = createdDocument.DocumentId }, response);")
        };
        uploadMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost" });
        uploadMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(UploadDocumentResponse), StatusCodes.Status201Created" });
        uploadMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status400BadRequest" });
        classModel.Methods.Add(uploadMethod);

        var getByIdMethod = new MethodModel
        {
            Name = "GetById",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("DocumentDto")] }] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var document = await documentRepository.GetByIdWithVersionsAsync(id, cancellationToken);

        if (document == null)
        {
            return NotFound();
        }

        return Ok(new DocumentDto
        {
            DocumentId = document.DocumentId,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            TenantId = document.TenantId,
            UploadedBy = document.UploadedBy,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            VersionCount = document.Versions.Count,
            Tags = document.Tags.Select(t => t.Name).ToList()
        });")
        };
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}\"" });
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(DocumentDto), StatusCodes.Status200OK" });
        getByIdMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(getByIdMethod);

        var deleteMethod = new MethodModel
        {
            Name = "Delete",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var document = await documentRepository.GetByIdAsync(id, cancellationToken);

        if (document == null)
        {
            return NotFound();
        }

        await documentRepository.DeleteAsync(id, cancellationToken);
        logger.LogInformation(""Document {DocumentId} deleted"", id);

        return NoContent();")
        };
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpDelete", Template = "\"{id:guid}\"" });
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status204NoContent" });
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(deleteMethod);

        var downloadMethod = new MethodModel
        {
            Name = "Download",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "id", Type = new TypeModel("Guid") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var document = await documentRepository.GetByIdAsync(id, cancellationToken);

        if (document == null)
        {
            return NotFound();
        }

        var stream = await storageProvider.GetAsync(document.StoragePath, cancellationToken);
        return File(stream, document.ContentType, document.FileName);")
        };
        downloadMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{id:guid}/download\"" });
        downloadMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(FileStreamResult), StatusCodes.Status200OK" });
        downloadMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(downloadMethod);

        var getAllMethod = new MethodModel
        {
            Name = "GetAll",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("IEnumerable") { GenericTypeParameters = [new TypeModel("DocumentDto")] }] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var documents = await documentRepository.GetAllAsync(cancellationToken);

        var documentDtos = documents.Select(document => new DocumentDto
        {
            DocumentId = document.DocumentId,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            TenantId = document.TenantId,
            UploadedBy = document.UploadedBy,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            VersionCount = document.Versions.Count,
            Tags = document.Tags.Select(t => t.Name).ToList()
        });

        return Ok(documentDtos);")
        };
        getAllMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet" });
        getAllMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK" });
        classModel.Methods.Add(getAllMethod);

        return new CodeFileModel<ClassModel>(classModel, "DocumentsController", directory, CSharp)
        {
            Namespace = "DocumentStorage.Api.Controllers"
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
                builder.Services.AddDocumentStorageInfrastructure(builder.Configuration);

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
                        Title = "DocumentStorage API",
                        Version = "v1",
                        Description = "DocumentStorage microservice for document management"
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
                    "DocumentStorageDb": "Server=.\\SQLEXPRESS;Database=DocumentStorageDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Storage": {
                    "BasePath": "C:\\DocumentStorage\\Files"
                  },
                  "Jwt": {
                    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
                    "Issuer": "DocumentStorage.Api",
                    "Audience": "DocumentStorage.Api",
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
