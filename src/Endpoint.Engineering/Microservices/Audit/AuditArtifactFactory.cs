// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Audit;

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

    private static FileModel CreateIAggregateRootFile(string directory)
    {
        return new FileModel("IAggregateRoot", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Audit.Core.Entities;

                /// <summary>
                /// Marker interface for aggregate roots.
                /// </summary>
                public interface IAggregateRoot
                {
                }
                """
        };
    }

    private static FileModel CreateAuditEntryEntityFile(string directory)
    {
        return new FileModel("AuditEntry", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Audit.Core.Entities;

                /// <summary>
                /// AuditEntry entity representing a tracked audit record.
                /// </summary>
                public class AuditEntry : IAggregateRoot
                {
                    public Guid AuditEntryId { get; set; }

                    public required string EntityId { get; set; }

                    public required string EntityType { get; set; }

                    public required string Action { get; set; }

                    public string? UserId { get; set; }

                    public string? UserName { get; set; }

                    public string? IpAddress { get; set; }

                    public string? OldValues { get; set; }

                    public string? NewValues { get; set; }

                    public string? AffectedColumns { get; set; }

                    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

                    public string? CorrelationId { get; set; }

                    public ICollection<ChangeLog> ChangeLogs { get; set; } = new List<ChangeLog>();
                }
                """
        };
    }

    private static FileModel CreateChangeLogEntityFile(string directory)
    {
        return new FileModel("ChangeLog", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Audit.Core.Entities;

                /// <summary>
                /// ChangeLog entity representing a single property change within an audit entry.
                /// </summary>
                public class ChangeLog
                {
                    public Guid ChangeLogId { get; set; }

                    public Guid AuditEntryId { get; set; }

                    public AuditEntry AuditEntry { get; set; } = null!;

                    public required string PropertyName { get; set; }

                    public string? OldValue { get; set; }

                    public string? NewValue { get; set; }

                    public required string PropertyType { get; set; }
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

    private static FileModel CreateIAuditRepositoryFile(string directory)
    {
        return new FileModel("IAuditRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Audit.Core.Entities;

                namespace Audit.Core.Interfaces;

                /// <summary>
                /// Repository interface for AuditEntry entities.
                /// </summary>
                public interface IAuditRepository
                {
                    Task<AuditEntry?> GetByIdAsync(Guid auditEntryId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<AuditEntry>> GetByEntityIdAsync(string entityId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<AuditEntry>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);

                    Task<IEnumerable<AuditEntry>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<AuditEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

                    Task<IEnumerable<AuditEntry>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default);

                    Task<AuditEntry> AddAsync(AuditEntry auditEntry, CancellationToken cancellationToken = default);

                    Task AddBatchAsync(IEnumerable<AuditEntry> auditEntries, CancellationToken cancellationToken = default);

                    Task<long> GetCountAsync(string? entityType = null, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateIAuditServiceFile(string directory)
    {
        return new FileModel("IAuditService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Audit.Core.Entities;

                namespace Audit.Core.Interfaces;

                /// <summary>
                /// Service interface for audit operations.
                /// </summary>
                public interface IAuditService
                {
                    Task<AuditEntry> LogAuditAsync(
                        string entityId,
                        string entityType,
                        string action,
                        string? userId = null,
                        string? userName = null,
                        string? ipAddress = null,
                        object? oldValues = null,
                        object? newValues = null,
                        IEnumerable<string>? affectedColumns = null,
                        string? correlationId = null,
                        CancellationToken cancellationToken = default);

                    Task<AuditEntry> LogCreateAsync(
                        string entityId,
                        string entityType,
                        object newValues,
                        string? userId = null,
                        string? userName = null,
                        string? ipAddress = null,
                        string? correlationId = null,
                        CancellationToken cancellationToken = default);

                    Task<AuditEntry> LogUpdateAsync(
                        string entityId,
                        string entityType,
                        object oldValues,
                        object newValues,
                        IEnumerable<string>? affectedColumns = null,
                        string? userId = null,
                        string? userName = null,
                        string? ipAddress = null,
                        string? correlationId = null,
                        CancellationToken cancellationToken = default);

                    Task<AuditEntry> LogDeleteAsync(
                        string entityId,
                        string entityType,
                        object oldValues,
                        string? userId = null,
                        string? userName = null,
                        string? ipAddress = null,
                        string? correlationId = null,
                        CancellationToken cancellationToken = default);

                    Task<IEnumerable<AuditEntry>> GetEntityHistoryAsync(
                        string entityId,
                        CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateAuditEntryCreatedEventFile(string directory)
    {
        return new FileModel("AuditEntryCreatedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Audit.Core.Interfaces;

                namespace Audit.Core.Events;

                /// <summary>
                /// Event raised when a new audit entry is created.
                /// </summary>
                public sealed class AuditEntryCreatedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "AuditEntry";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string EntityId { get; init; }

                    public required string EntityType { get; init; }

                    public required string Action { get; init; }

                    public string? UserId { get; init; }
                }
                """
        };
    }

    private static FileModel CreateAuditEntryDtoFile(string directory)
    {
        return new FileModel("AuditEntryDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Audit.Core.DTOs;

                /// <summary>
                /// Data transfer object for AuditEntry.
                /// </summary>
                public sealed class AuditEntryDto
                {
                    public Guid AuditEntryId { get; init; }

                    public required string EntityId { get; init; }

                    public required string EntityType { get; init; }

                    public required string Action { get; init; }

                    public string? UserId { get; init; }

                    public string? UserName { get; init; }

                    public string? IpAddress { get; init; }

                    public string? OldValues { get; init; }

                    public string? NewValues { get; init; }

                    public string? AffectedColumns { get; init; }

                    public DateTime Timestamp { get; init; }

                    public string? CorrelationId { get; init; }

                    public IReadOnlyList<ChangeLogDto> ChangeLogs { get; init; } = Array.Empty<ChangeLogDto>();
                }
                """
        };
    }

    private static FileModel CreateChangeLogDtoFile(string directory)
    {
        return new FileModel("ChangeLogDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Audit.Core.DTOs;

                /// <summary>
                /// Data transfer object for ChangeLog.
                /// </summary>
                public sealed class ChangeLogDto
                {
                    public Guid ChangeLogId { get; init; }

                    public required string PropertyName { get; init; }

                    public string? OldValue { get; init; }

                    public string? NewValue { get; init; }

                    public required string PropertyType { get; init; }
                }
                """
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static FileModel CreateAuditDbContextFile(string directory)
    {
        return new FileModel("AuditDbContext", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Audit.Core.Entities;
                using Microsoft.EntityFrameworkCore;

                namespace Audit.Infrastructure.Data;

                /// <summary>
                /// Entity Framework Core DbContext for Audit microservice.
                /// </summary>
                public class AuditDbContext : DbContext
                {
                    public AuditDbContext(DbContextOptions<AuditDbContext> options)
                        : base(options)
                    {
                    }

                    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

                    public DbSet<ChangeLog> ChangeLogs => Set<ChangeLog>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        base.OnModelCreating(modelBuilder);
                        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
                    }
                }
                """
        };
    }

    private static FileModel CreateAuditEntryConfigurationFile(string directory)
    {
        return new FileModel("AuditEntryConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Audit.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Audit.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for AuditEntry.
                /// </summary>
                public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
                {
                    public void Configure(EntityTypeBuilder<AuditEntry> builder)
                    {
                        builder.HasKey(a => a.AuditEntryId);

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
                            .HasColumnType("nvarchar(max)");

                        builder.Property(a => a.NewValues)
                            .HasColumnType("nvarchar(max)");

                        builder.Property(a => a.AffectedColumns)
                            .HasColumnType("nvarchar(max)");

                        builder.Property(a => a.Timestamp)
                            .IsRequired();

                        builder.Property(a => a.CorrelationId)
                            .HasMaxLength(100);

                        builder.HasIndex(a => a.EntityId);
                        builder.HasIndex(a => a.EntityType);
                        builder.HasIndex(a => a.UserId);
                        builder.HasIndex(a => a.Timestamp);
                        builder.HasIndex(a => a.CorrelationId);
                    }
                }
                """
        };
    }

    private static FileModel CreateChangeLogConfigurationFile(string directory)
    {
        return new FileModel("ChangeLogConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Audit.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Audit.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for ChangeLog.
                /// </summary>
                public class ChangeLogConfiguration : IEntityTypeConfiguration<ChangeLog>
                {
                    public void Configure(EntityTypeBuilder<ChangeLog> builder)
                    {
                        builder.HasKey(c => c.ChangeLogId);

                        builder.Property(c => c.PropertyName)
                            .IsRequired()
                            .HasMaxLength(200);

                        builder.Property(c => c.OldValue)
                            .HasColumnType("nvarchar(max)");

                        builder.Property(c => c.NewValue)
                            .HasColumnType("nvarchar(max)");

                        builder.Property(c => c.PropertyType)
                            .IsRequired()
                            .HasMaxLength(200);

                        builder.HasOne(c => c.AuditEntry)
                            .WithMany(a => a.ChangeLogs)
                            .HasForeignKey(c => c.AuditEntryId)
                            .OnDelete(DeleteBehavior.Cascade);
                    }
                }
                """
        };
    }

    private static FileModel CreateAuditRepositoryFile(string directory)
    {
        return new FileModel("AuditRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Audit.Core.Entities;
                using Audit.Core.Interfaces;
                using Audit.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace Audit.Infrastructure.Repositories;

                /// <summary>
                /// Repository implementation for AuditEntry entities.
                /// </summary>
                public class AuditRepository : IAuditRepository
                {
                    private readonly AuditDbContext context;

                    public AuditRepository(AuditDbContext context)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                    }

                    public async Task<AuditEntry?> GetByIdAsync(Guid auditEntryId, CancellationToken cancellationToken = default)
                    {
                        return await context.AuditEntries
                            .Include(a => a.ChangeLogs)
                            .FirstOrDefaultAsync(a => a.AuditEntryId == auditEntryId, cancellationToken);
                    }

                    public async Task<IEnumerable<AuditEntry>> GetByEntityIdAsync(string entityId, CancellationToken cancellationToken = default)
                    {
                        return await context.AuditEntries
                            .Include(a => a.ChangeLogs)
                            .Where(a => a.EntityId == entityId)
                            .OrderByDescending(a => a.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<AuditEntry>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
                    {
                        return await context.AuditEntries
                            .Include(a => a.ChangeLogs)
                            .Where(a => a.EntityType == entityType)
                            .OrderByDescending(a => a.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<AuditEntry>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
                    {
                        return await context.AuditEntries
                            .Include(a => a.ChangeLogs)
                            .Where(a => a.UserId == userId)
                            .OrderByDescending(a => a.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<AuditEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
                    {
                        return await context.AuditEntries
                            .Include(a => a.ChangeLogs)
                            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                            .OrderByDescending(a => a.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<AuditEntry>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default)
                    {
                        return await context.AuditEntries
                            .Include(a => a.ChangeLogs)
                            .OrderByDescending(a => a.Timestamp)
                            .Skip(skip)
                            .Take(take)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<AuditEntry> AddAsync(AuditEntry auditEntry, CancellationToken cancellationToken = default)
                    {
                        auditEntry.AuditEntryId = Guid.NewGuid();
                        auditEntry.Timestamp = DateTime.UtcNow;
                        await context.AuditEntries.AddAsync(auditEntry, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return auditEntry;
                    }

                    public async Task AddBatchAsync(IEnumerable<AuditEntry> auditEntries, CancellationToken cancellationToken = default)
                    {
                        foreach (var auditEntry in auditEntries)
                        {
                            auditEntry.AuditEntryId = Guid.NewGuid();
                            auditEntry.Timestamp = DateTime.UtcNow;
                        }

                        await context.AuditEntries.AddRangeAsync(auditEntries, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task<long> GetCountAsync(string? entityType = null, CancellationToken cancellationToken = default)
                    {
                        var query = context.AuditEntries.AsQueryable();

                        if (!string.IsNullOrEmpty(entityType))
                        {
                            query = query.Where(a => a.EntityType == entityType);
                        }

                        return await query.LongCountAsync(cancellationToken);
                    }
                }
                """
        };
    }

    private static FileModel CreateAuditServiceFile(string directory)
    {
        return new FileModel("AuditService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Text.Json;
                using Audit.Core.Entities;
                using Audit.Core.Interfaces;
                using Microsoft.Extensions.Logging;

                namespace Audit.Infrastructure.Services;

                /// <summary>
                /// Service implementation for audit operations.
                /// </summary>
                public class AuditService : IAuditService
                {
                    private readonly IAuditRepository auditRepository;
                    private readonly ILogger<AuditService> logger;

                    public AuditService(IAuditRepository auditRepository, ILogger<AuditService> logger)
                    {
                        this.auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    public async Task<AuditEntry> LogAuditAsync(
                        string entityId,
                        string entityType,
                        string action,
                        string? userId = null,
                        string? userName = null,
                        string? ipAddress = null,
                        object? oldValues = null,
                        object? newValues = null,
                        IEnumerable<string>? affectedColumns = null,
                        string? correlationId = null,
                        CancellationToken cancellationToken = default)
                    {
                        var auditEntry = new AuditEntry
                        {
                            EntityId = entityId,
                            EntityType = entityType,
                            Action = action,
                            UserId = userId,
                            UserName = userName,
                            IpAddress = ipAddress,
                            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                            AffectedColumns = affectedColumns != null ? string.Join(",", affectedColumns) : null,
                            CorrelationId = correlationId ?? Guid.NewGuid().ToString()
                        };

                        var createdEntry = await auditRepository.AddAsync(auditEntry, cancellationToken);

                        logger.LogInformation(
                            "Audit entry created: {Action} on {EntityType} ({EntityId}) by {UserId}",
                            action,
                            entityType,
                            entityId,
                            userId);

                        return createdEntry;
                    }

                    public async Task<AuditEntry> LogCreateAsync(
                        string entityId,
                        string entityType,
                        object newValues,
                        string? userId = null,
                        string? userName = null,
                        string? ipAddress = null,
                        string? correlationId = null,
                        CancellationToken cancellationToken = default)
                    {
                        return await LogAuditAsync(
                            entityId,
                            entityType,
                            "Create",
                            userId,
                            userName,
                            ipAddress,
                            null,
                            newValues,
                            null,
                            correlationId,
                            cancellationToken);
                    }

                    public async Task<AuditEntry> LogUpdateAsync(
                        string entityId,
                        string entityType,
                        object oldValues,
                        object newValues,
                        IEnumerable<string>? affectedColumns = null,
                        string? userId = null,
                        string? userName = null,
                        string? ipAddress = null,
                        string? correlationId = null,
                        CancellationToken cancellationToken = default)
                    {
                        return await LogAuditAsync(
                            entityId,
                            entityType,
                            "Update",
                            userId,
                            userName,
                            ipAddress,
                            oldValues,
                            newValues,
                            affectedColumns,
                            correlationId,
                            cancellationToken);
                    }

                    public async Task<AuditEntry> LogDeleteAsync(
                        string entityId,
                        string entityType,
                        object oldValues,
                        string? userId = null,
                        string? userName = null,
                        string? ipAddress = null,
                        string? correlationId = null,
                        CancellationToken cancellationToken = default)
                    {
                        return await LogAuditAsync(
                            entityId,
                            entityType,
                            "Delete",
                            userId,
                            userName,
                            ipAddress,
                            oldValues,
                            null,
                            null,
                            correlationId,
                            cancellationToken);
                    }

                    public async Task<IEnumerable<AuditEntry>> GetEntityHistoryAsync(
                        string entityId,
                        CancellationToken cancellationToken = default)
                    {
                        return await auditRepository.GetByEntityIdAsync(entityId, cancellationToken);
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

                using Audit.Core.Interfaces;
                using Audit.Infrastructure.Data;
                using Audit.Infrastructure.Repositories;
                using Audit.Infrastructure.Services;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;

                namespace Microsoft.Extensions.DependencyInjection;

                /// <summary>
                /// Extension methods for configuring Audit infrastructure services.
                /// </summary>
                public static class ConfigureServices
                {
                    /// <summary>
                    /// Adds Audit infrastructure services to the service collection.
                    /// </summary>
                    public static IServiceCollection AddAuditInfrastructure(
                        this IServiceCollection services,
                        IConfiguration configuration)
                    {
                        services.AddDbContext<AuditDbContext>(options =>
                            options.UseSqlServer(
                                configuration.GetConnectionString("AuditDb") ??
                                @"Server=.\SQLEXPRESS;Database=AuditDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IAuditRepository, AuditRepository>();
                        services.AddScoped<IAuditService, AuditService>();

                        return services;
                    }
                }
                """
        };
    }

    #endregion

    #region API Layer Files

    private static FileModel CreateAuditControllerFile(string directory)
    {
        return new FileModel("AuditController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Audit.Core.DTOs;
                using Audit.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace Audit.Api.Controllers;

                /// <summary>
                /// Controller for audit operations.
                /// </summary>
                [ApiController]
                [Route("api/audit")]
                [Authorize]
                public class AuditController : ControllerBase
                {
                    private readonly IAuditRepository auditRepository;
                    private readonly ILogger<AuditController> logger;

                    public AuditController(
                        IAuditRepository auditRepository,
                        ILogger<AuditController> logger)
                    {
                        this.auditRepository = auditRepository;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Get all audit entries with pagination.
                    /// </summary>
                    [HttpGet("entries")]
                    [ProducesResponseType(typeof(IEnumerable<AuditEntryDto>), StatusCodes.Status200OK)]
                    public async Task<ActionResult<IEnumerable<AuditEntryDto>>> GetEntries(
                        [FromQuery] int skip = 0,
                        [FromQuery] int take = 100,
                        CancellationToken cancellationToken = default)
                    {
                        var entries = await auditRepository.GetAllAsync(skip, take, cancellationToken);

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

                        return Ok(entryDtos);
                    }

                    /// <summary>
                    /// Get an audit entry by ID.
                    /// </summary>
                    [HttpGet("entries/{id:guid}")]
                    [ProducesResponseType(typeof(AuditEntryDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<AuditEntryDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var entry = await auditRepository.GetByIdAsync(id, cancellationToken);

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
                        });
                    }

                    /// <summary>
                    /// Get audit entries by entity ID.
                    /// </summary>
                    [HttpGet("entity/{entityId}")]
                    [ProducesResponseType(typeof(IEnumerable<AuditEntryDto>), StatusCodes.Status200OK)]
                    public async Task<ActionResult<IEnumerable<AuditEntryDto>>> GetByEntityId(
                        string entityId,
                        CancellationToken cancellationToken)
                    {
                        var entries = await auditRepository.GetByEntityIdAsync(entityId, cancellationToken);

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

                        return Ok(entryDtos);
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
