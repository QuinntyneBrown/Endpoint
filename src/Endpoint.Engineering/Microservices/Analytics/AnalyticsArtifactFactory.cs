// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Analytics;

/// <summary>
/// Factory for creating Analytics microservice artifacts according to analytics-microservice.spec.md.
/// </summary>
public class AnalyticsArtifactFactory : IAnalyticsArtifactFactory
{
    private readonly ILogger<AnalyticsArtifactFactory> logger;

    public AnalyticsArtifactFactory(ILogger<AnalyticsArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Analytics.Core files");

        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");

        // Entities
        project.Files.Add(CreateIAggregateRootFile(entitiesDir));
        project.Files.Add(CreateEventEntityFile(entitiesDir));
        project.Files.Add(CreateMetricEntityFile(entitiesDir));
        project.Files.Add(CreateReportEntityFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateIDomainEventFile(interfacesDir));
        project.Files.Add(CreateIEventRepositoryFile(interfacesDir));
        project.Files.Add(CreateIMetricsServiceFile(interfacesDir));
        project.Files.Add(CreateIReportGeneratorFile(interfacesDir));

        // Events
        project.Files.Add(CreateEventTrackedEventFile(eventsDir));
        project.Files.Add(CreateReportGeneratedEventFile(eventsDir));
        project.Files.Add(CreateMetricThresholdExceededEventFile(eventsDir));

        // DTOs
        var dtosDir = Path.Combine(project.Directory, "DTOs");
        project.Files.Add(CreateEventDtoFile(dtosDir));
        project.Files.Add(CreateMetricDtoFile(dtosDir));
        project.Files.Add(CreateReportDtoFile(dtosDir));
        project.Files.Add(CreateTrackEventRequestFile(dtosDir));
        project.Files.Add(CreateMetricsQueryRequestFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Analytics.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        // DbContext
        project.Files.Add(CreateAnalyticsDbContextFile(dataDir));

        // Entity Configurations
        project.Files.Add(CreateEventConfigurationFile(configurationsDir));
        project.Files.Add(CreateMetricConfigurationFile(configurationsDir));
        project.Files.Add(CreateReportConfigurationFile(configurationsDir));

        // Repositories
        project.Files.Add(CreateEventRepositoryFile(repositoriesDir));

        // Services
        project.Files.Add(CreateMetricsServiceFile(servicesDir));
        project.Files.Add(CreateReportGeneratorFile(servicesDir));

        // ConfigureServices
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Analytics.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Controllers
        project.Files.Add(CreateEventsControllerFile(controllersDir));
        project.Files.Add(CreateMetricsControllerFile(controllersDir));
        project.Files.Add(CreateReportsControllerFile(controllersDir));

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

                namespace Analytics.Core.Entities;

                /// <summary>
                /// Marker interface for aggregate roots.
                /// </summary>
                public interface IAggregateRoot
                {
                }
                """
        };
    }

    private static FileModel CreateEventEntityFile(string directory)
    {
        return new FileModel("Event", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Analytics.Core.Entities;

                /// <summary>
                /// Event entity representing a tracked analytics event.
                /// </summary>
                public class Event : IAggregateRoot
                {
                    public Guid EventId { get; set; }

                    public required string EventType { get; set; }

                    public required string Source { get; set; }

                    public string? UserId { get; set; }

                    public string? SessionId { get; set; }

                    public Dictionary<string, object>? Properties { get; set; }

                    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                }
                """
        };
    }

