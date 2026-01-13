// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.HistoricalTelemetry;

/// <summary>
/// Factory for creating Historical Telemetry microservice artifacts.
/// Manages long-term storage and retrieval of telemetry data.
/// </summary>
public class HistoricalTelemetryArtifactFactory : IHistoricalTelemetryArtifactFactory
{
    private readonly ILogger<HistoricalTelemetryArtifactFactory> logger;

    public HistoricalTelemetryArtifactFactory(ILogger<HistoricalTelemetryArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding HistoricalTelemetry.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");
        var optionsDir = Path.Combine(project.Directory, "Options");

        // Options per REQ-HIST-006
        project.Files.Add(new FileModel("HistoricalTelemetryOptions", optionsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.HistoricalTelemetry.Core.Options;

                /// <summary>
                /// Options for HistoricalTelemetry service per REQ-HIST-006.
                /// </summary>
                public class HistoricalTelemetryOptions
                {
                    public const string SectionName = "HistoricalTelemetry";

                    public string RedisConnectionString { get; set; } = "localhost:6379";
                    public string TelemetryChannel { get; set; } = "telemetry";
                    public int BatchSize { get; set; } = 100;
                    public int BatchIntervalMs { get; set; } = 1000;
                }
                """
        });

        // Entities
        project.Files.Add(new FileModel("HistoricalTelemetryRecord", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.HistoricalTelemetry.Core.Entities;

                /// <summary>
                /// Historical telemetry record with index-optimized queries per REQ-HIST-001.
                /// </summary>
                public class HistoricalTelemetryRecord
                {
                    public Guid RecordId { get; set; }
                    public required string Source { get; set; }
                    public required string MetricName { get; set; }
                    public required string Value { get; set; }
                    public string? Unit { get; set; }
                    public TelemetryType Type { get; set; } = TelemetryType.Metric;
                    public DateTime Timestamp { get; set; }
                    public DateTime StoredAt { get; set; } = DateTime.UtcNow;
                    public string? Tags { get; set; } // JSON serialized tags
                }

                public enum TelemetryType { Metric, Event, Trace, Log }
                """
        });

        project.Files.Add(new FileModel("TelemetryAggregation", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.HistoricalTelemetry.Core.Entities;

                /// <summary>
                /// Pre-computed statistics entity per REQ-HIST-005.
                /// </summary>
                public class TelemetryAggregation
                {
                    public Guid AggregationId { get; set; }
                    public required string Source { get; set; }
                    public required string MetricName { get; set; }
                    public AggregationType AggregationType { get; set; }
                    public decimal MinValue { get; set; }
                    public decimal MaxValue { get; set; }
                    public decimal AverageValue { get; set; }
                    public decimal SumValue { get; set; }
                    public int Count { get; set; }
                    public DateTime PeriodStart { get; set; }
                    public DateTime PeriodEnd { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                }

                public enum AggregationType { Hourly, Daily, Weekly, Monthly }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IHistoricalTelemetryRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.HistoricalTelemetry.Core.Entities;

                namespace EventMonitoring.HistoricalTelemetry.Core.Interfaces;

                public interface IHistoricalTelemetryRepository
                {
                    Task<HistoricalTelemetryRecord> AddAsync(HistoricalTelemetryRecord record, CancellationToken cancellationToken = default);
                    Task AddRangeAsync(IEnumerable<HistoricalTelemetryRecord> records, CancellationToken cancellationToken = default);
                    Task<IEnumerable<HistoricalTelemetryRecord>> GetBySourceAsync(string source, DateTime startTime, DateTime endTime, int page, int pageSize, CancellationToken cancellationToken = default);
                    Task<IEnumerable<HistoricalTelemetryRecord>> GetByMetricAsync(string metricName, DateTime startTime, DateTime endTime, int page, int pageSize, CancellationToken cancellationToken = default);
                    Task<IEnumerable<HistoricalTelemetryRecord>> GetBySourceAndMetricAsync(string source, string metricName, DateTime startTime, DateTime endTime, int page, int pageSize, CancellationToken cancellationToken = default);
                    Task<int> GetCountAsync(string? source, string? metricName, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ITelemetryPersistenceService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.HistoricalTelemetry.Core.Entities;

                namespace EventMonitoring.HistoricalTelemetry.Core.Interfaces;

                public interface ITelemetryPersistenceService
                {
                    Task PersistTelemetryAsync(HistoricalTelemetryRecord record, CancellationToken cancellationToken = default);
                    Task PersistBatchAsync(IEnumerable<HistoricalTelemetryRecord> records, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("TelemetryPersistedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.HistoricalTelemetry.Core.Events;

                public record TelemetryPersistedEvent(Guid RecordId, string Source, string MetricName, DateTime Timestamp, DateTime StoredAt);
                """
        });

        // DTOs
        project.Files.Add(new FileModel("HistoricalTelemetryDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.HistoricalTelemetry.Core.DTOs;

                public class HistoricalTelemetryDto
                {
                    public Guid RecordId { get; set; }
                    public required string Source { get; set; }
                    public required string MetricName { get; set; }
                    public required string Value { get; set; }
                    public string? Unit { get; set; }
                    public DateTime Timestamp { get; set; }
                    public DateTime StoredAt { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("TelemetryQueryRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.HistoricalTelemetry.Core.DTOs;

                public class TelemetryQueryRequest
                {
                    public string? Source { get; set; }
                    public string? MetricName { get; set; }
                    public DateTime StartTime { get; set; }
                    public DateTime EndTime { get; set; }
                    public int Page { get; set; } = 1;
                    public int PageSize { get; set; } = 50;
                }
                """
        });

        project.Files.Add(new FileModel("PagedTelemetryResponse", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.HistoricalTelemetry.Core.DTOs;

                /// <summary>
                /// Paginated response with TotalPages per REQ-HIST-003.
                /// </summary>
                public class PagedTelemetryResponse
                {
                    public IEnumerable<HistoricalTelemetryDto> Data { get; set; } = new List<HistoricalTelemetryDto>();
                    public int Page { get; set; }
                    public int PageSize { get; set; }
                    public int TotalCount { get; set; }
                    public int TotalPages { get; set; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding HistoricalTelemetry.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");
        var backgroundServicesDir = Path.Combine(project.Directory, "BackgroundServices");

        // DbContext
        project.Files.Add(new FileModel("HistoricalTelemetryDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.HistoricalTelemetry.Core.Entities;
                using Microsoft.EntityFrameworkCore;

                namespace EventMonitoring.HistoricalTelemetry.Infrastructure.Data;

                public class HistoricalTelemetryDbContext : DbContext
                {
                    public HistoricalTelemetryDbContext(DbContextOptions<HistoricalTelemetryDbContext> options)
                        : base(options)
                    {
                    }

                    public DbSet<HistoricalTelemetryRecord> TelemetryRecords => Set<HistoricalTelemetryRecord>();
                    public DbSet<TelemetryAggregation> TelemetryAggregations => Set<TelemetryAggregation>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        base.OnModelCreating(modelBuilder);
                        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HistoricalTelemetryDbContext).Assembly);
                    }
                }
                """
        });

        // Entity Configurations with indexes per REQ-HIST-001
        project.Files.Add(new FileModel("HistoricalTelemetryRecordConfiguration", configurationsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.HistoricalTelemetry.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace EventMonitoring.HistoricalTelemetry.Infrastructure.Data.Configurations;

                /// <summary>
                /// Configuration with index-optimized queries for Source, MetricName, Timestamp per REQ-HIST-001.
                /// </summary>
                public class HistoricalTelemetryRecordConfiguration : IEntityTypeConfiguration<HistoricalTelemetryRecord>
                {
                    public void Configure(EntityTypeBuilder<HistoricalTelemetryRecord> builder)
                    {
                        builder.HasKey(x => x.RecordId);
                        builder.Property(x => x.Source).IsRequired().HasMaxLength(200);
                        builder.Property(x => x.MetricName).IsRequired().HasMaxLength(200);
                        builder.Property(x => x.Value).IsRequired();

                        // Index-optimized queries per REQ-HIST-001
                        builder.HasIndex(x => new { x.Source, x.Timestamp });
                        builder.HasIndex(x => new { x.MetricName, x.Timestamp });
                        builder.HasIndex(x => x.Timestamp);
                    }
                }
                """
        });

        project.Files.Add(new FileModel("TelemetryAggregationConfiguration", configurationsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.HistoricalTelemetry.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace EventMonitoring.HistoricalTelemetry.Infrastructure.Data.Configurations;

                public class TelemetryAggregationConfiguration : IEntityTypeConfiguration<TelemetryAggregation>
                {
                    public void Configure(EntityTypeBuilder<TelemetryAggregation> builder)
                    {
                        builder.HasKey(x => x.AggregationId);
                        builder.Property(x => x.Source).IsRequired().HasMaxLength(200);
                        builder.Property(x => x.MetricName).IsRequired().HasMaxLength(200);
                        builder.HasIndex(x => new { x.Source, x.MetricName, x.PeriodStart, x.AggregationType });
                    }
                }
                """
        });

        // Repositories with bulk insert per REQ-HIST-002
        project.Files.Add(new FileModel("HistoricalTelemetryRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.HistoricalTelemetry.Core.Entities;
                using EventMonitoring.HistoricalTelemetry.Core.Interfaces;
                using EventMonitoring.HistoricalTelemetry.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace EventMonitoring.HistoricalTelemetry.Infrastructure.Repositories;

                public class HistoricalTelemetryRepository : IHistoricalTelemetryRepository
                {
                    private readonly HistoricalTelemetryDbContext context;

                    public HistoricalTelemetryRepository(HistoricalTelemetryDbContext context)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                    }

                    public async Task<HistoricalTelemetryRecord> AddAsync(HistoricalTelemetryRecord record, CancellationToken cancellationToken = default)
                    {
                        await context.TelemetryRecords.AddAsync(record, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return record;
                    }

                    /// <summary>
                    /// Bulk insert using AddRangeAsync per REQ-HIST-002.
                    /// </summary>
                    public async Task AddRangeAsync(IEnumerable<HistoricalTelemetryRecord> records, CancellationToken cancellationToken = default)
                    {
                        await context.TelemetryRecords.AddRangeAsync(records, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<HistoricalTelemetryRecord>> GetBySourceAsync(string source, DateTime startTime, DateTime endTime, int page, int pageSize, CancellationToken cancellationToken = default)
                    {
                        return await context.TelemetryRecords
                            .Where(x => x.Source == source && x.Timestamp >= startTime && x.Timestamp <= endTime)
                            .OrderByDescending(x => x.Timestamp)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<HistoricalTelemetryRecord>> GetByMetricAsync(string metricName, DateTime startTime, DateTime endTime, int page, int pageSize, CancellationToken cancellationToken = default)
                    {
                        return await context.TelemetryRecords
                            .Where(x => x.MetricName == metricName && x.Timestamp >= startTime && x.Timestamp <= endTime)
                            .OrderByDescending(x => x.Timestamp)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<IEnumerable<HistoricalTelemetryRecord>> GetBySourceAndMetricAsync(string source, string metricName, DateTime startTime, DateTime endTime, int page, int pageSize, CancellationToken cancellationToken = default)
                    {
                        return await context.TelemetryRecords
                            .Where(x => x.Source == source && x.MetricName == metricName && x.Timestamp >= startTime && x.Timestamp <= endTime)
                            .OrderByDescending(x => x.Timestamp)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<int> GetCountAsync(string? source, string? metricName, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
                    {
                        var query = context.TelemetryRecords.Where(x => x.Timestamp >= startTime && x.Timestamp <= endTime);

                        if (!string.IsNullOrEmpty(source))
                        {
                            query = query.Where(x => x.Source == source);
                        }

                        if (!string.IsNullOrEmpty(metricName))
                        {
                            query = query.Where(x => x.MetricName == metricName);
                        }

                        return await query.CountAsync(cancellationToken);
                    }
                }
                """
        });

        // Services
        project.Files.Add(new FileModel("TelemetryPersistenceService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.HistoricalTelemetry.Core.Entities;
                using EventMonitoring.HistoricalTelemetry.Core.Interfaces;
                using Microsoft.Extensions.Logging;

                namespace EventMonitoring.HistoricalTelemetry.Infrastructure.Services;

                /// <summary>
                /// Service to persist telemetry per REQ-HIST-007.
                /// </summary>
                public class TelemetryPersistenceService : ITelemetryPersistenceService
                {
                    private readonly IHistoricalTelemetryRepository repository;
                    private readonly ILogger<TelemetryPersistenceService> logger;

                    public TelemetryPersistenceService(
                        IHistoricalTelemetryRepository repository,
                        ILogger<TelemetryPersistenceService> logger)
                    {
                        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    public async Task PersistTelemetryAsync(HistoricalTelemetryRecord record, CancellationToken cancellationToken = default)
                    {
                        try
                        {
                            await repository.AddAsync(record, cancellationToken);
                            logger.LogDebug("Persisted telemetry record: {Source}/{MetricName}", record.Source, record.MetricName);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error persisting telemetry record: {Source}/{MetricName}", record.Source, record.MetricName);
                            throw;
                        }
                    }

                    public async Task PersistBatchAsync(IEnumerable<HistoricalTelemetryRecord> records, CancellationToken cancellationToken = default)
                    {
                        try
                        {
                            await repository.AddRangeAsync(records, cancellationToken);
                            logger.LogDebug("Persisted batch of {Count} telemetry records", records.Count());
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error persisting batch of telemetry records");
                            throw;
                        }
                    }
                }
                """
        });

        // Background Service - Redis Pub/Sub listener scaffold per REQ-HIST-004
        project.Files.Add(new FileModel("TelemetryListenerService", backgroundServicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.HistoricalTelemetry.Core.Entities;
                using EventMonitoring.HistoricalTelemetry.Core.Interfaces;
                using EventMonitoring.HistoricalTelemetry.Core.Options;
                using Microsoft.Extensions.Hosting;
                using Microsoft.Extensions.Logging;
                using Microsoft.Extensions.Options;
                using System.Text.Json;

                namespace EventMonitoring.HistoricalTelemetry.Infrastructure.BackgroundServices;

                /// <summary>
                /// Background listener scaffold for Redis Pub/Sub per REQ-HIST-004.
                /// Persists telemetry received via message broker to database per REQ-HIST-007.
                /// </summary>
                public class TelemetryListenerService : BackgroundService
                {
                    private readonly ILogger<TelemetryListenerService> logger;
                    private readonly ITelemetryPersistenceService persistenceService;
                    private readonly HistoricalTelemetryOptions options;

                    public TelemetryListenerService(
                        ILogger<TelemetryListenerService> logger,
                        ITelemetryPersistenceService persistenceService,
                        IOptions<HistoricalTelemetryOptions> options)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.persistenceService = persistenceService ?? throw new ArgumentNullException(nameof(persistenceService));
                        this.options = options?.Value ?? new HistoricalTelemetryOptions();
                    }

                    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
                    {
                        logger.LogInformation("Telemetry Listener Service is starting. Channel: {Channel}, Redis: {Redis}",
                            options.TelemetryChannel, options.RedisConnectionString);

                        // This is a scaffold for Redis Pub/Sub listener per REQ-HIST-004
                        // In a real implementation, this would subscribe to a Redis channel
                        // and persist incoming telemetry messages to the database per REQ-HIST-007

                        while (!stoppingToken.IsCancellationRequested)
                        {
                            try
                            {
                                // Scaffold: In production, replace with Redis Pub/Sub subscription:
                                // var redis = ConnectionMultiplexer.Connect(options.RedisConnectionString);
                                // var subscriber = redis.GetSubscriber();
                                // await subscriber.SubscribeAsync(options.TelemetryChannel, async (channel, message) => {
                                //     var record = JsonSerializer.Deserialize<HistoricalTelemetryRecord>(message);
                                //     await persistenceService.PersistTelemetryAsync(record, stoppingToken);
                                // });

                                await Task.Delay(options.BatchIntervalMs, stoppingToken);
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error occurred while listening for telemetry messages");
                                await Task.Delay(1000, stoppingToken);
                            }
                        }

                        logger.LogInformation("Telemetry Listener Service is stopping");
                    }
                }
                """
        });

        // ConfigureServices using Options pattern per REQ-HIST-006
        project.Files.Add(new FileModel("ConfigureServices", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.HistoricalTelemetry.Core.Interfaces;
                using EventMonitoring.HistoricalTelemetry.Core.Options;
                using EventMonitoring.HistoricalTelemetry.Infrastructure.BackgroundServices;
                using EventMonitoring.HistoricalTelemetry.Infrastructure.Data;
                using EventMonitoring.HistoricalTelemetry.Infrastructure.Repositories;
                using EventMonitoring.HistoricalTelemetry.Infrastructure.Services;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;
                using Microsoft.Extensions.DependencyInjection;

                namespace EventMonitoring.HistoricalTelemetry.Infrastructure;

                public static class ConfigureServices
                {
                    /// <summary>
                    /// Configure services using Options pattern per REQ-HIST-006.
                    /// </summary>
                    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
                    {
                        // Options pattern per REQ-HIST-006
                        services.Configure<HistoricalTelemetryOptions>(
                            configuration.GetSection(HistoricalTelemetryOptions.SectionName));

                        // Database connection using Options pattern
                        var connectionString = configuration.GetConnectionString("DefaultConnection");
                        services.AddDbContext<HistoricalTelemetryDbContext>(options =>
                            options.UseSqlServer(connectionString));

                        services.AddScoped<IHistoricalTelemetryRepository, HistoricalTelemetryRepository>();
                        services.AddScoped<ITelemetryPersistenceService, TelemetryPersistenceService>();
                        services.AddHostedService<TelemetryListenerService>();

                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding HistoricalTelemetry.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // HistoricalTelemetry controller with paginated queries per REQ-HIST-003
        project.Files.Add(new FileModel("HistoricalTelemetryController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.HistoricalTelemetry.Core.DTOs;
                using EventMonitoring.HistoricalTelemetry.Core.Interfaces;
                using Microsoft.AspNetCore.Mvc;

                namespace EventMonitoring.HistoricalTelemetry.Api.Controllers;

                /// <summary>
                /// Controller with paginated queries supporting time ranges and filters per REQ-HIST-003.
                /// </summary>
                [ApiController]
                [Route("api/[controller]")]
                public class HistoricalTelemetryController : ControllerBase
                {
                    private readonly IHistoricalTelemetryRepository repository;
                    private readonly ILogger<HistoricalTelemetryController> logger;

                    public HistoricalTelemetryController(
                        IHistoricalTelemetryRepository repository,
                        ILogger<HistoricalTelemetryController> logger)
                    {
                        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    /// <summary>
                    /// Query historical telemetry with pagination per REQ-HIST-003.
                    /// </summary>
                    [HttpGet]
                    public async Task<ActionResult<PagedTelemetryResponse>> Query([FromQuery] TelemetryQueryRequest request, CancellationToken cancellationToken)
                    {
                        logger.LogInformation("Querying historical telemetry: Source={Source}, Metric={MetricName}, Page={Page}",
                            request.Source, request.MetricName, request.Page);

                        IEnumerable<Core.Entities.HistoricalTelemetryRecord> records;

                        if (!string.IsNullOrEmpty(request.Source) && !string.IsNullOrEmpty(request.MetricName))
                        {
                            records = await repository.GetBySourceAndMetricAsync(
                                request.Source,
                                request.MetricName,
                                request.StartTime,
                                request.EndTime,
                                request.Page,
                                request.PageSize,
                                cancellationToken);
                        }
                        else if (!string.IsNullOrEmpty(request.Source))
                        {
                            records = await repository.GetBySourceAsync(
                                request.Source,
                                request.StartTime,
                                request.EndTime,
                                request.Page,
                                request.PageSize,
                                cancellationToken);
                        }
                        else if (!string.IsNullOrEmpty(request.MetricName))
                        {
                            records = await repository.GetByMetricAsync(
                                request.MetricName,
                                request.StartTime,
                                request.EndTime,
                                request.Page,
                                request.PageSize,
                                cancellationToken);
                        }
                        else
                        {
                            return BadRequest("Either Source or MetricName must be specified");
                        }

                        var totalCount = await repository.GetCountAsync(
                            request.Source,
                            request.MetricName,
                            request.StartTime,
                            request.EndTime,
                            cancellationToken);

                        var dtos = records.Select(r => new HistoricalTelemetryDto
                        {
                            RecordId = r.RecordId,
                            Source = r.Source,
                            MetricName = r.MetricName,
                            Value = r.Value,
                            Unit = r.Unit,
                            Timestamp = r.Timestamp,
                            StoredAt = r.StoredAt
                        });

                        var response = new PagedTelemetryResponse
                        {
                            Data = dtos,
                            Page = request.Page,
                            PageSize = request.PageSize,
                            TotalCount = totalCount,
                            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
                        };

                        return Ok(response);
                    }
                }
                """
        });

        // appsettings.json with SQL Express connection string and Options
        project.Files.Add(new FileModel("appsettings", project.Directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=HistoricalTelemetry;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
                  },
                  "HistoricalTelemetry": {
                    "RedisConnectionString": "localhost:6379",
                    "TelemetryChannel": "telemetry",
                    "BatchSize": 100,
                    "BatchIntervalMs": 1000
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information",
                      "Microsoft.AspNetCore": "Warning",
                      "Microsoft.EntityFrameworkCore": "Warning"
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        });

        // Program.cs
        project.Files.Add(new FileModel("Program", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.HistoricalTelemetry.Infrastructure;
                using EventMonitoring.HistoricalTelemetry.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddInfrastructureServices(builder.Configuration);

                var app = builder.Build();

                // Apply migrations
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<HistoricalTelemetryDbContext>();
                    dbContext.Database.Migrate();
                }

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
                """
        });
    }
}
