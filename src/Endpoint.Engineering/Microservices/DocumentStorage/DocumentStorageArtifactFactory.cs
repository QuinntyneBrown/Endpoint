// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.DocumentStorage;

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

    private static FileModel CreateIAggregateRootFile(string directory)
    {
        return new FileModel("IAggregateRoot", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace DocumentStorage.Core.Entities;

                /// <summary>
                /// Marker interface for aggregate roots.
                /// </summary>
                public interface IAggregateRoot
                {
                }
                """
        };
    }

    private static FileModel CreateDocumentEntityFile(string directory)
    {
        return new FileModel("Document", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace DocumentStorage.Core.Entities;

                /// <summary>
                /// Document entity representing a stored document.
                /// </summary>
                public class Document : IAggregateRoot
                {
                    public Guid DocumentId { get; set; }

                    public required string FileName { get; set; }

                    public required string ContentType { get; set; }

                    public long FileSizeBytes { get; set; }

                    public required string StoragePath { get; set; }

                    public Guid? TenantId { get; set; }

                    public Guid? UploadedBy { get; set; }

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? UpdatedAt { get; set; }

                    public bool IsDeleted { get; set; }

                    public DateTime? DeletedAt { get; set; }

                    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();

                    public DocumentMetadata? Metadata { get; set; }

                    public ICollection<DocumentTag> Tags { get; set; } = new List<DocumentTag>();
                }
                """
        };
    }

    private static FileModel CreateDocumentVersionEntityFile(string directory)
    {
        return new FileModel("DocumentVersion", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace DocumentStorage.Core.Entities;

                /// <summary>
                /// Document version entity representing a specific version of a document.
                /// </summary>
                public class DocumentVersion
                {
                    public Guid VersionId { get; set; }

                    public Guid DocumentId { get; set; }

                    public Document Document { get; set; } = null!;

                    public int VersionNumber { get; set; }

                    public required string StoragePath { get; set; }

                    public long FileSizeBytes { get; set; }

                    public required string ContentType { get; set; }

                    public Guid? CreatedBy { get; set; }

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public string? ChangeDescription { get; set; }
                }
                """
        };
    }

    private static FileModel CreateDocumentMetadataEntityFile(string directory)
    {
        return new FileModel("DocumentMetadata", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace DocumentStorage.Core.Entities;

                /// <summary>
                /// Document metadata entity containing additional document information.
                /// </summary>
                public class DocumentMetadata
                {
                    public Guid MetadataId { get; set; }

                    public Guid DocumentId { get; set; }

                    public Document Document { get; set; } = null!;

                    public string? Title { get; set; }

                    public string? Description { get; set; }

                    public string? Author { get; set; }

                    public string? Category { get; set; }

                    public Dictionary<string, string> CustomProperties { get; set; } = new();

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? UpdatedAt { get; set; }
                }
                """
        };
    }

    private static FileModel CreateDocumentTagEntityFile(string directory)
    {
        return new FileModel("DocumentTag", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace DocumentStorage.Core.Entities;

                /// <summary>
                /// Document tag entity for categorizing documents.
                /// </summary>
                public class DocumentTag
                {
                    public Guid TagId { get; set; }

                    public Guid DocumentId { get; set; }

                    public Document Document { get; set; } = null!;

                    public required string Name { get; set; }

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                }
                """
        };
    }

    private static FileModel CreateIDomainEventFile(string directory)
    {
        return new FileModel("IDomainEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace DocumentStorage.Core.Interfaces;

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

    private static FileModel CreateIDocumentRepositoryFile(string directory)
    {
        return new FileModel("IDocumentRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using DocumentStorage.Core.Entities;

                namespace DocumentStorage.Core.Interfaces;

                /// <summary>
                /// Repository interface for Document entities.
                /// </summary>
                public interface IDocumentRepository
                {
                    Task<Document?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default);

                    Task<Document?> GetByIdWithVersionsAsync(Guid documentId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Document>> GetAllAsync(CancellationToken cancellationToken = default);

                    Task<IEnumerable<Document>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

                    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);

                    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);

                    Task DeleteAsync(Guid documentId, CancellationToken cancellationToken = default);

                    Task<DocumentVersion> AddVersionAsync(DocumentVersion version, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Document>> SearchByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateIStorageProviderFile(string directory)
    {
        return new FileModel("IStorageProvider", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace DocumentStorage.Core.Interfaces;

                /// <summary>
                /// Interface for document storage operations.
                /// </summary>
                public interface IStorageProvider
                {
                    Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);

                    Task<Stream> GetAsync(string storagePath, CancellationToken cancellationToken = default);

                    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);

                    Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default);

                    Task<string> GetDownloadUrlAsync(string storagePath, TimeSpan expiration, CancellationToken cancellationToken = default);
                }
                """
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

    private static FileModel CreateDocumentStorageDbContextFile(string directory)
    {
        return new FileModel("DocumentStorageDbContext", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using DocumentStorage.Core.Entities;
                using Microsoft.EntityFrameworkCore;

                namespace DocumentStorage.Infrastructure.Data;

                /// <summary>
                /// Entity Framework Core DbContext for DocumentStorage microservice.
                /// </summary>
                public class DocumentStorageDbContext : DbContext
                {
                    public DocumentStorageDbContext(DbContextOptions<DocumentStorageDbContext> options)
                        : base(options)
                    {
                    }

                    public DbSet<Document> Documents => Set<Document>();

                    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();

                    public DbSet<DocumentMetadata> DocumentMetadata => Set<DocumentMetadata>();

                    public DbSet<DocumentTag> DocumentTags => Set<DocumentTag>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        base.OnModelCreating(modelBuilder);
                        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentStorageDbContext).Assembly);
                    }
                }
                """
        };
    }

    private static FileModel CreateDocumentConfigurationFile(string directory)
    {
        return new FileModel("DocumentConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using DocumentStorage.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace DocumentStorage.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for Document.
                /// </summary>
                public class DocumentConfiguration : IEntityTypeConfiguration<Document>
                {
                    public void Configure(EntityTypeBuilder<Document> builder)
                    {
                        builder.HasKey(d => d.DocumentId);

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

                        builder.HasQueryFilter(d => !d.IsDeleted);
                    }
                }
                """
        };
    }

    private static FileModel CreateDocumentVersionConfigurationFile(string directory)
    {
        return new FileModel("DocumentVersionConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using DocumentStorage.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace DocumentStorage.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for DocumentVersion.
                /// </summary>
                public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
                {
                    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
                    {
                        builder.HasKey(v => v.VersionId);

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
                            .IsUnique();
                    }
                }
                """
        };
    }

    private static FileModel CreateDocumentMetadataConfigurationFile(string directory)
    {
        return new FileModel("DocumentMetadataConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Text.Json;
                using DocumentStorage.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace DocumentStorage.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for DocumentMetadata.
                /// </summary>
                public class DocumentMetadataConfiguration : IEntityTypeConfiguration<DocumentMetadata>
                {
                    public void Configure(EntityTypeBuilder<DocumentMetadata> builder)
                    {
                        builder.HasKey(m => m.MetadataId);

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
                            .OnDelete(DeleteBehavior.Cascade);
                    }
                }
                """
        };
    }

    private static FileModel CreateDocumentTagConfigurationFile(string directory)
    {
        return new FileModel("DocumentTagConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using DocumentStorage.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace DocumentStorage.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for DocumentTag.
                /// </summary>
                public class DocumentTagConfiguration : IEntityTypeConfiguration<DocumentTag>
                {
                    public void Configure(EntityTypeBuilder<DocumentTag> builder)
                    {
                        builder.HasKey(t => t.TagId);

                        builder.Property(t => t.Name)
                            .IsRequired()
                            .HasMaxLength(100);

                        builder.HasOne(t => t.Document)
                            .WithMany(d => d.Tags)
                            .HasForeignKey(t => t.DocumentId)
                            .OnDelete(DeleteBehavior.Cascade);

                        builder.HasIndex(t => new { t.DocumentId, t.Name })
                            .IsUnique();

                        builder.HasIndex(t => t.Name);
                    }
                }
                """
        };
    }

    private static FileModel CreateDocumentRepositoryFile(string directory)
    {
        return new FileModel("DocumentRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using DocumentStorage.Core.Entities;
                using DocumentStorage.Core.Interfaces;
                using DocumentStorage.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace DocumentStorage.Infrastructure.Repositories;

                /// <summary>
                /// Repository implementation for Document entities.
                /// </summary>
                public class DocumentRepository : IDocumentRepository
                {
                    private readonly DocumentStorageDbContext context;

                    public DocumentRepository(DocumentStorageDbContext context)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                    }

                    public async Task<Document?> GetByIdAsync(Guid documentId, CancellationToken cancellationToken = default)
                    {
                        return await context.Documents
                            .Include(d => d.Metadata)
                            .Include(d => d.Tags)
                            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
                    }

                    public async Task<Document?> GetByIdWithVersionsAsync(Guid documentId, CancellationToken cancellationToken = default)
                    {
                        return await context.Documents
                            .Include(d => d.Metadata)
                            .Include(d => d.Tags)
                            .Include(d => d.Versions.OrderByDescending(v => v.VersionNumber))
                            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
                    }

                    public async Task<IEnumerable<Document>> GetAllAsync(CancellationToken cancellationToken = default)
                    {
                        return await context.Documents
                            .Include(d => d.Metadata)
                            .Include(d => d.Tags)
                            .OrderByDescending(d => d.CreatedAt)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<Document>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
                    {
                        return await context.Documents
                            .Include(d => d.Metadata)
                            .Include(d => d.Tags)
                            .Where(d => d.TenantId == tenantId)
                            .OrderByDescending(d => d.CreatedAt)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
                    {
                        document.DocumentId = Guid.NewGuid();
                        document.CreatedAt = DateTime.UtcNow;
                        await context.Documents.AddAsync(document, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return document;
                    }

                    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
                    {
                        document.UpdatedAt = DateTime.UtcNow;
                        context.Documents.Update(document);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid documentId, CancellationToken cancellationToken = default)
                    {
                        var document = await context.Documents.FindAsync(new object[] { documentId }, cancellationToken);
                        if (document != null)
                        {
                            document.IsDeleted = true;
                            document.DeletedAt = DateTime.UtcNow;
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task<DocumentVersion> AddVersionAsync(DocumentVersion version, CancellationToken cancellationToken = default)
                    {
                        version.VersionId = Guid.NewGuid();
                        version.CreatedAt = DateTime.UtcNow;
                        await context.DocumentVersions.AddAsync(version, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return version;
                    }

                    public async Task<IEnumerable<Document>> SearchByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
                    {
                        var tagList = tags.ToList();
                        return await context.Documents
                            .Include(d => d.Metadata)
                            .Include(d => d.Tags)
                            .Where(d => d.Tags.Any(t => tagList.Contains(t.Name)))
                            .OrderByDescending(d => d.CreatedAt)
                            .ToListAsync(cancellationToken);
                    }
                }
                """
        };
    }

    private static FileModel CreateFileSystemStorageProviderFile(string directory)
    {
        return new FileModel("FileSystemStorageProvider", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using DocumentStorage.Core.Interfaces;
                using Microsoft.Extensions.Configuration;
                using Microsoft.Extensions.Logging;

                namespace DocumentStorage.Infrastructure.Services;

                /// <summary>
                /// File system implementation of IStorageProvider.
                /// </summary>
                public class FileSystemStorageProvider : IStorageProvider
                {
                    private readonly string basePath;
                    private readonly ILogger<FileSystemStorageProvider> logger;

                    public FileSystemStorageProvider(IConfiguration configuration, ILogger<FileSystemStorageProvider> logger)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        basePath = configuration["Storage:BasePath"] ?? Path.Combine(Path.GetTempPath(), "DocumentStorage");

                        if (!Directory.Exists(basePath))
                        {
                            Directory.CreateDirectory(basePath);
                        }
                    }

                    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
                    {
                        var storagePath = GenerateStoragePath(fileName);
                        var fullPath = Path.Combine(basePath, storagePath);

                        var directory = Path.GetDirectoryName(fullPath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                        await content.CopyToAsync(fileStream, cancellationToken);

                        logger.LogInformation("Document saved to {StoragePath}", storagePath);
                        return storagePath;
                    }

                    public async Task<Stream> GetAsync(string storagePath, CancellationToken cancellationToken = default)
                    {
                        var fullPath = Path.Combine(basePath, storagePath);

                        if (!File.Exists(fullPath))
                        {
                            throw new FileNotFoundException($"Document not found at path: {storagePath}");
                        }

                        var memoryStream = new MemoryStream();
                        await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                        await fileStream.CopyToAsync(memoryStream, cancellationToken);
                        memoryStream.Position = 0;
                        return memoryStream;
                    }

                    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
                    {
                        var fullPath = Path.Combine(basePath, storagePath);

                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            logger.LogInformation("Document deleted from {StoragePath}", storagePath);
                        }

                        return Task.CompletedTask;
                    }

                    public Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default)
                    {
                        var fullPath = Path.Combine(basePath, storagePath);
                        return Task.FromResult(File.Exists(fullPath));
                    }

                    public Task<string> GetDownloadUrlAsync(string storagePath, TimeSpan expiration, CancellationToken cancellationToken = default)
                    {
                        // For file system storage, return the relative path
                        // In a real implementation, this would generate a signed URL
                        return Task.FromResult($"/api/documents/download/{Uri.EscapeDataString(storagePath)}");
                    }

                    private static string GenerateStoragePath(string fileName)
                    {
                        var date = DateTime.UtcNow;
                        var uniqueId = Guid.NewGuid().ToString("N")[..8];
                        var extension = Path.GetExtension(fileName);
                        var sanitizedName = Path.GetFileNameWithoutExtension(fileName);

                        return Path.Combine(
                            date.Year.ToString(),
                            date.Month.ToString("D2"),
                            date.Day.ToString("D2"),
                            $"{sanitizedName}_{uniqueId}{extension}");
                    }
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

                using DocumentStorage.Core.Interfaces;
                using DocumentStorage.Infrastructure.Data;
                using DocumentStorage.Infrastructure.Repositories;
                using DocumentStorage.Infrastructure.Services;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;

                namespace Microsoft.Extensions.DependencyInjection;

                /// <summary>
                /// Extension methods for configuring DocumentStorage infrastructure services.
                /// </summary>
                public static class ConfigureServices
                {
                    /// <summary>
                    /// Adds DocumentStorage infrastructure services to the service collection.
                    /// </summary>
                    public static IServiceCollection AddDocumentStorageInfrastructure(
                        this IServiceCollection services,
                        IConfiguration configuration)
                    {
                        services.AddDbContext<DocumentStorageDbContext>(options =>
                            options.UseSqlServer(
                                configuration.GetConnectionString("DocumentStorageDb") ??
                                @"Server=.\SQLEXPRESS;Database=DocumentStorageDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IDocumentRepository, DocumentRepository>();
                        services.AddScoped<IStorageProvider, FileSystemStorageProvider>();

                        return services;
                    }
                }
                """
        };
    }

    #endregion

    #region API Layer Files

    private static FileModel CreateDocumentsControllerFile(string directory)
    {
        return new FileModel("DocumentsController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using DocumentStorage.Core.DTOs;
                using DocumentStorage.Core.Entities;
                using DocumentStorage.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace DocumentStorage.Api.Controllers;

                /// <summary>
                /// Controller for document management operations.
                /// </summary>
                [ApiController]
                [Route("api/[controller]")]
                [Authorize]
                public class DocumentsController : ControllerBase
                {
                    private readonly IDocumentRepository documentRepository;
                    private readonly IStorageProvider storageProvider;
                    private readonly ILogger<DocumentsController> logger;

                    public DocumentsController(
                        IDocumentRepository documentRepository,
                        IStorageProvider storageProvider,
                        ILogger<DocumentsController> logger)
                    {
                        this.documentRepository = documentRepository;
                        this.storageProvider = storageProvider;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Upload a new document.
                    /// </summary>
                    [HttpPost]
                    [ProducesResponseType(typeof(UploadDocumentResponse), StatusCodes.Status201Created)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<UploadDocumentResponse>> Upload(
                        IFormFile file,
                        [FromForm] UploadDocumentRequest request,
                        CancellationToken cancellationToken)
                    {
                        if (file == null || file.Length == 0)
                        {
                            return BadRequest(new { error = "No file provided" });
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

                        logger.LogInformation("Document {FileName} uploaded with ID {DocumentId}", file.FileName, createdDocument.DocumentId);

                        var response = new UploadDocumentResponse
                        {
                            DocumentId = createdDocument.DocumentId,
                            FileName = createdDocument.FileName,
                            ContentType = createdDocument.ContentType,
                            FileSizeBytes = createdDocument.FileSizeBytes,
                            CreatedAt = createdDocument.CreatedAt
                        };

                        return CreatedAtAction(nameof(GetById), new { id = createdDocument.DocumentId }, response);
                    }

                    /// <summary>
                    /// Get a document by ID.
                    /// </summary>
                    [HttpGet("{id:guid}")]
                    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<DocumentDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var document = await documentRepository.GetByIdWithVersionsAsync(id, cancellationToken);

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
                        });
                    }

                    /// <summary>
                    /// Delete a document.
                    /// </summary>
                    [HttpDelete("{id:guid}")]
                    [ProducesResponseType(StatusCodes.Status204NoContent)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
                    {
                        var document = await documentRepository.GetByIdAsync(id, cancellationToken);

                        if (document == null)
                        {
                            return NotFound();
                        }

                        await documentRepository.DeleteAsync(id, cancellationToken);
                        logger.LogInformation("Document {DocumentId} deleted", id);

                        return NoContent();
                    }

                    /// <summary>
                    /// Download a document.
                    /// </summary>
                    [HttpGet("{id:guid}/download")]
                    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
                    {
                        var document = await documentRepository.GetByIdAsync(id, cancellationToken);

                        if (document == null)
                        {
                            return NotFound();
                        }

                        var stream = await storageProvider.GetAsync(document.StoragePath, cancellationToken);
                        return File(stream, document.ContentType, document.FileName);
                    }

                    /// <summary>
                    /// Get all documents.
                    /// </summary>
                    [HttpGet]
                    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
                    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAll(CancellationToken cancellationToken)
                    {
                        var documents = await documentRepository.GetAllAsync(cancellationToken);

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

                        return Ok(documentDtos);
                    }
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