    private static FileModel CreateMetricEntityFile(string directory)
    {
        return new FileModel("Metric", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Analytics.Core.Entities;

                /// <summary>
                /// Metric entity representing a measured analytics metric.
                /// </summary>
                public class Metric : IAggregateRoot
                {
                    public Guid MetricId { get; set; }

                    public required string Name { get; set; }

                    public required string Category { get; set; }

                    public double Value { get; set; }

                    public string? Unit { get; set; }

                    public Dictionary<string, string>? Tags { get; set; }

                    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                }
                """
        };
    }

    private static FileModel CreateReportEntityFile(string directory)
    {
        return new FileModel("Report", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Analytics.Core.Entities;

                /// <summary>
                /// Report entity representing a generated analytics report.
                /// </summary>
                public class Report : IAggregateRoot
                {
                    public Guid ReportId { get; set; }

                    public required string Name { get; set; }

                    public required string ReportType { get; set; }

                    public string? Description { get; set; }

                    public DateTime StartDate { get; set; }

                    public DateTime EndDate { get; set; }

                    public string? GeneratedBy { get; set; }

                    public string? Data { get; set; }

                    public ReportStatus Status { get; set; } = ReportStatus.Pending;

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                    public DateTime? CompletedAt { get; set; }
                }

                /// <summary>
                /// Report generation status.
                /// </summary>
                public enum ReportStatus
                {
                    Pending,
                    Processing,
                    Completed,
                    Failed
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

                namespace Analytics.Core.Interfaces;

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

    private static FileModel CreateIEventRepositoryFile(string directory)
    {
        return new FileModel("IEventRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Entities;

                namespace Analytics.Core.Interfaces;

                /// <summary>
                /// Repository interface for Event entities.
                /// </summary>
                public interface IEventRepository
                {
                    Task<Event?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Event>> GetByTypeAsync(string eventType, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Event>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Event>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Event>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default);

                    Task<Event> AddAsync(Event analyticsEvent, CancellationToken cancellationToken = default);

                    Task AddBatchAsync(IEnumerable<Event> events, CancellationToken cancellationToken = default);

                    Task<long> GetCountAsync(string? eventType = null, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateIMetricsServiceFile(string directory)
    {
        return new FileModel("IMetricsService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Entities;

                namespace Analytics.Core.Interfaces;

                /// <summary>
                /// Service interface for metrics operations.
                /// </summary>
                public interface IMetricsService
                {
                    Task<Metric> RecordMetricAsync(Metric metric, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Metric>> GetMetricsByNameAsync(string name, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Metric>> GetMetricsByCategoryAsync(string category, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Metric>> GetMetricsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

                    Task<double> GetAverageAsync(string metricName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

                    Task<double> GetSumAsync(string metricName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

                    Task<double> GetMinAsync(string metricName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

                    Task<double> GetMaxAsync(string metricName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

                    Task CheckThresholdsAsync(CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateIReportGeneratorFile(string directory)
    {
        return new FileModel("IReportGenerator", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Entities;

                namespace Analytics.Core.Interfaces;

                /// <summary>
                /// Service interface for report generation.
                /// </summary>
                public interface IReportGenerator
                {
                    Task<Report> GenerateReportAsync(string name, string reportType, DateTime startDate, DateTime endDate, string? generatedBy = null, CancellationToken cancellationToken = default);

                    Task<Report?> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Report>> GetReportsByTypeAsync(string reportType, CancellationToken cancellationToken = default);

                    Task<IEnumerable<Report>> GetAllReportsAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default);

                    Task<Report> UpdateReportStatusAsync(Guid reportId, ReportStatus status, string? data = null, CancellationToken cancellationToken = default);
                }
                """
        };
    }

