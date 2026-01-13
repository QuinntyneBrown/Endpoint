// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Media;

/// <summary>
/// Factory for creating Media microservice artifacts according to media-microservice.spec.md.
/// </summary>
public class MediaArtifactFactory : IMediaArtifactFactory
{
    private readonly ILogger<MediaArtifactFactory> logger;

    public MediaArtifactFactory(ILogger<MediaArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Media.Core files");

        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateIAggregateRootFile(entitiesDir));
        project.Files.Add(CreateMediaFileEntityFile(entitiesDir));
        project.Files.Add(CreateThumbnailEntityFile(entitiesDir));
        project.Files.Add(CreateTranscodingJobEntityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIDomainEventFile(interfacesDir));
        project.Files.Add(CreateIMediaRepositoryFile(interfacesDir));
        project.Files.Add(CreateIImageProcessorFile(interfacesDir));
        project.Files.Add(CreateIVideoTranscoderFile(interfacesDir));

        // Events
        project.Files.Add(CreateMediaUploadedEventFile(eventsDir));
        project.Files.Add(CreateThumbnailGeneratedEventFile(eventsDir));
        project.Files.Add(CreateTranscodingCompletedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateMediaFileDtoFile(dtosDir));
        project.Files.Add(CreateThumbnailDtoFile(dtosDir));
        project.Files.Add(CreateUploadRequestFile(dtosDir));
        project.Files.Add(CreateUploadResponseFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Media.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // DbContext
        project.Files.Add(CreateMediaDbContextFile(dataDir));

        // Entity Configurations
        project.Files.Add(CreateMediaFileConfigurationFile(configurationsDir));
        project.Files.Add(CreateThumbnailConfigurationFile(configurationsDir));
        project.Files.Add(CreateTranscodingJobConfigurationFile(configurationsDir));

        // Repositories
        project.Files.Add(CreateMediaRepositoryFile(repositoriesDir));

        // Services
        project.Files.Add(CreateImageProcessorFile(servicesDir));
        project.Files.Add(CreateVideoTranscoderFile(servicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Media.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Controllers
        project.Files.Add(CreateMediaControllerFile(controllersDir));

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

                namespace Media.Core.Entities;

                /// <summary>
                /// Marker interface for aggregate roots.
                /// </summary>
                public interface IAggregateRoot
                {
                }
                """
        };
    }

    private static FileModel CreateMediaFileEntityFile(string directory)
    {
        return new FileModel("MediaFile", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Media.Core.Entities;

                /// <summary>
                /// Entity representing a media file (image, video, audio, etc.).
                /// </summary>
                public class MediaFile : IAggregateRoot
                {
                    public Guid MediaFileId { get; set; }

                    public required string FileName { get; set; }

                    public required string ContentType { get; set; }

                    public long FileSize { get; set; }

                    public required string StoragePath { get; set; }

                    public string? OriginalFileName { get; set; }

                    public string? Description { get; set; }

                    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

                    public Guid? UploadedBy { get; set; }

                    public bool IsProcessed { get; set; }

                    public DateTime? ProcessedAt { get; set; }

                    public ICollection<Thumbnail> Thumbnails { get; set; } = new List<Thumbnail>();

                    public ICollection<TranscodingJob> TranscodingJobs { get; set; } = new List<TranscodingJob>();
                }
                """
        };
    }

    private static FileModel CreateThumbnailEntityFile(string directory)
    {
        return new FileModel("Thumbnail", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Media.Core.Entities;

                /// <summary>
                /// Entity representing a thumbnail image for a media file.
                /// </summary>
                public class Thumbnail
                {
                    public Guid ThumbnailId { get; set; }

                    public Guid MediaFileId { get; set; }

                    public MediaFile MediaFile { get; set; } = null!;

                    public required string StoragePath { get; set; }

                    public int Width { get; set; }

                    public int Height { get; set; }

                    public required string Format { get; set; }

                    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
                }
                """
        };
    }

    private static FileModel CreateTranscodingJobEntityFile(string directory)
    {
        return new FileModel("TranscodingJob", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Media.Core.Entities;

                /// <summary>
                /// Entity representing a video transcoding job.
                /// </summary>
                public class TranscodingJob
                {
                    public Guid TranscodingJobId { get; set; }

                    public Guid MediaFileId { get; set; }

                    public MediaFile MediaFile { get; set; } = null!;

                    public required string TargetFormat { get; set; }

                    public required string TargetResolution { get; set; }

                    public TranscodingStatus Status { get; set; } = TranscodingStatus.Pending;

                    public string? OutputPath { get; set; }

                    public int Progress { get; set; }

                    public string? ErrorMessage { get; set; }

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? StartedAt { get; set; }

                    public DateTime? CompletedAt { get; set; }
                }

                /// <summary>
                /// Status of a transcoding job.
                /// </summary>
                public enum TranscodingStatus
                {
                    Pending,
                    InProgress,
                    Completed,
                    Failed,
                    Cancelled
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

                namespace Media.Core.Interfaces;

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

    private static FileModel CreateIMediaRepositoryFile(string directory)
    {
        return new FileModel("IMediaRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Entities;

                namespace Media.Core.Interfaces;

                /// <summary>
                /// Repository interface for MediaFile entities.
                /// </summary>
                public interface IMediaRepository
                {
                    Task<MediaFile?> GetByIdAsync(Guid mediaFileId, CancellationToken cancellationToken = default);

                    Task<MediaFile?> GetByIdWithThumbnailsAsync(Guid mediaFileId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<MediaFile>> GetAllAsync(CancellationToken cancellationToken = default);

                    Task<IEnumerable<MediaFile>> GetByUploaderAsync(Guid uploaderId, CancellationToken cancellationToken = default);

                    Task<MediaFile> AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default);

                    Task UpdateAsync(MediaFile mediaFile, CancellationToken cancellationToken = default);

                    Task DeleteAsync(Guid mediaFileId, CancellationToken cancellationToken = default);

                    Task<Thumbnail> AddThumbnailAsync(Thumbnail thumbnail, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Thumbnail>> GetThumbnailsAsync(Guid mediaFileId, CancellationToken cancellationToken = default);

                    Task<TranscodingJob> AddTranscodingJobAsync(TranscodingJob job, CancellationToken cancellationToken = default);

                    Task UpdateTranscodingJobAsync(TranscodingJob job, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateIImageProcessorFile(string directory)
    {
        return new FileModel("IImageProcessor", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Media.Core.Interfaces;

                /// <summary>
                /// Service interface for image processing operations.
                /// </summary>
                public interface IImageProcessor
                {
                    Task<string> GenerateThumbnailAsync(string sourcePath, string outputPath, int width, int height, CancellationToken cancellationToken = default);

                    Task<string> ResizeImageAsync(string sourcePath, string outputPath, int maxWidth, int maxHeight, CancellationToken cancellationToken = default);

                    Task<(int Width, int Height)> GetImageDimensionsAsync(string imagePath, CancellationToken cancellationToken = default);

                    Task<string> ConvertFormatAsync(string sourcePath, string outputPath, string targetFormat, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateIVideoTranscoderFile(string directory)
    {
        return new FileModel("IVideoTranscoder", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Entities;

                namespace Media.Core.Interfaces;

                /// <summary>
                /// Service interface for video transcoding operations.
                /// </summary>
                public interface IVideoTranscoder
                {
                    Task<string> TranscodeAsync(string sourcePath, string outputPath, string targetFormat, string resolution, IProgress<int>? progress = null, CancellationToken cancellationToken = default);

                    Task<string> ExtractThumbnailAsync(string videoPath, string outputPath, TimeSpan timestamp, CancellationToken cancellationToken = default);

                    Task<TimeSpan> GetDurationAsync(string videoPath, CancellationToken cancellationToken = default);

                    Task<(int Width, int Height)> GetResolutionAsync(string videoPath, CancellationToken cancellationToken = default);

                    bool IsFormatSupported(string format);
                }
                """
        };
    }

    private static FileModel CreateMediaUploadedEventFile(string directory)
    {
        return new FileModel("MediaUploadedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Interfaces;

                namespace Media.Core.Events;

                /// <summary>
                /// Event raised when a media file is uploaded.
                /// </summary>
                public sealed class MediaUploadedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "MediaFile";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string FileName { get; init; }

                    public required string ContentType { get; init; }

                    public long FileSize { get; init; }

                    public Guid? UploadedBy { get; init; }
                }
                """
        };
    }

    private static FileModel CreateThumbnailGeneratedEventFile(string directory)
    {
        return new FileModel("ThumbnailGeneratedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Interfaces;

                namespace Media.Core.Events;

                /// <summary>
                /// Event raised when a thumbnail is generated for a media file.
                /// </summary>
                public sealed class ThumbnailGeneratedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "MediaFile";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public Guid ThumbnailId { get; init; }

                    public int Width { get; init; }

                    public int Height { get; init; }

                    public required string Format { get; init; }
                }
                """
        };
    }

