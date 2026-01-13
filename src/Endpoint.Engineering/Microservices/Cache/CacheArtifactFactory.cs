// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Cache;

public class CacheArtifactFactory : ICacheArtifactFactory
{
    private readonly ILogger<CacheArtifactFactory> logger;

    public CacheArtifactFactory(ILogger<CacheArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Cache.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("CacheEntry", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Cache.Core.Entities;

                public class CacheEntry
                {
                    public Guid EntryId { get; set; }
                    public required string Key { get; set; }
                    public required string Value { get; set; }
                    public string? ContentType { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? ExpiresAt { get; set; }
                    public DateTime? LastAccessedAt { get; set; }
                    public long SizeBytes { get; set; }
                    public Dictionary<string, string> Tags { get; set; } = new();
                }
                """
        });

        project.Files.Add(new FileModel("CacheStatistics", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Cache.Core.Entities;

                public class CacheStatistics
                {
                    public Guid StatisticsId { get; set; }
                    public long TotalEntries { get; set; }
                    public long TotalSizeBytes { get; set; }
                    public long HitCount { get; set; }
                    public long MissCount { get; set; }
                    public double HitRatio => HitCount + MissCount > 0 ? (double)HitCount / (HitCount + MissCount) : 0;
                    public long EvictionCount { get; set; }
                    public DateTime LastResetAt { get; set; } = DateTime.UtcNow;
                    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("ICacheService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Cache.Core.Entities;

                namespace Cache.Core.Interfaces;

                public interface ICacheService
                {
                    Task<CacheEntry?> GetAsync(string key, CancellationToken cancellationToken = default);
                    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
                    Task SetAsync(string key, string value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
                    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
                    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
                    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);
                    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ICacheInvalidation", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Cache.Core.Interfaces;

                public interface ICacheInvalidation
                {
                    Task InvalidateAsync(string key, CancellationToken cancellationToken = default);
                    Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default);
                    Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default);
                    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("CacheUpdatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Cache.Core.Events;

                public sealed class CacheUpdatedEvent
                {
                    public required string Key { get; init; }
                    public string? OldValue { get; init; }
                    public string? NewValue { get; init; }
                    public DateTime? ExpiresAt { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("CacheInvalidatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Cache.Core.Events;

                public sealed class CacheInvalidatedEvent
                {
                    public required string Key { get; init; }
                    public string? Pattern { get; init; }
                    public string? Tag { get; init; }
                    public InvalidationReason Reason { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }

                public enum InvalidationReason { Manual, Expiration, Eviction, PatternMatch, TagMatch }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("CacheEntryDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Cache.Core.DTOs;

                public sealed class CacheEntryDto
                {
                    public required string Key { get; init; }
                    public required string Value { get; init; }
                    public string? ContentType { get; init; }
                    public DateTime CreatedAt { get; init; }
                    public DateTime? ExpiresAt { get; init; }
                    public long SizeBytes { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("SetCacheRequestDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Cache.Core.DTOs;

                public sealed class SetCacheRequestDto
                {
                    [Required]
                    public required string Value { get; init; }

                    public string? ContentType { get; init; }

                    public int? ExpirationSeconds { get; init; }

                    public Dictionary<string, string>? Tags { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CacheStatisticsDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Cache.Core.DTOs;

                public sealed class CacheStatisticsDto
                {
                    public long TotalEntries { get; init; }
                    public long TotalSizeBytes { get; init; }
                    public long HitCount { get; init; }
                    public long MissCount { get; init; }
                    public double HitRatio { get; init; }
                    public long EvictionCount { get; init; }
                    public DateTime CollectedAt { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Cache.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("CacheDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Cache.Core.Entities;

                namespace Cache.Infrastructure.Data;

                public class CacheDbContext : DbContext
                {
                    public CacheDbContext(DbContextOptions<CacheDbContext> options) : base(options) { }

                    public DbSet<CacheEntry> CacheEntries => Set<CacheEntry>();
                    public DbSet<CacheStatistics> CacheStatistics => Set<CacheStatistics>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<CacheEntry>(entity =>
                        {
                            entity.HasKey(e => e.EntryId);
                            entity.Property(e => e.Key).IsRequired().HasMaxLength(500);
                            entity.Property(e => e.Value).IsRequired();
                            entity.Property(e => e.ContentType).HasMaxLength(100);
                            entity.HasIndex(e => e.Key).IsUnique();
                            entity.HasIndex(e => e.ExpiresAt);
                        });

                        modelBuilder.Entity<CacheStatistics>(entity =>
                        {
                            entity.HasKey(s => s.StatisticsId);
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("CacheRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Cache.Core.Entities;
                using Cache.Infrastructure.Data;

                namespace Cache.Infrastructure.Repositories;

                public class CacheRepository
                {
                    private readonly CacheDbContext context;

                    public CacheRepository(CacheDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<CacheEntry?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
                        => await context.CacheEntries.FirstOrDefaultAsync(e => e.Key == key, cancellationToken);

                    public async Task<CacheEntry> CreateAsync(CacheEntry entry, CancellationToken cancellationToken = default)
                    {
                        entry.EntryId = Guid.NewGuid();
                        await context.CacheEntries.AddAsync(entry, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return entry;
                    }

                    public async Task UpdateAsync(CacheEntry entry, CancellationToken cancellationToken = default)
                    {
                        context.CacheEntries.Update(entry);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task<bool> DeleteByKeyAsync(string key, CancellationToken cancellationToken = default)
                    {
                        var entry = await context.CacheEntries.FirstOrDefaultAsync(e => e.Key == key, cancellationToken);
                        if (entry == null) return false;

                        context.CacheEntries.Remove(entry);
                        await context.SaveChangesAsync(cancellationToken);
                        return true;
                    }

                    public async Task<int> DeleteExpiredAsync(CancellationToken cancellationToken = default)
                    {
                        var now = DateTime.UtcNow;
                        var expired = await context.CacheEntries
                            .Where(e => e.ExpiresAt != null && e.ExpiresAt < now)
                            .ToListAsync(cancellationToken);

                        context.CacheEntries.RemoveRange(expired);
                        await context.SaveChangesAsync(cancellationToken);
                        return expired.Count;
                    }

                    public async Task<int> DeleteByPatternAsync(string pattern, CancellationToken cancellationToken = default)
                    {
                        var entries = await context.CacheEntries
                            .Where(e => EF.Functions.Like(e.Key, pattern))
                            .ToListAsync(cancellationToken);

                        context.CacheEntries.RemoveRange(entries);
                        await context.SaveChangesAsync(cancellationToken);
                        return entries.Count;
                    }

                    public async Task<int> DeleteAllAsync(CancellationToken cancellationToken = default)
                    {
                        var count = await context.CacheEntries.CountAsync(cancellationToken);
                        context.CacheEntries.RemoveRange(context.CacheEntries);
                        await context.SaveChangesAsync(cancellationToken);
                        return count;
                    }
                }
                """
        });

        project.Files.Add(new FileModel("CacheService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Text.Json;
                using Microsoft.Extensions.Logging;
                using Cache.Core.Entities;
                using Cache.Core.Interfaces;
                using Cache.Infrastructure.Repositories;

                namespace Cache.Infrastructure.Services;

                public class CacheService : ICacheService
                {
                    private readonly CacheRepository repository;
                    private readonly ILogger<CacheService> logger;
                    private long hitCount;
                    private long missCount;

                    public CacheService(CacheRepository repository, ILogger<CacheService> logger)
                    {
                        this.repository = repository;
                        this.logger = logger;
                    }

                    public async Task<CacheEntry?> GetAsync(string key, CancellationToken cancellationToken = default)
                    {
                        var entry = await repository.GetByKeyAsync(key, cancellationToken);

                        if (entry == null)
                        {
                            Interlocked.Increment(ref missCount);
                            logger.LogDebug("Cache miss for key: {Key}", key);
                            return null;
                        }

                        if (entry.ExpiresAt.HasValue && entry.ExpiresAt < DateTime.UtcNow)
                        {
                            await repository.DeleteByKeyAsync(key, cancellationToken);
                            Interlocked.Increment(ref missCount);
                            logger.LogDebug("Cache entry expired for key: {Key}", key);
                            return null;
                        }

                        Interlocked.Increment(ref hitCount);
                        entry.LastAccessedAt = DateTime.UtcNow;
                        await repository.UpdateAsync(entry, cancellationToken);
                        logger.LogDebug("Cache hit for key: {Key}", key);
                        return entry;
                    }

                    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
                    {
                        var entry = await GetAsync(key, cancellationToken);
                        if (entry == null) return null;

                        return JsonSerializer.Deserialize<T>(entry.Value);
                    }

                    public async Task SetAsync(string key, string value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
                    {
                        var existingEntry = await repository.GetByKeyAsync(key, cancellationToken);

                        if (existingEntry != null)
                        {
                            existingEntry.Value = value;
                            existingEntry.ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null;
                            existingEntry.SizeBytes = System.Text.Encoding.UTF8.GetByteCount(value);
                            await repository.UpdateAsync(existingEntry, cancellationToken);
                        }
                        else
                        {
                            var entry = new CacheEntry
                            {
                                Key = key,
                                Value = value,
                                ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null,
                                SizeBytes = System.Text.Encoding.UTF8.GetByteCount(value)
                            };
                            await repository.CreateAsync(entry, cancellationToken);
                        }

                        logger.LogDebug("Cache set for key: {Key}", key);
                    }

                    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
                    {
                        var json = JsonSerializer.Serialize(value);
                        await SetAsync(key, json, expiration, cancellationToken);
                    }

                    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
                    {
                        var entry = await repository.GetByKeyAsync(key, cancellationToken);
                        return entry != null && (!entry.ExpiresAt.HasValue || entry.ExpiresAt >= DateTime.UtcNow);
                    }

                    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
                    {
                        var result = await repository.DeleteByKeyAsync(key, cancellationToken);
                        logger.LogDebug("Cache delete for key: {Key}, result: {Result}", key, result);
                        return result;
                    }

                    public Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
                    {
                        var stats = new CacheStatistics
                        {
                            StatisticsId = Guid.NewGuid(),
                            HitCount = Interlocked.Read(ref hitCount),
                            MissCount = Interlocked.Read(ref missCount),
                            CollectedAt = DateTime.UtcNow
                        };
                        return Task.FromResult(stats);
                    }
                }
                """
        });

        project.Files.Add(new FileModel("CacheInvalidationService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.Extensions.Logging;
                using Cache.Core.Interfaces;
                using Cache.Infrastructure.Repositories;

                namespace Cache.Infrastructure.Services;

                public class CacheInvalidationService : ICacheInvalidation
                {
                    private readonly CacheRepository repository;
                    private readonly ILogger<CacheInvalidationService> logger;

                    public CacheInvalidationService(CacheRepository repository, ILogger<CacheInvalidationService> logger)
                    {
                        this.repository = repository;
                        this.logger = logger;
                    }

                    public async Task InvalidateAsync(string key, CancellationToken cancellationToken = default)
                    {
                        await repository.DeleteByKeyAsync(key, cancellationToken);
                        logger.LogInformation("Invalidated cache entry: {Key}", key);
                    }

                    public async Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default)
                    {
                        var count = await repository.DeleteByPatternAsync(pattern, cancellationToken);
                        logger.LogInformation("Invalidated {Count} cache entries matching pattern: {Pattern}", count, pattern);
                    }

                    public Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default)
                    {
                        // Tag-based invalidation requires additional implementation
                        logger.LogWarning("Tag-based invalidation not yet implemented for tag: {Tag}", tag);
                        return Task.CompletedTask;
                    }

                    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
                    {
                        var count = await repository.DeleteAllAsync(cancellationToken);
                        logger.LogInformation("Invalidated all {Count} cache entries", count);
                    }
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
                using Cache.Core.Interfaces;
                using Cache.Infrastructure.Data;
                using Cache.Infrastructure.Repositories;
                using Cache.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddCacheInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<CacheDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("CacheDb") ??
                                @"Server=.\SQLEXPRESS;Database=CacheDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<CacheRepository>();
                        services.AddScoped<ICacheService, CacheService>();
                        services.AddScoped<ICacheInvalidation, CacheInvalidationService>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Cache.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("CacheController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Cache.Core.DTOs;
                using Cache.Core.Interfaces;

                namespace Cache.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class CacheController : ControllerBase
                {
                    private readonly ICacheService cacheService;
                    private readonly ICacheInvalidation cacheInvalidation;
                    private readonly ILogger<CacheController> logger;

                    public CacheController(
                        ICacheService cacheService,
                        ICacheInvalidation cacheInvalidation,
                        ILogger<CacheController> logger)
                    {
                        this.cacheService = cacheService;
                        this.cacheInvalidation = cacheInvalidation;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Get a cache entry by key.
                    /// </summary>
                    [HttpGet("{key}")]
                    [ProducesResponseType(typeof(CacheEntryDto), StatusCodes.Status200OK)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<ActionResult<CacheEntryDto>> Get(string key, CancellationToken cancellationToken)
                    {
                        var entry = await cacheService.GetAsync(key, cancellationToken);

                        if (entry == null)
                        {
                            return NotFound();
                        }

                        return Ok(new CacheEntryDto
                        {
                            Key = entry.Key,
                            Value = entry.Value,
                            ContentType = entry.ContentType,
                            CreatedAt = entry.CreatedAt,
                            ExpiresAt = entry.ExpiresAt,
                            SizeBytes = entry.SizeBytes
                        });
                    }

                    /// <summary>
                    /// Set a cache entry.
                    /// </summary>
                    [HttpPut("{key}")]
                    [ProducesResponseType(StatusCodes.Status204NoContent)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<IActionResult> Set(string key, [FromBody] SetCacheRequestDto request, CancellationToken cancellationToken)
                    {
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            return BadRequest("Key is required");
                        }

                        TimeSpan? expiration = request.ExpirationSeconds.HasValue
                            ? TimeSpan.FromSeconds(request.ExpirationSeconds.Value)
                            : null;

                        await cacheService.SetAsync(key, request.Value, expiration, cancellationToken);
                        logger.LogInformation("Cache entry set for key: {Key}", key);

                        return NoContent();
                    }

                    /// <summary>
                    /// Delete a cache entry.
                    /// </summary>
                    [HttpDelete("{key}")]
                    [ProducesResponseType(StatusCodes.Status204NoContent)]
                    [ProducesResponseType(StatusCodes.Status404NotFound)]
                    public async Task<IActionResult> Delete(string key, CancellationToken cancellationToken)
                    {
                        var deleted = await cacheService.DeleteAsync(key, cancellationToken);

                        if (!deleted)
                        {
                            return NotFound();
                        }

                        logger.LogInformation("Cache entry deleted for key: {Key}", key);
                        return NoContent();
                    }

                    /// <summary>
                    /// Get cache statistics.
                    /// </summary>
                    [HttpGet("statistics")]
                    [ProducesResponseType(typeof(CacheStatisticsDto), StatusCodes.Status200OK)]
                    public async Task<ActionResult<CacheStatisticsDto>> GetStatistics(CancellationToken cancellationToken)
                    {
                        var stats = await cacheService.GetStatisticsAsync(cancellationToken);

                        return Ok(new CacheStatisticsDto
                        {
                            TotalEntries = stats.TotalEntries,
                            TotalSizeBytes = stats.TotalSizeBytes,
                            HitCount = stats.HitCount,
                            MissCount = stats.MissCount,
                            HitRatio = stats.HitRatio,
                            EvictionCount = stats.EvictionCount,
                            CollectedAt = stats.CollectedAt
                        });
                    }

                    /// <summary>
                    /// Invalidate cache entries by pattern.
                    /// </summary>
                    [HttpPost("invalidate")]
                    [ProducesResponseType(StatusCodes.Status204NoContent)]
                    public async Task<IActionResult> Invalidate([FromQuery] string? pattern, [FromQuery] string? tag, CancellationToken cancellationToken)
                    {
                        if (!string.IsNullOrWhiteSpace(pattern))
                        {
                            await cacheInvalidation.InvalidateByPatternAsync(pattern, cancellationToken);
                        }
                        else if (!string.IsNullOrWhiteSpace(tag))
                        {
                            await cacheInvalidation.InvalidateByTagAsync(tag, cancellationToken);
                        }
                        else
                        {
                            await cacheInvalidation.InvalidateAllAsync(cancellationToken);
                        }

                        return NoContent();
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

                builder.Services.AddCacheInfrastructure(builder.Configuration);
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
                    "CacheDb": "Server=.\\SQLEXPRESS;Database=CacheDb;Trusted_Connection=True;TrustServerCertificate=True"
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
