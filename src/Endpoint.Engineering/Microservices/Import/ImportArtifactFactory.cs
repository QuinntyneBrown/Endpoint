// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Import;

public class ImportArtifactFactory : IImportArtifactFactory
{
    private readonly ILogger<ImportArtifactFactory> logger;

    public ImportArtifactFactory(ILogger<ImportArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Import.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("ImportJob", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Import.Core.Entities;

                public class ImportJob
                {
                    public Guid ImportJobId { get; set; }
                    public Guid UserId { get; set; }
                    public Guid? MappingId { get; set; }
                    public required string Name { get; set; }
                    public required string SourceFileName { get; set; }
                    public required string SourceType { get; set; }
                    public ImportJobStatus Status { get; set; } = ImportJobStatus.Pending;
                    public int TotalRecords { get; set; }
                    public int ProcessedRecords { get; set; }
                    public int SuccessfulRecords { get; set; }
                    public int FailedRecords { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? StartedAt { get; set; }
                    public DateTime? CompletedAt { get; set; }
                    public ImportMapping? Mapping { get; set; }
                    public ICollection<ImportError> Errors { get; set; } = new List<ImportError>();
                }

                public enum ImportJobStatus { Pending, Validating, Processing, Completed, Failed, Cancelled }
                """
        });

        project.Files.Add(new FileModel("ImportMapping", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Import.Core.Entities;

                public class ImportMapping
                {
                    public Guid MappingId { get; set; }
                    public required string Name { get; set; }
                    public required string Description { get; set; }
                    public required string SourceType { get; set; }
                    public required string TargetEntity { get; set; }
                    public required string MappingDefinition { get; set; }
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("ImportError", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Import.Core.Entities;

                public class ImportError
                {
                    public Guid ImportErrorId { get; set; }
                    public Guid ImportJobId { get; set; }
                    public ImportJob ImportJob { get; set; } = null!;
                    public int RowNumber { get; set; }
                    public string? ColumnName { get; set; }
                    public required string ErrorCode { get; set; }
                    public required string ErrorMessage { get; set; }
                    public string? OriginalValue { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IImportRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Import.Core.Entities;

                namespace Import.Core.Interfaces;

                public interface IImportRepository
                {
                    Task<ImportJob?> GetByIdAsync(Guid importJobId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ImportJob>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ImportJob>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<ImportJob> AddAsync(ImportJob importJob, CancellationToken cancellationToken = default);
                    Task UpdateAsync(ImportJob importJob, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid importJobId, CancellationToken cancellationToken = default);
                    Task<ImportMapping?> GetMappingByIdAsync(Guid mappingId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ImportMapping>> GetMappingsAsync(CancellationToken cancellationToken = default);
                    Task<ImportMapping> AddMappingAsync(ImportMapping mapping, CancellationToken cancellationToken = default);
                    Task AddErrorAsync(ImportError error, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ImportError>> GetErrorsByJobIdAsync(Guid importJobId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IImportService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Import.Core.DTOs;

                namespace Import.Core.Interfaces;

                public interface IImportService
                {
                    Task<ImportJobDto> UploadAsync(UploadImportRequest request, CancellationToken cancellationToken = default);
                    Task<ImportJobDto?> GetJobByIdAsync(Guid importJobId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ImportJobDto>> GetJobsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
                    Task<ImportMappingDto> CreateMappingAsync(CreateMappingRequest request, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ImportMappingDto>> GetMappingsAsync(CancellationToken cancellationToken = default);
                    Task<IEnumerable<ImportErrorDto>> GetJobErrorsAsync(Guid importJobId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IDataValidator", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Import.Core.Entities;

                namespace Import.Core.Interfaces;

                public interface IDataValidator
                {
                    Task<ValidationResult> ValidateAsync(ImportJob job, Stream dataStream, CancellationToken cancellationToken = default);
                    bool SupportsSourceType(string sourceType);
                }

                public class ValidationResult
                {
                    public bool IsValid { get; set; }
                    public int TotalRecords { get; set; }
                    public ICollection<ImportError> Errors { get; set; } = new List<ImportError>();
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("ImportStartedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Import.Core.Events;

                public sealed class ImportStartedEvent
                {
                    public Guid ImportJobId { get; init; }
                    public Guid UserId { get; init; }
                    public required string SourceFileName { get; init; }
                    public required string SourceType { get; init; }
                    public int TotalRecords { get; init; }
                    public DateTime StartedAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("ImportCompletedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Import.Core.Events;

                public sealed class ImportCompletedEvent
                {
                    public Guid ImportJobId { get; init; }
                    public Guid UserId { get; init; }
                    public int TotalRecords { get; init; }
                    public int SuccessfulRecords { get; init; }
                    public int FailedRecords { get; init; }
                    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("ImportFailedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Import.Core.Events;

                public sealed class ImportFailedEvent
                {
                    public Guid ImportJobId { get; init; }
                    public Guid UserId { get; init; }
                    public required string Reason { get; init; }
                    public int ErrorCount { get; init; }
                    public DateTime FailedAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("ImportJobDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Import.Core.DTOs;

                public sealed class ImportJobDto
                {
                    public Guid ImportJobId { get; init; }
                    public Guid UserId { get; init; }
                    public required string Name { get; init; }
                    public required string SourceFileName { get; init; }
                    public required string SourceType { get; init; }
                    public string Status { get; init; } = "Pending";
                    public int TotalRecords { get; init; }
                    public int ProcessedRecords { get; init; }
                    public int SuccessfulRecords { get; init; }
                    public int FailedRecords { get; init; }
                    public DateTime CreatedAt { get; init; }
                    public DateTime? StartedAt { get; init; }
                    public DateTime? CompletedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("UploadImportRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Import.Core.DTOs;

                public sealed class UploadImportRequest
                {
                    [Required]
                    public Guid UserId { get; init; }

                    [Required]
                    public required string Name { get; init; }

                    [Required]
                    public required string SourceFileName { get; init; }

                    [Required]
                    public required string SourceType { get; init; }

                    public Guid? MappingId { get; init; }

                    public required Stream FileStream { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("ImportMappingDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Import.Core.DTOs;

                public sealed class ImportMappingDto
                {
                    public Guid MappingId { get; init; }
                    public required string Name { get; init; }
                    public required string Description { get; init; }
                    public required string SourceType { get; init; }
                    public required string TargetEntity { get; init; }
                    public bool IsActive { get; init; }
                    public DateTime CreatedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateMappingRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Import.Core.DTOs;

                public sealed class CreateMappingRequest
                {
                    [Required]
                    public required string Name { get; init; }

                    [Required]
                    public required string Description { get; init; }

                    [Required]
                    public required string SourceType { get; init; }

                    [Required]
                    public required string TargetEntity { get; init; }

                    [Required]
                    public required string MappingDefinition { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("ImportErrorDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Import.Core.DTOs;

                public sealed class ImportErrorDto
                {
                    public Guid ImportErrorId { get; init; }
                    public int RowNumber { get; init; }
                    public string? ColumnName { get; init; }
                    public required string ErrorCode { get; init; }
                    public required string ErrorMessage { get; init; }
                    public string? OriginalValue { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Import.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("ImportDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Import.Core.Entities;

                namespace Import.Infrastructure.Data;

                public class ImportDbContext : DbContext
                {
                    public ImportDbContext(DbContextOptions<ImportDbContext> options) : base(options) { }

                    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
                    public DbSet<ImportMapping> ImportMappings => Set<ImportMapping>();
                    public DbSet<ImportError> ImportErrors => Set<ImportError>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<ImportJob>(entity =>
                        {
                            entity.HasKey(e => e.ImportJobId);
                            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                            entity.Property(e => e.SourceFileName).IsRequired().HasMaxLength(500);
                            entity.Property(e => e.SourceType).IsRequired().HasMaxLength(50);
                            entity.HasOne(e => e.Mapping).WithMany().HasForeignKey(e => e.MappingId);
                            entity.HasMany(e => e.Errors).WithOne(e => e.ImportJob).HasForeignKey(e => e.ImportJobId);
                        });

                        modelBuilder.Entity<ImportMapping>(entity =>
                        {
                            entity.HasKey(m => m.MappingId);
                            entity.Property(m => m.Name).IsRequired().HasMaxLength(100);
                            entity.HasIndex(m => m.Name).IsUnique();
                        });

                        modelBuilder.Entity<ImportError>(entity =>
                        {
                            entity.HasKey(e => e.ImportErrorId);
                            entity.Property(e => e.ErrorCode).IsRequired().HasMaxLength(50);
                            entity.Property(e => e.ErrorMessage).IsRequired().HasMaxLength(1000);
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("ImportRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Import.Core.Entities;
                using Import.Core.Interfaces;
                using Import.Infrastructure.Data;

                namespace Import.Infrastructure.Repositories;

                public class ImportRepository : IImportRepository
                {
                    private readonly ImportDbContext context;

                    public ImportRepository(ImportDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<ImportJob?> GetByIdAsync(Guid importJobId, CancellationToken cancellationToken = default)
                        => await context.ImportJobs.Include(e => e.Mapping).Include(e => e.Errors).FirstOrDefaultAsync(e => e.ImportJobId == importJobId, cancellationToken);

                    public async Task<IEnumerable<ImportJob>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
                        => await context.ImportJobs.Include(e => e.Mapping).Where(e => e.UserId == userId).OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);

                    public async Task<IEnumerable<ImportJob>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.ImportJobs.Include(e => e.Mapping).ToListAsync(cancellationToken);

                    public async Task<ImportJob> AddAsync(ImportJob importJob, CancellationToken cancellationToken = default)
                    {
                        importJob.ImportJobId = Guid.NewGuid();
                        await context.ImportJobs.AddAsync(importJob, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return importJob;
                    }

                    public async Task UpdateAsync(ImportJob importJob, CancellationToken cancellationToken = default)
                    {
                        context.ImportJobs.Update(importJob);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid importJobId, CancellationToken cancellationToken = default)
                    {
                        var importJob = await context.ImportJobs.FindAsync(new object[] { importJobId }, cancellationToken);
                        if (importJob != null)
                        {
                            context.ImportJobs.Remove(importJob);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task<ImportMapping?> GetMappingByIdAsync(Guid mappingId, CancellationToken cancellationToken = default)
                        => await context.ImportMappings.FirstOrDefaultAsync(m => m.MappingId == mappingId, cancellationToken);

                    public async Task<IEnumerable<ImportMapping>> GetMappingsAsync(CancellationToken cancellationToken = default)
                        => await context.ImportMappings.Where(m => m.IsActive).ToListAsync(cancellationToken);

                    public async Task<ImportMapping> AddMappingAsync(ImportMapping mapping, CancellationToken cancellationToken = default)
                    {
                        mapping.MappingId = Guid.NewGuid();
                        await context.ImportMappings.AddAsync(mapping, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return mapping;
                    }

                    public async Task AddErrorAsync(ImportError error, CancellationToken cancellationToken = default)
                    {
                        error.ImportErrorId = Guid.NewGuid();
                        await context.ImportErrors.AddAsync(error, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<ImportError>> GetErrorsByJobIdAsync(Guid importJobId, CancellationToken cancellationToken = default)
                        => await context.ImportErrors.Where(e => e.ImportJobId == importJobId).OrderBy(e => e.RowNumber).ToListAsync(cancellationToken);
                }
                """
        });

        project.Files.Add(new FileModel("ImportService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Import.Core.DTOs;
                using Import.Core.Entities;
                using Import.Core.Interfaces;

                namespace Import.Infrastructure.Services;

                public class ImportService : IImportService
                {
                    private readonly IImportRepository repository;
                    private readonly IDataValidator dataValidator;

                    public ImportService(IImportRepository repository, IDataValidator dataValidator)
                    {
                        this.repository = repository;
                        this.dataValidator = dataValidator;
                    }

                    public async Task<ImportJobDto> UploadAsync(UploadImportRequest request, CancellationToken cancellationToken = default)
                    {
                        var importJob = new ImportJob
                        {
                            UserId = request.UserId,
                            Name = request.Name,
                            SourceFileName = request.SourceFileName,
                            SourceType = request.SourceType,
                            MappingId = request.MappingId,
                            Status = ImportJobStatus.Validating,
                            StartedAt = DateTime.UtcNow
                        };

                        var created = await repository.AddAsync(importJob, cancellationToken);

                        // Validate the data
                        try
                        {
                            var validationResult = await dataValidator.ValidateAsync(created, request.FileStream, cancellationToken);
                            created.TotalRecords = validationResult.TotalRecords;

                            if (!validationResult.IsValid)
                            {
                                created.Status = ImportJobStatus.Failed;
                                created.FailedRecords = validationResult.Errors.Count;
                                foreach (var error in validationResult.Errors)
                                {
                                    error.ImportJobId = created.ImportJobId;
                                    await repository.AddErrorAsync(error, cancellationToken);
                                }
                            }
                            else
                            {
                                created.Status = ImportJobStatus.Processing;
                                // Processing would continue asynchronously
                            }

                            await repository.UpdateAsync(created, cancellationToken);
                        }
                        catch
                        {
                            created.Status = ImportJobStatus.Failed;
                            await repository.UpdateAsync(created, cancellationToken);
                            throw;
                        }

                        return new ImportJobDto
                        {
                            ImportJobId = created.ImportJobId,
                            UserId = created.UserId,
                            Name = created.Name,
                            SourceFileName = created.SourceFileName,
                            SourceType = created.SourceType,
                            Status = created.Status.ToString(),
                            TotalRecords = created.TotalRecords,
                            ProcessedRecords = created.ProcessedRecords,
                            SuccessfulRecords = created.SuccessfulRecords,
                            FailedRecords = created.FailedRecords,
                            CreatedAt = created.CreatedAt,
                            StartedAt = created.StartedAt,
                            CompletedAt = created.CompletedAt
                        };
                    }

                    public async Task<ImportJobDto?> GetJobByIdAsync(Guid importJobId, CancellationToken cancellationToken = default)
                    {
                        var importJob = await repository.GetByIdAsync(importJobId, cancellationToken);
                        if (importJob == null) return null;

                        return new ImportJobDto
                        {
                            ImportJobId = importJob.ImportJobId,
                            UserId = importJob.UserId,
                            Name = importJob.Name,
                            SourceFileName = importJob.SourceFileName,
                            SourceType = importJob.SourceType,
                            Status = importJob.Status.ToString(),
                            TotalRecords = importJob.TotalRecords,
                            ProcessedRecords = importJob.ProcessedRecords,
                            SuccessfulRecords = importJob.SuccessfulRecords,
                            FailedRecords = importJob.FailedRecords,
                            CreatedAt = importJob.CreatedAt,
                            StartedAt = importJob.StartedAt,
                            CompletedAt = importJob.CompletedAt
                        };
                    }

                    public async Task<IEnumerable<ImportJobDto>> GetJobsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
                    {
                        var importJobs = await repository.GetByUserIdAsync(userId, cancellationToken);
                        return importJobs.Select(e => new ImportJobDto
                        {
                            ImportJobId = e.ImportJobId,
                            UserId = e.UserId,
                            Name = e.Name,
                            SourceFileName = e.SourceFileName,
                            SourceType = e.SourceType,
                            Status = e.Status.ToString(),
                            TotalRecords = e.TotalRecords,
                            ProcessedRecords = e.ProcessedRecords,
                            SuccessfulRecords = e.SuccessfulRecords,
                            FailedRecords = e.FailedRecords,
                            CreatedAt = e.CreatedAt,
                            StartedAt = e.StartedAt,
                            CompletedAt = e.CompletedAt
                        });
                    }

                    public async Task<ImportMappingDto> CreateMappingAsync(CreateMappingRequest request, CancellationToken cancellationToken = default)
                    {
                        var mapping = new ImportMapping
                        {
                            Name = request.Name,
                            Description = request.Description,
                            SourceType = request.SourceType,
                            TargetEntity = request.TargetEntity,
                            MappingDefinition = request.MappingDefinition
                        };

                        var created = await repository.AddMappingAsync(mapping, cancellationToken);

                        return new ImportMappingDto
                        {
                            MappingId = created.MappingId,
                            Name = created.Name,
                            Description = created.Description,
                            SourceType = created.SourceType,
                            TargetEntity = created.TargetEntity,
                            IsActive = created.IsActive,
                            CreatedAt = created.CreatedAt
                        };
                    }

                    public async Task<IEnumerable<ImportMappingDto>> GetMappingsAsync(CancellationToken cancellationToken = default)
                    {
                        var mappings = await repository.GetMappingsAsync(cancellationToken);
                        return mappings.Select(m => new ImportMappingDto
                        {
                            MappingId = m.MappingId,
                            Name = m.Name,
                            Description = m.Description,
                            SourceType = m.SourceType,
                            TargetEntity = m.TargetEntity,
                            IsActive = m.IsActive,
                            CreatedAt = m.CreatedAt
                        });
                    }

                    public async Task<IEnumerable<ImportErrorDto>> GetJobErrorsAsync(Guid importJobId, CancellationToken cancellationToken = default)
                    {
                        var errors = await repository.GetErrorsByJobIdAsync(importJobId, cancellationToken);
                        return errors.Select(e => new ImportErrorDto
                        {
                            ImportErrorId = e.ImportErrorId,
                            RowNumber = e.RowNumber,
                            ColumnName = e.ColumnName,
                            ErrorCode = e.ErrorCode,
                            ErrorMessage = e.ErrorMessage,
                            OriginalValue = e.OriginalValue
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("DefaultDataValidator", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Import.Core.Entities;
                using Import.Core.Interfaces;

                namespace Import.Infrastructure.Services;

                public class DefaultDataValidator : IDataValidator
                {
                    private static readonly HashSet<string> SupportedSourceTypes = new(StringComparer.OrdinalIgnoreCase) { "csv", "xlsx", "json", "xml" };

                    public async Task<ValidationResult> ValidateAsync(ImportJob job, Stream dataStream, CancellationToken cancellationToken = default)
                    {
                        // Simulate validation
                        await Task.Delay(100, cancellationToken);

                        var result = new ValidationResult
                        {
                            IsValid = true,
                            TotalRecords = 0,
                            Errors = new List<ImportError>()
                        };

                        // Basic validation - in production this would parse the file and validate each record
                        if (dataStream.Length == 0)
                        {
                            result.IsValid = false;
                            result.Errors.Add(new ImportError
                            {
                                ImportJobId = job.ImportJobId,
                                RowNumber = 0,
                                ErrorCode = "EMPTY_FILE",
                                ErrorMessage = "The uploaded file is empty"
                            });
                        }

                        return result;
                    }

                    public bool SupportsSourceType(string sourceType) => SupportedSourceTypes.Contains(sourceType);
                }
                """
        });

        project.Files.Add(new FileModel("ConfigureServices", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;
                using Import.Core.Interfaces;
                using Import.Infrastructure.Data;
                using Import.Infrastructure.Repositories;
                using Import.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddImportInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<ImportDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("ImportDb") ??
                                @"Server=.\SQLEXPRESS;Database=ImportDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IImportRepository, ImportRepository>();
                        services.AddScoped<IImportService, ImportService>();
                        services.AddScoped<IDataValidator, DefaultDataValidator>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Import.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("ImportController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Import.Core.DTOs;
                using Import.Core.Interfaces;

                namespace Import.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class ImportController : ControllerBase
                {
                    private readonly IImportService service;

                    public ImportController(IImportService service)
                    {
                        this.service = service;
                    }

                    [HttpPost("upload")]
                    public async Task<ActionResult<ImportJobDto>> Upload([FromForm] Guid userId, [FromForm] string name, [FromForm] string sourceType, [FromForm] Guid? mappingId, IFormFile file, CancellationToken cancellationToken)
                    {
                        if (file == null || file.Length == 0)
                            return BadRequest("No file uploaded");

                        using var stream = file.OpenReadStream();
                        var request = new UploadImportRequest
                        {
                            UserId = userId,
                            Name = name,
                            SourceFileName = file.FileName,
                            SourceType = sourceType,
                            MappingId = mappingId,
                            FileStream = stream
                        };

                        var importJob = await service.UploadAsync(request, cancellationToken);
                        return CreatedAtAction(nameof(GetJob), new { id = importJob.ImportJobId }, importJob);
                    }

                    [HttpGet("jobs/{id:guid}")]
                    public async Task<ActionResult<ImportJobDto>> GetJob(Guid id, CancellationToken cancellationToken)
                    {
                        var importJob = await service.GetJobByIdAsync(id, cancellationToken);
                        if (importJob == null) return NotFound();
                        return Ok(importJob);
                    }

                    [HttpGet("jobs/{id:guid}/errors")]
                    public async Task<ActionResult<IEnumerable<ImportErrorDto>>> GetJobErrors(Guid id, CancellationToken cancellationToken)
                    {
                        var errors = await service.GetJobErrorsAsync(id, cancellationToken);
                        return Ok(errors);
                    }

                    [HttpPost("mappings")]
                    public async Task<ActionResult<ImportMappingDto>> CreateMapping([FromBody] CreateMappingRequest request, CancellationToken cancellationToken)
                    {
                        var mapping = await service.CreateMappingAsync(request, cancellationToken);
                        return CreatedAtAction(nameof(GetMappings), mapping);
                    }

                    [HttpGet("mappings")]
                    public async Task<ActionResult<IEnumerable<ImportMappingDto>>> GetMappings(CancellationToken cancellationToken)
                    {
                        var mappings = await service.GetMappingsAsync(cancellationToken);
                        return Ok(mappings);
                    }
                }
                """
        });

        project.Files.Add(new FileModel("Program", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddImportInfrastructure(builder.Configuration);
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
        });

        project.Files.Add(new FileModel("appsettings", project.Directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "ImportDb": "Server=.\\SQLEXPRESS;Database=ImportDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information"
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        });
    }
}