    private static FileModel CreateEventTrackedEventFile(string directory)
    {
        return new FileModel("EventTrackedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Interfaces;

                namespace Analytics.Core.Events;

                /// <summary>
                /// Event raised when an analytics event is tracked.
                /// </summary>
                public sealed class EventTrackedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "Event";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string EventType { get; init; }

                    public required string Source { get; init; }

                    public string? UserId { get; init; }
                }
                """
        };
    }

    private static FileModel CreateReportGeneratedEventFile(string directory)
    {
        return new FileModel("ReportGeneratedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Interfaces;

                namespace Analytics.Core.Events;

                /// <summary>
                /// Event raised when a report is generated.
                /// </summary>
                public sealed class ReportGeneratedEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "Report";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string ReportName { get; init; }

                    public required string ReportType { get; init; }

                    public string? GeneratedBy { get; init; }
                }
                """
        };
    }

    private static FileModel CreateMetricThresholdExceededEventFile(string directory)
    {
        return new FileModel("MetricThresholdExceededEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Interfaces;

                namespace Analytics.Core.Events;

                /// <summary>
                /// Event raised when a metric threshold is exceeded.
                /// </summary>
                public sealed class MetricThresholdExceededEvent : IDomainEvent
                {
                    public Guid AggregateId { get; init; }

                    public string AggregateType => "Metric";

                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

                    public required string CorrelationId { get; init; }

                    public required string MetricName { get; init; }

                    public required double CurrentValue { get; init; }

                    public required double ThresholdValue { get; init; }

                    public required string ThresholdType { get; init; }
                }
                """
        };
    }

    private static FileModel CreateEventDtoFile(string directory)
    {
        return new FileModel("EventDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Analytics.Core.DTOs;

                /// <summary>
                /// Data transfer object for Event.
                /// </summary>
                public sealed class EventDto
                {
                    public Guid EventId { get; init; }

                    public required string EventType { get; init; }

                    public required string Source { get; init; }

                    public string? UserId { get; init; }

                    public string? SessionId { get; init; }

                    public Dictionary<string, object>? Properties { get; init; }

                    public DateTime Timestamp { get; init; }
                }
                """
        };
    }

    private static FileModel CreateMetricDtoFile(string directory)
    {
        return new FileModel("MetricDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Analytics.Core.DTOs;

                /// <summary>
                /// Data transfer object for Metric.
                /// </summary>
                public sealed class MetricDto
                {
                    public Guid MetricId { get; init; }

                    public required string Name { get; init; }

                    public required string Category { get; init; }

                    public double Value { get; init; }

                    public string? Unit { get; init; }

                    public Dictionary<string, string>? Tags { get; init; }

                    public DateTime Timestamp { get; init; }
                }
                """
        };
    }

    private static FileModel CreateReportDtoFile(string directory)
    {
        return new FileModel("ReportDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Entities;

                namespace Analytics.Core.DTOs;

                /// <summary>
                /// Data transfer object for Report.
                /// </summary>
                public sealed class ReportDto
                {
                    public Guid ReportId { get; init; }

                    public required string Name { get; init; }

                    public required string ReportType { get; init; }

                    public string? Description { get; init; }

                    public DateTime StartDate { get; init; }

                    public DateTime EndDate { get; init; }

                    public string? GeneratedBy { get; init; }

                    public string? Data { get; init; }

                    public ReportStatus Status { get; init; }

                    public DateTime CreatedAt { get; init; }

                    public DateTime? CompletedAt { get; init; }
                }
                """
        };
    }

    private static FileModel CreateTrackEventRequestFile(string directory)
    {
        return new FileModel("TrackEventRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Analytics.Core.DTOs;

                /// <summary>
                /// Request model for tracking an analytics event.
                /// </summary>
                public sealed class TrackEventRequest
                {
                    [Required]
                    public required string EventType { get; init; }

                    [Required]
                    public required string Source { get; init; }

                    public string? UserId { get; init; }

                    public string? SessionId { get; init; }

                    public Dictionary<string, object>? Properties { get; init; }

                    public DateTime? Timestamp { get; init; }
                }
                """
        };
    }

    private static FileModel CreateMetricsQueryRequestFile(string directory)
    {
        return new FileModel("MetricsQueryRequest", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Analytics.Core.DTOs;

                /// <summary>
                /// Request model for querying metrics.
                /// </summary>
                public sealed class MetricsQueryRequest
                {
                    public string? Name { get; init; }

                    public string? Category { get; init; }

                    public DateTime? StartDate { get; init; }

                    public DateTime? EndDate { get; init; }

                    public string? AggregationType { get; init; }
                }
                """
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static FileModel CreateAnalyticsDbContextFile(string directory)
    {
        return new FileModel("AnalyticsDbContext", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Entities;
                using Microsoft.EntityFrameworkCore;

                namespace Analytics.Infrastructure.Data;

                /// <summary>
                /// Entity Framework Core DbContext for Analytics microservice.
                /// </summary>
                public class AnalyticsDbContext : DbContext
                {
                    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
                        : base(options)
                    {
                    }

                    public DbSet<Event> Events => Set<Event>();

                    public DbSet<Metric> Metrics => Set<Metric>();

                    public DbSet<Report> Reports => Set<Report>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        base.OnModelCreating(modelBuilder);
                        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);
                    }
                }
                """
        };
    }

    private static FileModel CreateEventConfigurationFile(string directory)
    {
        return new FileModel("EventConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Analytics.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for Event.
                /// </summary>
                public class EventConfiguration : IEntityTypeConfiguration<Event>
                {
                    public void Configure(EntityTypeBuilder<Event> builder)
                    {
                        builder.HasKey(e => e.EventId);

                        builder.Property(e => e.EventType)
                            .IsRequired()
                            .HasMaxLength(100);

                        builder.Property(e => e.Source)
                            .IsRequired()
                            .HasMaxLength(200);

                        builder.Property(e => e.UserId)
                            .HasMaxLength(100);

                        builder.Property(e => e.SessionId)
                            .HasMaxLength(100);

                        builder.Property(e => e.Properties)
                            .HasColumnType("nvarchar(max)");

                        builder.Property(e => e.Timestamp)
                            .IsRequired();

                        builder.Property(e => e.CreatedAt)
                            .IsRequired();

                        builder.HasIndex(e => e.EventType);
                        builder.HasIndex(e => e.Timestamp);
                        builder.HasIndex(e => e.UserId);
                    }
                }
                """
        };
    }

    private static FileModel CreateMetricConfigurationFile(string directory)
    {
        return new FileModel("MetricConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Analytics.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for Metric.
                /// </summary>
                public class MetricConfiguration : IEntityTypeConfiguration<Metric>
                {
                    public void Configure(EntityTypeBuilder<Metric> builder)
                    {
                        builder.HasKey(m => m.MetricId);

                        builder.Property(m => m.Name)
                            .IsRequired()
                            .HasMaxLength(100);

                        builder.Property(m => m.Category)
                            .IsRequired()
                            .HasMaxLength(100);

                        builder.Property(m => m.Value)
                            .IsRequired();

                        builder.Property(m => m.Unit)
                            .HasMaxLength(50);

                        builder.Property(m => m.Tags)
                            .HasColumnType("nvarchar(max)");

                        builder.Property(m => m.Timestamp)
                            .IsRequired();

                        builder.Property(m => m.CreatedAt)
                            .IsRequired();

                        builder.HasIndex(m => m.Name);
                        builder.HasIndex(m => m.Category);
                        builder.HasIndex(m => m.Timestamp);
                    }
                }
                """
        };
    }

    private static FileModel CreateReportConfigurationFile(string directory)
    {
        return new FileModel("ReportConfiguration", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace Analytics.Infrastructure.Data.Configurations;

                /// <summary>
                /// Entity configuration for Report.
                /// </summary>
                public class ReportConfiguration : IEntityTypeConfiguration<Report>
                {
                    public void Configure(EntityTypeBuilder<Report> builder)
                    {
                        builder.HasKey(r => r.ReportId);

                        builder.Property(r => r.Name)
                            .IsRequired()
                            .HasMaxLength(200);

                        builder.Property(r => r.ReportType)
                            .IsRequired()
                            .HasMaxLength(100);

                        builder.Property(r => r.Description)
                            .HasMaxLength(1000);

                        builder.Property(r => r.GeneratedBy)
                            .HasMaxLength(100);

                        builder.Property(r => r.Data)
                            .HasColumnType("nvarchar(max)");

                        builder.Property(r => r.Status)
                            .IsRequired()
                            .HasConversion<string>()
                            .HasMaxLength(50);

                        builder.Property(r => r.CreatedAt)
                            .IsRequired();

                        builder.HasIndex(r => r.ReportType);
                        builder.HasIndex(r => r.Status);
                        builder.HasIndex(r => r.CreatedAt);
                    }
                }
                """
        };
    }

    private static FileModel CreateEventRepositoryFile(string directory)
    {
        return new FileModel("EventRepository", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Entities;
                using Analytics.Core.Interfaces;
                using Analytics.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace Analytics.Infrastructure.Repositories;

                /// <summary>
                /// Repository implementation for Event entities.
                /// </summary>
                public class EventRepository : IEventRepository
                {
                    private readonly AnalyticsDbContext context;

                    public EventRepository(AnalyticsDbContext context)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                    }

                    public async Task<Event?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
                    {
                        return await context.Events
                            .FirstOrDefaultAsync(e => e.EventId == eventId, cancellationToken);
                    }

                    public async Task<IEnumerable<Event>> GetByTypeAsync(string eventType, CancellationToken cancellationToken = default)
                    {
                        return await context.Events
                            .Where(e => e.EventType == eventType)
                            .OrderByDescending(e => e.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<Event>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
                    {
                        return await context.Events
                            .Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate)
                            .OrderByDescending(e => e.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<Event>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
                    {
                        return await context.Events
                            .Where(e => e.UserId == userId)
                            .OrderByDescending(e => e.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<Event>> GetAllAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default)
                    {
                        return await context.Events
                            .OrderByDescending(e => e.Timestamp)
                            .Skip(skip)
                            .Take(take)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<Event> AddAsync(Event analyticsEvent, CancellationToken cancellationToken = default)
                    {
                        analyticsEvent.EventId = Guid.NewGuid();
                        analyticsEvent.CreatedAt = DateTime.UtcNow;
                        await context.Events.AddAsync(analyticsEvent, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return analyticsEvent;
                    }

                    public async Task AddBatchAsync(IEnumerable<Event> events, CancellationToken cancellationToken = default)
                    {
                        foreach (var analyticsEvent in events)
                        {
                            analyticsEvent.EventId = Guid.NewGuid();
                            analyticsEvent.CreatedAt = DateTime.UtcNow;
                        }

                        await context.Events.AddRangeAsync(events, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task<long> GetCountAsync(string? eventType = null, CancellationToken cancellationToken = default)
                    {
                        var query = context.Events.AsQueryable();

                        if (!string.IsNullOrEmpty(eventType))
                        {
                            query = query.Where(e => e.EventType == eventType);
                        }

                        return await query.LongCountAsync(cancellationToken);
                    }
                }
                """
        };
    }

    private static FileModel CreateMetricsServiceFile(string directory)
    {
        return new FileModel("MetricsService", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Entities;
                using Analytics.Core.Interfaces;
                using Analytics.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Logging;

                namespace Analytics.Infrastructure.Services;

                /// <summary>
                /// Service implementation for metrics operations.
                /// </summary>
                public class MetricsService : IMetricsService
                {
                    private readonly AnalyticsDbContext context;
                    private readonly ILogger<MetricsService> logger;

                    public MetricsService(AnalyticsDbContext context, ILogger<MetricsService> logger)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    public async Task<Metric> RecordMetricAsync(Metric metric, CancellationToken cancellationToken = default)
                    {
                        metric.MetricId = Guid.NewGuid();
                        metric.CreatedAt = DateTime.UtcNow;
                        await context.Metrics.AddAsync(metric, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);

                        logger.LogInformation("Recorded metric {MetricName} with value {Value}", metric.Name, metric.Value);
                        return metric;
                    }

                    public async Task<IEnumerable<Metric>> GetMetricsByNameAsync(string name, CancellationToken cancellationToken = default)
                    {
                        return await context.Metrics
                            .Where(m => m.Name == name)
                            .OrderByDescending(m => m.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<Metric>> GetMetricsByCategoryAsync(string category, CancellationToken cancellationToken = default)
                    {
                        return await context.Metrics
                            .Where(m => m.Category == category)
                            .OrderByDescending(m => m.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<Metric>> GetMetricsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
                    {
                        return await context.Metrics
                            .Where(m => m.Timestamp >= startDate && m.Timestamp <= endDate)
                            .OrderByDescending(m => m.Timestamp)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<double> GetAverageAsync(string metricName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
                    {
                        return await context.Metrics
                            .Where(m => m.Name == metricName && m.Timestamp >= startDate && m.Timestamp <= endDate)
                            .AverageAsync(m => m.Value, cancellationToken);
                    }

                    public async Task<double> GetSumAsync(string metricName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
                    {
                        return await context.Metrics
                            .Where(m => m.Name == metricName && m.Timestamp >= startDate && m.Timestamp <= endDate)
                            .SumAsync(m => m.Value, cancellationToken);
                    }

                    public async Task<double> GetMinAsync(string metricName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
                    {
                        return await context.Metrics
                            .Where(m => m.Name == metricName && m.Timestamp >= startDate && m.Timestamp <= endDate)
                            .MinAsync(m => m.Value, cancellationToken);
                    }

                    public async Task<double> GetMaxAsync(string metricName, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
                    {
                        return await context.Metrics
                            .Where(m => m.Name == metricName && m.Timestamp >= startDate && m.Timestamp <= endDate)
                            .MaxAsync(m => m.Value, cancellationToken);
                    }

                    public async Task CheckThresholdsAsync(CancellationToken cancellationToken = default)
                    {
                        // Implementation would check configured thresholds and raise events
                        logger.LogDebug("Checking metric thresholds");
                        await Task.CompletedTask;
                    }
                }
                """
        };
    }

    private static FileModel CreateReportGeneratorFile(string directory)
    {
        return new FileModel("ReportGenerator", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.Entities;
                using Analytics.Core.Interfaces;
                using Analytics.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Logging;
                using System.Text.Json;

                namespace Analytics.Infrastructure.Services;

                /// <summary>
                /// Service implementation for report generation.
                /// </summary>
                public class ReportGenerator : IReportGenerator
                {
                    private readonly AnalyticsDbContext context;
                    private readonly IEventRepository eventRepository;
                    private readonly IMetricsService metricsService;
                    private readonly ILogger<ReportGenerator> logger;

                    public ReportGenerator(
                        AnalyticsDbContext context,
                        IEventRepository eventRepository,
                        IMetricsService metricsService,
                        ILogger<ReportGenerator> logger)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                        this.eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
                        this.metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    public async Task<Report> GenerateReportAsync(
                        string name,
                        string reportType,
                        DateTime startDate,
                        DateTime endDate,
                        string? generatedBy = null,
                        CancellationToken cancellationToken = default)
                    {
                        var report = new Report
                        {
                            ReportId = Guid.NewGuid(),
                            Name = name,
                            ReportType = reportType,
                            StartDate = startDate,
                            EndDate = endDate,
                            GeneratedBy = generatedBy,
                            Status = ReportStatus.Processing,
                            CreatedAt = DateTime.UtcNow
                        };

                        await context.Reports.AddAsync(report, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);

                        logger.LogInformation("Starting report generation: {ReportName} ({ReportType})", name, reportType);

                        try
                        {
                            var reportData = await GenerateReportDataAsync(reportType, startDate, endDate, cancellationToken);
                            report.Data = JsonSerializer.Serialize(reportData);
                            report.Status = ReportStatus.Completed;
                            report.CompletedAt = DateTime.UtcNow;

                            logger.LogInformation("Report generation completed: {ReportId}", report.ReportId);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Report generation failed: {ReportId}", report.ReportId);
                            report.Status = ReportStatus.Failed;
                        }

                        context.Reports.Update(report);
                        await context.SaveChangesAsync(cancellationToken);

                        return report;
                    }

                    public async Task<Report?> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default)
                    {
                        return await context.Reports
                            .FirstOrDefaultAsync(r => r.ReportId == reportId, cancellationToken);
                    }

                    public async Task<IEnumerable<Report>> GetReportsByTypeAsync(string reportType, CancellationToken cancellationToken = default)
                    {
                        return await context.Reports
                            .Where(r => r.ReportType == reportType)
                            .OrderByDescending(r => r.CreatedAt)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<Report>> GetAllReportsAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default)
                    {
                        return await context.Reports
                            .OrderByDescending(r => r.CreatedAt)
                            .Skip(skip)
                            .Take(take)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<Report> UpdateReportStatusAsync(Guid reportId, ReportStatus status, string? data = null, CancellationToken cancellationToken = default)
                    {
                        var report = await context.Reports.FindAsync(new object[] { reportId }, cancellationToken)
                            ?? throw new InvalidOperationException($"Report {reportId} not found");

                        report.Status = status;

                        if (data != null)
                        {
                            report.Data = data;
                        }

                        if (status == ReportStatus.Completed)
                        {
                            report.CompletedAt = DateTime.UtcNow;
                        }

                        context.Reports.Update(report);
                        await context.SaveChangesAsync(cancellationToken);

                        return report;
                    }

                    private async Task<object> GenerateReportDataAsync(
                        string reportType,
                        DateTime startDate,
                        DateTime endDate,
                        CancellationToken cancellationToken)
                    {
                        var events = await eventRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
                        var metrics = await metricsService.GetMetricsByDateRangeAsync(startDate, endDate, cancellationToken);

                        return new
                        {
                            ReportType = reportType,
                            Period = new { StartDate = startDate, EndDate = endDate },
                            Summary = new
                            {
                                TotalEvents = events.Count(),
                                TotalMetrics = metrics.Count(),
                                EventTypes = events.GroupBy(e => e.EventType).Select(g => new { Type = g.Key, Count = g.Count() }),
                                MetricCategories = metrics.GroupBy(m => m.Category).Select(g => new { Category = g.Key, Count = g.Count() })
                            },
                            GeneratedAt = DateTime.UtcNow
                        };
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

                using Analytics.Core.Interfaces;
                using Analytics.Infrastructure.Data;
                using Analytics.Infrastructure.Repositories;
                using Analytics.Infrastructure.Services;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;

                namespace Microsoft.Extensions.DependencyInjection;

                /// <summary>
                /// Extension methods for configuring Analytics infrastructure services.
                /// </summary>
                public static class ConfigureServices
                {
                    /// <summary>
                    /// Adds Analytics infrastructure services to the service collection.
                    /// </summary>
                    public static IServiceCollection AddAnalyticsInfrastructure(
                        this IServiceCollection services,
                        IConfiguration configuration)
                    {
                        services.AddDbContext<AnalyticsDbContext>(options =>
                            options.UseSqlServer(
                                configuration.GetConnectionString("AnalyticsDb") ??
                                @"Server=.\SQLEXPRESS;Database=AnalyticsDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IEventRepository, EventRepository>();
                        services.AddScoped<IMetricsService, MetricsService>();
                        services.AddScoped<IReportGenerator, ReportGenerator>();

                        return services;
                    }
                }
                """
        };
    }

    #endregion

    #region API Layer Files

    private static FileModel CreateEventsControllerFile(string directory)
    {
        return new FileModel("EventsController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.DTOs;
                using Analytics.Core.Entities;
                using Analytics.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace Analytics.Api.Controllers;

                /// <summary>
                /// Controller for analytics event operations.
                /// </summary>
                [ApiController]
                [Route("api/analytics/events")]
                public class EventsController : ControllerBase
                {
                    private readonly IEventRepository eventRepository;
                    private readonly ILogger<EventsController> logger;

                    public EventsController(
                        IEventRepository eventRepository,
                        ILogger<EventsController> logger)
                    {
                        this.eventRepository = eventRepository;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Track a new analytics event.
                    /// </summary>
                    [HttpPost]
                    [AllowAnonymous]
                    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<EventDto>> TrackEvent(
                        [FromBody] TrackEventRequest request,
                        CancellationToken cancellationToken)
                    {
                        var analyticsEvent = new Event
                        {
                            EventType = request.EventType,
                            Source = request.Source,
                            UserId = request.UserId,
                            SessionId = request.SessionId,
                            Properties = request.Properties,
                            Timestamp = request.Timestamp ?? DateTime.UtcNow
                        };

                        var createdEvent = await eventRepository.AddAsync(analyticsEvent, cancellationToken);

                        logger.LogInformation("Event tracked: {EventType} from {Source}", request.EventType, request.Source);

                        var response = new EventDto
                        {
                            EventId = createdEvent.EventId,
                            EventType = createdEvent.EventType,
                            Source = createdEvent.Source,
                            UserId = createdEvent.UserId,
                            SessionId = createdEvent.SessionId,
                            Properties = createdEvent.Properties,
                            Timestamp = createdEvent.Timestamp
                        };

                        return CreatedAtAction(nameof(GetById), new { id = createdEvent.EventId }, response);
                    }

                    /// <summary>
                    /// Get an event by ID.
                    /// </summary>
                    [HttpGet("{id:guid}")]
                    [Authorize]
                    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<EventDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var analyticsEvent = await eventRepository.GetByIdAsync(id, cancellationToken);

                        if (analyticsEvent == null)
                        {
                            return NotFound();
                        }

                        return Ok(new EventDto
                        {
                            EventId = analyticsEvent.EventId,
                            EventType = analyticsEvent.EventType,
                            Source = analyticsEvent.Source,
                            UserId = analyticsEvent.UserId,
                            SessionId = analyticsEvent.SessionId,
                            Properties = analyticsEvent.Properties,
                            Timestamp = analyticsEvent.Timestamp
                        });
                    }

                    /// <summary>
                    /// Get events by type.
                    /// </summary>
                    [HttpGet("type/{eventType}")]
                    [Authorize]
                    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
                    public async Task<ActionResult<IEnumerable<EventDto>>> GetByType(string eventType, CancellationToken cancellationToken)
                    {
                        var events = await eventRepository.GetByTypeAsync(eventType, cancellationToken);

                        var eventDtos = events.Select(e => new EventDto
                        {
                            EventId = e.EventId,
                            EventType = e.EventType,
                            Source = e.Source,
                            UserId = e.UserId,
                            SessionId = e.SessionId,
                            Properties = e.Properties,
                            Timestamp = e.Timestamp
                        });

                        return Ok(eventDtos);
                    }
                }
                """
        };
    }

    private static FileModel CreateMetricsControllerFile(string directory)
    {
        return new FileModel("MetricsController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.DTOs;
                using Analytics.Core.Entities;
                using Analytics.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace Analytics.Api.Controllers;

                /// <summary>
                /// Controller for metrics operations.
                /// </summary>
                [ApiController]
                [Route("api/analytics/metrics")]
                [Authorize]
                public class MetricsController : ControllerBase
                {
                    private readonly IMetricsService metricsService;
                    private readonly ILogger<MetricsController> logger;

                    public MetricsController(
                        IMetricsService metricsService,
                        ILogger<MetricsController> logger)
                    {
                        this.metricsService = metricsService;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Get metrics with optional filtering.
                    /// </summary>
                    [HttpGet]
                    [ProducesResponseType(typeof(IEnumerable<MetricDto>), StatusCodes.Status200OK)]
                    public async Task<ActionResult<IEnumerable<MetricDto>>> GetMetrics(
                        [FromQuery] MetricsQueryRequest request,
                        CancellationToken cancellationToken)
                    {
                        IEnumerable<Metric> metrics;

                        if (!string.IsNullOrEmpty(request.Name))
                        {
                            metrics = await metricsService.GetMetricsByNameAsync(request.Name, cancellationToken);
                        }
                        else if (!string.IsNullOrEmpty(request.Category))
                        {
                            metrics = await metricsService.GetMetricsByCategoryAsync(request.Category, cancellationToken);
                        }
                        else if (request.StartDate.HasValue && request.EndDate.HasValue)
                        {
                            metrics = await metricsService.GetMetricsByDateRangeAsync(
                                request.StartDate.Value,
                                request.EndDate.Value,
                                cancellationToken);
                        }
                        else
                        {
                            metrics = await metricsService.GetMetricsByDateRangeAsync(
                                DateTime.UtcNow.AddDays(-7),
                                DateTime.UtcNow,
                                cancellationToken);
                        }

                        var metricDtos = metrics.Select(m => new MetricDto
                        {
                            MetricId = m.MetricId,
                            Name = m.Name,
                            Category = m.Category,
                            Value = m.Value,
                            Unit = m.Unit,
                            Tags = m.Tags,
                            Timestamp = m.Timestamp
                        });

                        return Ok(metricDtos);
                    }

                    /// <summary>
                    /// Record a new metric.
                    /// </summary>
                    [HttpPost]
                    [ProducesResponseType(typeof(MetricDto), StatusCodes.Status201Created)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<MetricDto>> RecordMetric(
                        [FromBody] MetricDto request,
                        CancellationToken cancellationToken)
                    {
                        var metric = new Metric
                        {
                            Name = request.Name,
                            Category = request.Category,
                            Value = request.Value,
                            Unit = request.Unit,
                            Tags = request.Tags,
                            Timestamp = request.Timestamp != default ? request.Timestamp : DateTime.UtcNow
                        };

                        var createdMetric = await metricsService.RecordMetricAsync(metric, cancellationToken);

                        logger.LogInformation("Metric recorded: {MetricName} = {Value}", metric.Name, metric.Value);

                        var response = new MetricDto
                        {
                            MetricId = createdMetric.MetricId,
                            Name = createdMetric.Name,
                            Category = createdMetric.Category,
                            Value = createdMetric.Value,
                            Unit = createdMetric.Unit,
                            Tags = createdMetric.Tags,
                            Timestamp = createdMetric.Timestamp
                        };

                        return CreatedAtAction(nameof(GetMetrics), response);
                    }

                    /// <summary>
                    /// Get aggregated metric value.
                    /// </summary>
                    [HttpGet("aggregate")]
                    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<object>> GetAggregatedMetric(
                        [FromQuery] string name,
                        [FromQuery] string aggregation,
                        [FromQuery] DateTime startDate,
                        [FromQuery] DateTime endDate,
                        CancellationToken cancellationToken)
                    {
                        double result = aggregation.ToLowerInvariant() switch
                        {
                            "average" or "avg" => await metricsService.GetAverageAsync(name, startDate, endDate, cancellationToken),
                            "sum" => await metricsService.GetSumAsync(name, startDate, endDate, cancellationToken),
                            "min" => await metricsService.GetMinAsync(name, startDate, endDate, cancellationToken),
                            "max" => await metricsService.GetMaxAsync(name, startDate, endDate, cancellationToken),
                            _ => throw new ArgumentException($"Unknown aggregation type: {aggregation}")
                        };

                        return Ok(new
                        {
                            MetricName = name,
                            Aggregation = aggregation,
                            Value = result,
                            StartDate = startDate,
                            EndDate = endDate
                        });
                    }
                }
                """
        };
    }

    private static FileModel CreateReportsControllerFile(string directory)
    {
        return new FileModel("ReportsController", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Analytics.Core.DTOs;
                using Analytics.Core.Interfaces;
                using Microsoft.AspNetCore.Authorization;
                using Microsoft.AspNetCore.Mvc;

                namespace Analytics.Api.Controllers;

                /// <summary>
                /// Controller for report operations.
                /// </summary>
                [ApiController]
                [Route("api/analytics/reports")]
                [Authorize]
                public class ReportsController : ControllerBase
                {
                    private readonly IReportGenerator reportGenerator;
                    private readonly ILogger<ReportsController> logger;

                    public ReportsController(
                        IReportGenerator reportGenerator,
                        ILogger<ReportsController> logger)
                    {
                        this.reportGenerator = reportGenerator;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Get a report by ID.
                    /// </summary>
                    [HttpGet("{id:guid}")]
                    [ProducesResponseType(typeof(ReportDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<ReportDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var report = await reportGenerator.GetReportByIdAsync(id, cancellationToken);

                        if (report == null)
                        {
                            return NotFound();
                        }

                        return Ok(new ReportDto
                        {
                            ReportId = report.ReportId,
                            Name = report.Name,
                            ReportType = report.ReportType,
                            Description = report.Description,
                            StartDate = report.StartDate,
                            EndDate = report.EndDate,
                            GeneratedBy = report.GeneratedBy,
                            Data = report.Data,
                            Status = report.Status,
                            CreatedAt = report.CreatedAt,
                            CompletedAt = report.CompletedAt
                        });
                    }

                    /// <summary>
                    /// Get all reports.
                    /// </summary>
                    [HttpGet]
                    [ProducesResponseType(typeof(IEnumerable<ReportDto>), StatusCodes.Status200OK)]
                    public async Task<ActionResult<IEnumerable<ReportDto>>> GetAll(
                        [FromQuery] int skip = 0,
                        [FromQuery] int take = 100,
                        CancellationToken cancellationToken = default)
                    {
                        var reports = await reportGenerator.GetAllReportsAsync(skip, take, cancellationToken);

                        var reportDtos = reports.Select(r => new ReportDto
                        {
                            ReportId = r.ReportId,
                            Name = r.Name,
                            ReportType = r.ReportType,
                            Description = r.Description,
                            StartDate = r.StartDate,
                            EndDate = r.EndDate,
                            GeneratedBy = r.GeneratedBy,
                            Data = r.Data,
                            Status = r.Status,
                            CreatedAt = r.CreatedAt,
                            CompletedAt = r.CompletedAt
                        });

                        return Ok(reportDtos);
                    }

                    /// <summary>
                    /// Generate a new report.
                    /// </summary>
                    [HttpPost]
                    [ProducesResponseType(typeof(ReportDto), StatusCodes.Status202Accepted)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<ActionResult<ReportDto>> GenerateReport(
                        [FromBody] GenerateReportRequest request,
                        CancellationToken cancellationToken)
                    {
                        var report = await reportGenerator.GenerateReportAsync(
                            request.Name,
                            request.ReportType,
                            request.StartDate,
                            request.EndDate,
                            request.GeneratedBy,
                            cancellationToken);

                        logger.LogInformation("Report generation initiated: {ReportId}", report.ReportId);

                        var response = new ReportDto
                        {
                            ReportId = report.ReportId,
                            Name = report.Name,
                            ReportType = report.ReportType,
                            Description = report.Description,
                            StartDate = report.StartDate,
                            EndDate = report.EndDate,
                            GeneratedBy = report.GeneratedBy,
                            Data = report.Data,
                            Status = report.Status,
                            CreatedAt = report.CreatedAt,
                            CompletedAt = report.CompletedAt
                        };

                        return AcceptedAtAction(nameof(GetById), new { id = report.ReportId }, response);
                    }
                }

                /// <summary>
                /// Request model for generating a report.
                /// </summary>
                public sealed class GenerateReportRequest
                {
                    public required string Name { get; init; }

                    public required string ReportType { get; init; }

                    public DateTime StartDate { get; init; }

                    public DateTime EndDate { get; init; }

                    public string? GeneratedBy { get; init; }
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
                builder.Services.AddAnalyticsInfrastructure(builder.Configuration);

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
                        Title = "Analytics API",
                        Version = "v1",
                        Description = "Analytics microservice for event tracking, metrics, and reporting"
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
                    "AnalyticsDb": "Server=.\\SQLEXPRESS;Database=AnalyticsDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Jwt": {
                    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
                    "Issuer": "Analytics.Api",
                    "Audience": "Analytics.Api",
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