    private static FileModel CreateTranscodingCompletedEventFile(string directory)
    {
        return new FileModel("TranscodingCompletedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Interfaces;

                namespace Media.Core.Events;

                /// <summary>
                /// Event raised when a video transcoding job is completed.
                /// </summary>
                public sealed class TranscodingCompletedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "MediaFile";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public Guid TranscodingJobId { get; init; }

                    public required string TargetFormat { get; init; }

                    public required string TargetResolution { get; init; }

                    public required string OutputPath { get; init; }

                    public TimeSpan Duration { get; init; }
                }
                """
        };
    }

    private static FileModel CreateMediaFileDtoFile(string directory)
    {
        return new FileModel("MediaFileDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Media.Core.DTOs;

                /// <summary>
                /// Data transfer object for MediaFile.
                /// </summary>
                public sealed class MediaFileDto
                {
                    public Guid MediaFileId { get; init; }

                    public required string FileName { get; init; }

                    public required string ContentType { get; init; }

                    public long FileSize { get; init; }

                    public string? OriginalFileName { get; init; }

                    public string? Description { get; init; }

                    public DateTime UploadedAt { get; init; }

                    public Guid? UploadedBy { get; init; }

                    public bool IsProcessed { get; init; }

                    public IReadOnlyList<ThumbnailDto> Thumbnails { get; init; } = Array.Empty<ThumbnailDto>();
                }
                """
        };
    }

    private static FileModel CreateThumbnailDtoFile(string directory)
    {
        return new FileModel("ThumbnailDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Media.Core.DTOs;

                /// <summary>
                /// Data transfer object for Thumbnail.
                /// </summary>
                public sealed class ThumbnailDto
                {
                    public Guid ThumbnailId { get; init; }

                    public int Width { get; init; }

                    public int Height { get; init; }

                    public required string Format { get; init; }

                    public required string Url { get; init; }
                }
                """
        };
    }

    private static FileModel CreateUploadRequestFile(string directory)
    {
        return new FileModel("UploadRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Media.Core.DTOs;

                /// <summary>
                /// Request model for media file upload.
                /// </summary>
                public sealed class UploadRequest
                {
                    public string? Description { get; init; }

                    public bool GenerateThumbnail { get; init; } = true;

                    public bool Transcode { get; init; }

                    public string? TargetFormat { get; init; }

                    public string? TargetResolution { get; init; }
                }
                """
        };
    }

    private static FileModel CreateUploadResponseFile(string directory)
    {
        return new FileModel("UploadResponse", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Media.Core.DTOs;

                /// <summary>
                /// Response model for successful media upload.
                /// </summary>
                public sealed class UploadResponse
                {
                    public Guid MediaFileId { get; init; }

                    public required string FileName { get; init; }

                    public required string ContentType { get; init; }

                    public long FileSize { get; init; }

                    public required string Url { get; init; }
                }
                """
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static FileModel CreateMediaDbContextFile(string directory)
    {
        return new FileModel("MediaDbContext", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Entities;
                using Microsoft.EntityFrameworkCore;

                namespace Media.Infrastructure.Data;

                /// <summary>
                /// Entity Framework Core DbContext for Media microservice.
                /// </summary>
                public class MediaDbContext : DbContext
                {
                    public MediaDbContext(DbContextOptions<MediaDbContext> options)
                        : base(options)
                    {
                    }

                    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

                    public DbSet<Thumbnail> Thumbnails => Set<Thumbnail>();

                    public DbSet<TranscodingJob> TranscodingJobs => Set<TranscodingJob>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        base.OnModelCreating(modelBuilder);
                        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediaDbContext).Assembly);
                    }
                }
                """
        };
    }

    private static FileModel CreateMediaFileConfigurationFile(string directory)
    {
        return new FileModel("MediaFileConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Media.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for MediaFile.
                /// </summary>
                public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
                {
                    public void Configure(EntityTypeBuilder<MediaFile> builder)
                    {
                        builder.HasKey(m => m.MediaFileId);

                        builder.Property(m => m.FileName)
                            .IsRequired()
                            .HasMaxLength(255);

                        builder.Property(m => m.ContentType)
                            .IsRequired()
                            .HasMaxLength(100);

                        builder.Property(m => m.StoragePath)
                            .IsRequired()
                            .HasMaxLength(1000);

                        builder.Property(m => m.OriginalFileName)
                            .HasMaxLength(255);

                        builder.Property(m => m.Description)
                            .HasMaxLength(1000);

                        builder.Property(m => m.UploadedAt)
                            .IsRequired();

                        builder.HasIndex(m => m.UploadedBy);
                        builder.HasIndex(m => m.ContentType);
                    }
                }
                """
        };
    }

    private static FileModel CreateThumbnailConfigurationFile(string directory)
    {
        return new FileModel("ThumbnailConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Media.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for Thumbnail.
                /// </summary>
                public class ThumbnailConfiguration : IEntityTypeConfiguration<Thumbnail>
                {
                    public void Configure(EntityTypeBuilder<Thumbnail> builder)
                    {
                        builder.HasKey(t => t.ThumbnailId);

                        builder.Property(t => t.StoragePath)
                            .IsRequired()
                            .HasMaxLength(1000);

                        builder.Property(t => t.Format)
                            .IsRequired()
                            .HasMaxLength(20);

                        builder.HasOne(t => t.MediaFile)
                            .WithMany(m => m.Thumbnails)
                            .HasForeignKey(t => t.MediaFileId)
                            .OnDelete(DeleteBehavior.Cascade);

                        builder.HasIndex(t => t.MediaFileId);
                    }
                }
                """
        };
    }

    private static FileModel CreateTranscodingJobConfigurationFile(string directory)
    {
        return new FileModel("TranscodingJobConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Media.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for TranscodingJob.
                /// </summary>
                public class TranscodingJobConfiguration : IEntityTypeConfiguration<TranscodingJob>
                {
                    public void Configure(EntityTypeBuilder<TranscodingJob> builder)
                    {
                        builder.HasKey(j => j.TranscodingJobId);

                        builder.Property(j => j.TargetFormat)
                            .IsRequired()
                            .HasMaxLength(20);

                        builder.Property(j => j.TargetResolution)
                            .IsRequired()
                            .HasMaxLength(20);

                        builder.Property(j => j.OutputPath)
                            .HasMaxLength(1000);

                        builder.Property(j => j.ErrorMessage)
                            .HasMaxLength(2000);

                        builder.HasOne(j => j.MediaFile)
                            .WithMany(m => m.TranscodingJobs)
                            .HasForeignKey(j => j.MediaFileId)
                            .OnDelete(DeleteBehavior.Cascade);

                        builder.HasIndex(j => j.MediaFileId);
                        builder.HasIndex(j => j.Status);
                    }
                }
                """
        };
    }

    private static FileModel CreateMediaRepositoryFile(string directory)
    {
        return new FileModel("MediaRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Entities;
                using Media.Core.Interfaces;
                using Media.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace Media.Infrastructure.Repositories;

                /// <summary>
                /// Repository implementation for MediaFile entities.
                /// </summary>
                public class MediaRepository : IMediaRepository
                {
                    private readonly MediaDbContext context;

                    public MediaRepository(MediaDbContext context)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                    }

                    public async Task<MediaFile?> GetByIdAsync(Guid mediaFileId, CancellationToken cancellationToken = default)
                    {
                        return await context.MediaFiles
                            .FirstOrDefaultAsync(m => m.MediaFileId == mediaFileId, cancellationToken);
                    }

                    public async Task<MediaFile?> GetByIdWithThumbnailsAsync(Guid mediaFileId, CancellationToken cancellationToken = default)
                    {
                        return await context.MediaFiles
                            .Include(m => m.Thumbnails)
                            .FirstOrDefaultAsync(m => m.MediaFileId == mediaFileId, cancellationToken);
                    }

                    public async Task<IEnumerable<MediaFile>> GetAllAsync(CancellationToken cancellationToken = default)
                    {
                        return await context.MediaFiles
                            .Include(m => m.Thumbnails)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<MediaFile>> GetByUploaderAsync(Guid uploaderId, CancellationToken cancellationToken = default)
                    {
                        return await context.MediaFiles
                            .Include(m => m.Thumbnails)
                            .Where(m => m.UploadedBy == uploaderId)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<MediaFile> AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
                    {
                        mediaFile.MediaFileId = Guid.NewGuid();
                        mediaFile.UploadedAt = DateTime.UtcNow;
                        await context.MediaFiles.AddAsync(mediaFile, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return mediaFile;
                    }

                    public async Task UpdateAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
                    {
                        context.MediaFiles.Update(mediaFile);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid mediaFileId, CancellationToken cancellationToken = default)
                    {
                        var mediaFile = await context.MediaFiles.FindAsync(new object[] { mediaFileId }, cancellationToken);
                        if (mediaFile != null)
                        {
                            context.MediaFiles.Remove(mediaFile);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task<Thumbnail> AddThumbnailAsync(Thumbnail thumbnail, CancellationToken cancellationToken = default)
                    {
                        thumbnail.ThumbnailId = Guid.NewGuid();
                        thumbnail.GeneratedAt = DateTime.UtcNow;
                        await context.Thumbnails.AddAsync(thumbnail, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return thumbnail;
                    }

                    public async Task<IEnumerable<Thumbnail>> GetThumbnailsAsync(Guid mediaFileId, CancellationToken cancellationToken = default)
                    {
                        return await context.Thumbnails
                            .Where(t => t.MediaFileId == mediaFileId)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<TranscodingJob> AddTranscodingJobAsync(TranscodingJob job, CancellationToken cancellationToken = default)
                    {
                        job.TranscodingJobId = Guid.NewGuid();
                        job.CreatedAt = DateTime.UtcNow;
                        await context.TranscodingJobs.AddAsync(job, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return job;
                    }

                    public async Task UpdateTranscodingJobAsync(TranscodingJob job, CancellationToken cancellationToken = default)
                    {
                        context.TranscodingJobs.Update(job);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }
                """
        };
    }

    private static FileModel CreateImageProcessorFile(string directory)
    {
        return new FileModel("ImageProcessor", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Interfaces;
                using Microsoft.Extensions.Logging;

                namespace Media.Infrastructure.Services;

                /// <summary>
                /// Image processing service implementation.
                /// </summary>
                public class ImageProcessor : IImageProcessor
                {
                    private readonly ILogger<ImageProcessor> logger;

                    public ImageProcessor(ILogger<ImageProcessor> logger)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    public Task<string> GenerateThumbnailAsync(string sourcePath, string outputPath, int width, int height, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Generating thumbnail for {SourcePath} at {Width}x{Height}", sourcePath, width, height);

                        // Implementation would use an image processing library like ImageSharp or SkiaSharp
                        // This is a placeholder that would be replaced with actual implementation

                        return Task.FromResult(outputPath);
                    }

                    public Task<string> ResizeImageAsync(string sourcePath, string outputPath, int maxWidth, int maxHeight, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Resizing image {SourcePath} to max {MaxWidth}x{MaxHeight}", sourcePath, maxWidth, maxHeight);

                        // Implementation would use an image processing library
                        return Task.FromResult(outputPath);
                    }

                    public Task<(int Width, int Height)> GetImageDimensionsAsync(string imagePath, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Getting dimensions for {ImagePath}", imagePath);

                        // Implementation would read image metadata
                        return Task.FromResult((1920, 1080));
                    }

                    public Task<string> ConvertFormatAsync(string sourcePath, string outputPath, string targetFormat, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Converting {SourcePath} to {TargetFormat}", sourcePath, targetFormat);

                        // Implementation would convert image format
                        return Task.FromResult(outputPath);
                    }
                }
                """
        };
    }

    private static FileModel CreateVideoTranscoderFile(string directory)
    {
        return new FileModel("VideoTranscoder", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.Interfaces;
                using Microsoft.Extensions.Logging;

                namespace Media.Infrastructure.Services;

                /// <summary>
                /// Video transcoding service implementation.
                /// </summary>
                public class VideoTranscoder : IVideoTranscoder
                {
                    private readonly ILogger<VideoTranscoder> logger;
                    private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase)
                    {
                        "mp4", "webm", "mkv", "avi", "mov", "wmv", "flv"
                    };

                    public VideoTranscoder(ILogger<VideoTranscoder> logger)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    public Task<string> TranscodeAsync(string sourcePath, string outputPath, string targetFormat, string resolution, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Transcoding {SourcePath} to {TargetFormat} at {Resolution}", sourcePath, targetFormat, resolution);

                        // Implementation would use FFmpeg or similar library
                        // This is a placeholder that would be replaced with actual implementation

                        progress?.Report(100);
                        return Task.FromResult(outputPath);
                    }

                    public Task<string> ExtractThumbnailAsync(string videoPath, string outputPath, TimeSpan timestamp, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Extracting thumbnail from {VideoPath} at {Timestamp}", videoPath, timestamp);

                        // Implementation would use FFmpeg to extract frame
                        return Task.FromResult(outputPath);
                    }

                    public Task<TimeSpan> GetDurationAsync(string videoPath, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Getting duration for {VideoPath}", videoPath);

                        // Implementation would read video metadata
                        return Task.FromResult(TimeSpan.FromMinutes(5));
                    }

                    public Task<(int Width, int Height)> GetResolutionAsync(string videoPath, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Getting resolution for {VideoPath}", videoPath);

                        // Implementation would read video metadata
                        return Task.FromResult((1920, 1080));
                    }

                    public bool IsFormatSupported(string format)
                    {
                        return SupportedFormats.Contains(format);
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

                using Media.Core.Interfaces;
                using Media.Infrastructure.Data;
                using Media.Infrastructure.Repositories;
                using Media.Infrastructure.Services;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;

                namespace Microsoft.Extensions.DependencyInjection;

                /// <summary>
                /// Extension methods for configuring Media infrastructure services.
                /// </summary>
                public static class ConfigureServices
                {
                    /// <summary>
                    /// Adds Media infrastructure services to the service collection.
                    /// </summary>
                    public static IServiceCollection AddMediaInfrastructure(
                        this IServiceCollection services,
                        IConfiguration configuration)
                    {
                        services.AddDbContext<MediaDbContext>(options =>
                            options.UseSqlServer(
                                configuration.GetConnectionString("MediaDb") ??
                                @"Server=.\SQLEXPRESS;Database=MediaDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IMediaRepository, MediaRepository>();
                        services.AddScoped<IImageProcessor, ImageProcessor>();
                        services.AddScoped<IVideoTranscoder, VideoTranscoder>();

                        return services;
                    }
                }
                """
        };
    }

    #endregion

    #region API Layer Files

    private static FileModel CreateMediaControllerFile(string directory)
    {
        return new FileModel("MediaController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Media.Core.DTOs;
                using Media.Core.Entities;
                using Media.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace Media.Api.Controllers;

                /// <summary>
                /// Controller for media file operations.
                /// </summary>
                [ApiController]
                [Route("api/[controller]")]
                public class MediaController : ControllerBase
                {
                    private readonly IMediaRepository mediaRepository;
                    private readonly IImageProcessor imageProcessor;
                    private readonly IVideoTranscoder videoTranscoder;
                    private readonly ILogger<MediaController> logger;

                    public MediaController(
                        IMediaRepository mediaRepository,
                        IImageProcessor imageProcessor,
                        IVideoTranscoder videoTranscoder,
                        ILogger<MediaController> logger)
                    {
                        this.mediaRepository = mediaRepository;
                        this.imageProcessor = imageProcessor;
                        this.videoTranscoder = videoTranscoder;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Upload a media file.
                    /// </summary>
                    [HttpPost("upload")]
                    [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status201Created)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<UploadResponse>> Upload(
                        IFormFile file,
                        [FromForm] UploadRequest? request,
                        CancellationToken cancellationToken)
                    {
                        if (file == null || file.Length == 0)
                        {
                            return BadRequest(new { error = "No file provided" });
                        }

                        // Generate unique file name and storage path
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var storagePath = Path.Combine("uploads", DateTime.UtcNow.ToString("yyyy/MM/dd"), fileName);

                        // Create the directory if it doesn't exist
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", storagePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                        // Save the file
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream, cancellationToken);
                        }

                        var mediaFile = new MediaFile
                        {
                            FileName = fileName,
                            ContentType = file.ContentType,
                            FileSize = file.Length,
                            StoragePath = storagePath,
                            OriginalFileName = file.FileName,
                            Description = request?.Description
                        };

                        var createdMedia = await mediaRepository.AddAsync(mediaFile, cancellationToken);

                        logger.LogInformation("Media file {FileName} uploaded successfully", fileName);

                        var response = new UploadResponse
                        {
                            MediaFileId = createdMedia.MediaFileId,
                            FileName = createdMedia.FileName,
                            ContentType = createdMedia.ContentType,
                            FileSize = createdMedia.FileSize,
                            Url = $"/uploads/{storagePath}"
                        };

                        return CreatedAtAction(nameof(GetById), new { id = createdMedia.MediaFileId }, response);
                    }

                    /// <summary>
                    /// Get a media file by ID.
                    /// </summary>
                    [HttpGet("{id:guid}")]
                    [ProducesResponseType(typeof(MediaFileDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<MediaFileDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var mediaFile = await mediaRepository.GetByIdWithThumbnailsAsync(id, cancellationToken);

                        if (mediaFile == null)
                        {
                            return NotFound();
                        }

                        return Ok(new MediaFileDto
                        {
                            MediaFileId = mediaFile.MediaFileId,
                            FileName = mediaFile.FileName,
                            ContentType = mediaFile.ContentType,
                            FileSize = mediaFile.FileSize,
                            OriginalFileName = mediaFile.OriginalFileName,
                            Description = mediaFile.Description,
                            UploadedAt = mediaFile.UploadedAt,
                            UploadedBy = mediaFile.UploadedBy,
                            IsProcessed = mediaFile.IsProcessed,
                            Thumbnails = mediaFile.Thumbnails.Select(t => new ThumbnailDto
                            {
                                ThumbnailId = t.ThumbnailId,
                                Width = t.Width,
                                Height = t.Height,
                                Format = t.Format,
                                Url = $"/uploads/{t.StoragePath}"
                            }).ToList()
                        });
                    }

                    /// <summary>
                    /// Get thumbnail for a media file.
                    /// </summary>
                    [HttpGet("{id:guid}/thumbnail")]
                    [ProducesResponseType(typeof(ThumbnailDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<ThumbnailDto>> GetThumbnail(Guid id, CancellationToken cancellationToken)
                    {
                        var thumbnails = await mediaRepository.GetThumbnailsAsync(id, cancellationToken);
                        var thumbnail = thumbnails.FirstOrDefault();

                        if (thumbnail == null)
                        {
                            return NotFound(new { error = "No thumbnail found for this media file" });
                        }

                        return Ok(new ThumbnailDto
                        {
                            ThumbnailId = thumbnail.ThumbnailId,
                            Width = thumbnail.Width,
                            Height = thumbnail.Height,
                            Format = thumbnail.Format,
                            Url = $"/uploads/{thumbnail.StoragePath}"
                        });
                    }

                    /// <summary>
                    /// Delete a media file.
                    /// </summary>
                    [HttpDelete("{id:guid}")]
                    [ProducesResponseType(StatusCodes.Status204NoContent)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
                    {
                        var mediaFile = await mediaRepository.GetByIdAsync(id, cancellationToken);

                        if (mediaFile == null)
                        {
                            return NotFound();
                        }

                        // Delete physical file
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", mediaFile.StoragePath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }

                        await mediaRepository.DeleteAsync(id, cancellationToken);
                        logger.LogInformation("Media file {MediaFileId} deleted", id);

                        return NoContent();
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
                builder.Services.AddMediaInfrastructure(builder.Configuration);

                builder.Services.AddControllers();

                // Configure Swagger
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Media API",
                        Version = "v1",
                        Description = "Media microservice for file upload and processing"
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
                app.UseStaticFiles();
                app.UseCors();

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
                    "MediaDb": "Server=.\\SQLEXPRESS;Database=MediaDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Storage": {
                    "BasePath": "wwwroot/uploads",
                    "MaxFileSize": 104857600,
                    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".mp4", ".webm", ".mov", ".avi"]
                  },
                  "Thumbnail": {
                    "DefaultWidth": 150,
                    "DefaultHeight": 150,
                    "Format": "jpg"
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
