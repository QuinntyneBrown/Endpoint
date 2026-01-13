// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.RateLimiting;

public class RateLimitingArtifactFactory : IRateLimitingArtifactFactory
{
    private readonly ILogger<RateLimitingArtifactFactory> logger;

    public RateLimitingArtifactFactory(ILogger<RateLimitingArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding RateLimiting.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("RateLimitRule", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RateLimiting.Core.Entities;

                public class RateLimitRule
                {
                    public Guid RateLimitRuleId { get; set; }
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public required string Endpoint { get; set; }
                    public int RequestLimit { get; set; }
                    public int TimeWindowSeconds { get; set; }
                    public RateLimitScope Scope { get; set; } = RateLimitScope.PerUser;
                    public bool IsEnabled { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                }

                public enum RateLimitScope { Global, PerTenant, PerUser, PerIpAddress }
                """
        });

        project.Files.Add(new FileModel("QuotaUsage", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RateLimiting.Core.Entities;

                public class QuotaUsage
                {
                    public Guid QuotaUsageId { get; set; }
                    public Guid RateLimitRuleId { get; set; }
                    public RateLimitRule RateLimitRule { get; set; } = null!;
                    public required string Identifier { get; set; }
                    public int RequestCount { get; set; }
                    public DateTime WindowStart { get; set; }
                    public DateTime WindowEnd { get; set; }
                    public DateTime LastRequestAt { get; set; } = DateTime.UtcNow;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IRateLimitRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using RateLimiting.Core.Entities;

                namespace RateLimiting.Core.Interfaces;

                public interface IRateLimitRepository
                {
                    Task<RateLimitRule?> GetByIdAsync(Guid ruleId, CancellationToken cancellationToken = default);
                    Task<RateLimitRule?> GetByEndpointAsync(string endpoint, CancellationToken cancellationToken = default);
                    Task<IEnumerable<RateLimitRule>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<IEnumerable<RateLimitRule>> GetEnabledRulesAsync(CancellationToken cancellationToken = default);
                    Task<RateLimitRule> AddAsync(RateLimitRule rule, CancellationToken cancellationToken = default);
                    Task UpdateAsync(RateLimitRule rule, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid ruleId, CancellationToken cancellationToken = default);
                    Task<QuotaUsage?> GetQuotaUsageAsync(Guid ruleId, string identifier, CancellationToken cancellationToken = default);
                    Task<QuotaUsage> AddOrUpdateQuotaUsageAsync(QuotaUsage usage, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IRateLimitService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using RateLimiting.Core.Entities;

                namespace RateLimiting.Core.Interfaces;

                public interface IRateLimitService
                {
                    Task<RateLimitResult> CheckRateLimitAsync(string endpoint, string identifier, CancellationToken cancellationToken = default);
                    Task<bool> IsRateLimitedAsync(string endpoint, string identifier, CancellationToken cancellationToken = default);
                    Task IncrementRequestCountAsync(string endpoint, string identifier, CancellationToken cancellationToken = default);
                    Task ResetQuotaAsync(Guid ruleId, string identifier, CancellationToken cancellationToken = default);
                    Task<RateLimitStatus> GetStatusAsync(string endpoint, string identifier, CancellationToken cancellationToken = default);
                }

                public class RateLimitResult
                {
                    public bool IsAllowed { get; init; }
                    public int RemainingRequests { get; init; }
                    public int RequestLimit { get; init; }
                    public DateTime? ResetTime { get; init; }
                    public string? RetryAfterSeconds { get; init; }
                }

                public class RateLimitStatus
                {
                    public required string Endpoint { get; init; }
                    public required string Identifier { get; init; }
                    public int CurrentCount { get; init; }
                    public int Limit { get; init; }
                    public int Remaining { get; init; }
                    public DateTime WindowStart { get; init; }
                    public DateTime WindowEnd { get; init; }
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("RateLimitExceededEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RateLimiting.Core.Events;

                public sealed class RateLimitExceededEvent
                {
                    public Guid RateLimitRuleId { get; init; }
                    public required string Endpoint { get; init; }
                    public required string Identifier { get; init; }
                    public int RequestCount { get; init; }
                    public int RequestLimit { get; init; }
                    public DateTime WindowStart { get; init; }
                    public DateTime WindowEnd { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("QuotaResetEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RateLimiting.Core.Events;

                public sealed class QuotaResetEvent
                {
                    public Guid RateLimitRuleId { get; init; }
                    public required string Identifier { get; init; }
                    public int PreviousRequestCount { get; init; }
                    public DateTime PreviousWindowEnd { get; init; }
                    public DateTime NewWindowStart { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("RateLimitStatusDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RateLimiting.Core.DTOs;

                public sealed class RateLimitStatusDto
                {
                    public required string Endpoint { get; init; }
                    public required string Identifier { get; init; }
                    public int CurrentCount { get; init; }
                    public int Limit { get; init; }
                    public int Remaining { get; init; }
                    public DateTime ResetAt { get; init; }
                    public bool IsLimited { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("QuotaDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RateLimiting.Core.DTOs;

                public sealed class QuotaDto
                {
                    public Guid RuleId { get; init; }
                    public required string RuleName { get; init; }
                    public required string Endpoint { get; init; }
                    public required string Identifier { get; init; }
                    public int Used { get; init; }
                    public int Limit { get; init; }
                    public int Remaining { get; init; }
                    public double UsagePercentage { get; init; }
                    public DateTime WindowStart { get; init; }
                    public DateTime WindowEnd { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateRateLimitRuleRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace RateLimiting.Core.DTOs;

                public sealed class CreateRateLimitRuleRequest
                {
                    [Required]
                    [StringLength(100)]
                    public required string Name { get; init; }

                    [StringLength(500)]
                    public string? Description { get; init; }

                    [Required]
                    [StringLength(500)]
                    public required string Endpoint { get; init; }

                    [Required]
                    [Range(1, int.MaxValue, ErrorMessage = "Request limit must be at least 1")]
                    public int RequestLimit { get; init; }

                    [Required]
                    [Range(1, 86400, ErrorMessage = "Time window must be between 1 and 86400 seconds")]
                    public int TimeWindowSeconds { get; init; }

                    public string Scope { get; init; } = "PerUser";
                }
                """
        });

        project.Files.Add(new FileModel("RateLimitRuleDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace RateLimiting.Core.DTOs;

                public sealed class RateLimitRuleDto
                {
                    public Guid RateLimitRuleId { get; init; }
                    public required string Name { get; init; }
                    public string? Description { get; init; }
                    public required string Endpoint { get; init; }
                    public int RequestLimit { get; init; }
                    public int TimeWindowSeconds { get; init; }
                    public string Scope { get; init; } = "PerUser";
                    public bool IsEnabled { get; init; }
                    public DateTime CreatedAt { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding RateLimiting.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("RateLimitingDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using RateLimiting.Core.Entities;

                namespace RateLimiting.Infrastructure.Data;

                public class RateLimitingDbContext : DbContext
                {
                    public RateLimitingDbContext(DbContextOptions<RateLimitingDbContext> options) : base(options) { }

                    public DbSet<RateLimitRule> RateLimitRules => Set<RateLimitRule>();
                    public DbSet<QuotaUsage> QuotaUsages => Set<QuotaUsage>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<RateLimitRule>(entity =>
                        {
                            entity.HasKey(r => r.RateLimitRuleId);
                            entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
                            entity.Property(r => r.Endpoint).IsRequired().HasMaxLength(500);
                            entity.Property(r => r.Description).HasMaxLength(500);
                            entity.HasIndex(r => r.Endpoint);
                            entity.HasIndex(r => r.IsEnabled);
                        });

                        modelBuilder.Entity<QuotaUsage>(entity =>
                        {
                            entity.HasKey(q => q.QuotaUsageId);
                            entity.Property(q => q.Identifier).IsRequired().HasMaxLength(500);
                            entity.HasOne(q => q.RateLimitRule).WithMany().HasForeignKey(q => q.RateLimitRuleId);
                            entity.HasIndex(q => new { q.RateLimitRuleId, q.Identifier }).IsUnique();
                            entity.HasIndex(q => q.WindowEnd);
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("RateLimitRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using RateLimiting.Core.Entities;
                using RateLimiting.Core.Interfaces;
                using RateLimiting.Infrastructure.Data;

                namespace RateLimiting.Infrastructure.Repositories;

                public class RateLimitRepository : IRateLimitRepository
                {
                    private readonly RateLimitingDbContext context;

                    public RateLimitRepository(RateLimitingDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<RateLimitRule?> GetByIdAsync(Guid ruleId, CancellationToken cancellationToken = default)
                        => await context.RateLimitRules.FirstOrDefaultAsync(r => r.RateLimitRuleId == ruleId, cancellationToken);

                    public async Task<RateLimitRule?> GetByEndpointAsync(string endpoint, CancellationToken cancellationToken = default)
                        => await context.RateLimitRules.FirstOrDefaultAsync(r => r.Endpoint == endpoint && r.IsEnabled, cancellationToken);

                    public async Task<IEnumerable<RateLimitRule>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.RateLimitRules.ToListAsync(cancellationToken);

                    public async Task<IEnumerable<RateLimitRule>> GetEnabledRulesAsync(CancellationToken cancellationToken = default)
                        => await context.RateLimitRules.Where(r => r.IsEnabled).ToListAsync(cancellationToken);

                    public async Task<RateLimitRule> AddAsync(RateLimitRule rule, CancellationToken cancellationToken = default)
                    {
                        rule.RateLimitRuleId = Guid.NewGuid();
                        await context.RateLimitRules.AddAsync(rule, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return rule;
                    }

                    public async Task UpdateAsync(RateLimitRule rule, CancellationToken cancellationToken = default)
                    {
                        rule.UpdatedAt = DateTime.UtcNow;
                        context.RateLimitRules.Update(rule);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid ruleId, CancellationToken cancellationToken = default)
                    {
                        var rule = await context.RateLimitRules.FindAsync(new object[] { ruleId }, cancellationToken);
                        if (rule != null)
                        {
                            context.RateLimitRules.Remove(rule);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task<QuotaUsage?> GetQuotaUsageAsync(Guid ruleId, string identifier, CancellationToken cancellationToken = default)
                        => await context.QuotaUsages
                            .Include(q => q.RateLimitRule)
                            .FirstOrDefaultAsync(q => q.RateLimitRuleId == ruleId && q.Identifier == identifier, cancellationToken);

                    public async Task<QuotaUsage> AddOrUpdateQuotaUsageAsync(QuotaUsage usage, CancellationToken cancellationToken = default)
                    {
                        var existing = await GetQuotaUsageAsync(usage.RateLimitRuleId, usage.Identifier, cancellationToken);
                        if (existing == null)
                        {
                            usage.QuotaUsageId = Guid.NewGuid();
                            await context.QuotaUsages.AddAsync(usage, cancellationToken);
                        }
                        else
                        {
                            existing.RequestCount = usage.RequestCount;
                            existing.WindowStart = usage.WindowStart;
                            existing.WindowEnd = usage.WindowEnd;
                            existing.LastRequestAt = usage.LastRequestAt;
                            context.QuotaUsages.Update(existing);
                        }
                        await context.SaveChangesAsync(cancellationToken);
                        return existing ?? usage;
                    }
                }
                """
        });

        project.Files.Add(new FileModel("RateLimitService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using RateLimiting.Core.Entities;
                using RateLimiting.Core.Interfaces;

                namespace RateLimiting.Infrastructure.Services;

                public class RateLimitService : IRateLimitService
                {
                    private readonly IRateLimitRepository repository;

                    public RateLimitService(IRateLimitRepository repository)
                    {
                        this.repository = repository;
                    }

                    public async Task<RateLimitResult> CheckRateLimitAsync(string endpoint, string identifier, CancellationToken cancellationToken = default)
                    {
                        var rule = await repository.GetByEndpointAsync(endpoint, cancellationToken);
                        if (rule == null)
                        {
                            return new RateLimitResult { IsAllowed = true, RemainingRequests = int.MaxValue, RequestLimit = int.MaxValue };
                        }

                        var usage = await repository.GetQuotaUsageAsync(rule.RateLimitRuleId, identifier, cancellationToken);
                        var now = DateTime.UtcNow;

                        if (usage == null || usage.WindowEnd < now)
                        {
                            return new RateLimitResult
                            {
                                IsAllowed = true,
                                RemainingRequests = rule.RequestLimit - 1,
                                RequestLimit = rule.RequestLimit,
                                ResetTime = now.AddSeconds(rule.TimeWindowSeconds)
                            };
                        }

                        var remaining = rule.RequestLimit - usage.RequestCount;
                        var isAllowed = remaining > 0;

                        return new RateLimitResult
                        {
                            IsAllowed = isAllowed,
                            RemainingRequests = Math.Max(0, remaining - 1),
                            RequestLimit = rule.RequestLimit,
                            ResetTime = usage.WindowEnd,
                            RetryAfterSeconds = isAllowed ? null : ((int)(usage.WindowEnd - now).TotalSeconds).ToString()
                        };
                    }

                    public async Task<bool> IsRateLimitedAsync(string endpoint, string identifier, CancellationToken cancellationToken = default)
                    {
                        var result = await CheckRateLimitAsync(endpoint, identifier, cancellationToken);
                        return !result.IsAllowed;
                    }

                    public async Task IncrementRequestCountAsync(string endpoint, string identifier, CancellationToken cancellationToken = default)
                    {
                        var rule = await repository.GetByEndpointAsync(endpoint, cancellationToken);
                        if (rule == null) return;

                        var now = DateTime.UtcNow;
                        var usage = await repository.GetQuotaUsageAsync(rule.RateLimitRuleId, identifier, cancellationToken);

                        if (usage == null || usage.WindowEnd < now)
                        {
                            usage = new QuotaUsage
                            {
                                RateLimitRuleId = rule.RateLimitRuleId,
                                Identifier = identifier,
                                RequestCount = 1,
                                WindowStart = now,
                                WindowEnd = now.AddSeconds(rule.TimeWindowSeconds),
                                LastRequestAt = now
                            };
                        }
                        else
                        {
                            usage.RequestCount++;
                            usage.LastRequestAt = now;
                        }

                        await repository.AddOrUpdateQuotaUsageAsync(usage, cancellationToken);
                    }

                    public async Task ResetQuotaAsync(Guid ruleId, string identifier, CancellationToken cancellationToken = default)
                    {
                        var rule = await repository.GetByIdAsync(ruleId, cancellationToken);
                        if (rule == null) return;

                        var now = DateTime.UtcNow;
                        var usage = new QuotaUsage
                        {
                            RateLimitRuleId = ruleId,
                            Identifier = identifier,
                            RequestCount = 0,
                            WindowStart = now,
                            WindowEnd = now.AddSeconds(rule.TimeWindowSeconds),
                            LastRequestAt = now
                        };

                        await repository.AddOrUpdateQuotaUsageAsync(usage, cancellationToken);
                    }

                    public async Task<RateLimitStatus> GetStatusAsync(string endpoint, string identifier, CancellationToken cancellationToken = default)
                    {
                        var rule = await repository.GetByEndpointAsync(endpoint, cancellationToken);
                        if (rule == null)
                        {
                            return new RateLimitStatus
                            {
                                Endpoint = endpoint,
                                Identifier = identifier,
                                CurrentCount = 0,
                                Limit = int.MaxValue,
                                Remaining = int.MaxValue,
                                WindowStart = DateTime.UtcNow,
                                WindowEnd = DateTime.MaxValue
                            };
                        }

                        var usage = await repository.GetQuotaUsageAsync(rule.RateLimitRuleId, identifier, cancellationToken);
                        var now = DateTime.UtcNow;

                        if (usage == null || usage.WindowEnd < now)
                        {
                            return new RateLimitStatus
                            {
                                Endpoint = endpoint,
                                Identifier = identifier,
                                CurrentCount = 0,
                                Limit = rule.RequestLimit,
                                Remaining = rule.RequestLimit,
                                WindowStart = now,
                                WindowEnd = now.AddSeconds(rule.TimeWindowSeconds)
                            };
                        }

                        return new RateLimitStatus
                        {
                            Endpoint = endpoint,
                            Identifier = identifier,
                            CurrentCount = usage.RequestCount,
                            Limit = rule.RequestLimit,
                            Remaining = Math.Max(0, rule.RequestLimit - usage.RequestCount),
                            WindowStart = usage.WindowStart,
                            WindowEnd = usage.WindowEnd
                        };
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
                using RateLimiting.Core.Interfaces;
                using RateLimiting.Infrastructure.Data;
                using RateLimiting.Infrastructure.Repositories;
                using RateLimiting.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddRateLimitingInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<RateLimitingDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("RateLimitingDb") ??
                                @"Server=.\SQLEXPRESS;Database=RateLimitingDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IRateLimitRepository, RateLimitRepository>();
                        services.AddScoped<IRateLimitService, RateLimitService>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding RateLimiting.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("RateLimitsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using RateLimiting.Core.DTOs;
                using RateLimiting.Core.Interfaces;

                namespace RateLimiting.Api.Controllers;

                [ApiController]
                [Route("api/ratelimits")]
                public class RateLimitsController : ControllerBase
                {
                    private readonly IRateLimitService rateLimitService;
                    private readonly IRateLimitRepository repository;

                    public RateLimitsController(IRateLimitService rateLimitService, IRateLimitRepository repository)
                    {
                        this.rateLimitService = rateLimitService;
                        this.repository = repository;
                    }

                    [HttpGet("status")]
                    public async Task<ActionResult<RateLimitStatusDto>> GetStatus([FromQuery] string endpoint, [FromQuery] string identifier, CancellationToken cancellationToken)
                    {
                        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(identifier))
                        {
                            return BadRequest("Endpoint and identifier are required");
                        }

                        var status = await rateLimitService.GetStatusAsync(endpoint, identifier, cancellationToken);
                        return Ok(new RateLimitStatusDto
                        {
                            Endpoint = status.Endpoint,
                            Identifier = status.Identifier,
                            CurrentCount = status.CurrentCount,
                            Limit = status.Limit,
                            Remaining = status.Remaining,
                            ResetAt = status.WindowEnd,
                            IsLimited = status.Remaining <= 0
                        });
                    }

                    [HttpGet("quotas")]
                    public async Task<ActionResult<IEnumerable<QuotaDto>>> GetQuotas([FromQuery] string? identifier, CancellationToken cancellationToken)
                    {
                        var rules = await repository.GetEnabledRulesAsync(cancellationToken);
                        var quotas = new List<QuotaDto>();

                        foreach (var rule in rules)
                        {
                            var currentIdentifier = identifier ?? "anonymous";
                            var usage = await repository.GetQuotaUsageAsync(rule.RateLimitRuleId, currentIdentifier, cancellationToken);
                            var now = DateTime.UtcNow;

                            var used = (usage != null && usage.WindowEnd >= now) ? usage.RequestCount : 0;
                            var remaining = rule.RequestLimit - used;

                            quotas.Add(new QuotaDto
                            {
                                RuleId = rule.RateLimitRuleId,
                                RuleName = rule.Name,
                                Endpoint = rule.Endpoint,
                                Identifier = currentIdentifier,
                                Used = used,
                                Limit = rule.RequestLimit,
                                Remaining = Math.Max(0, remaining),
                                UsagePercentage = rule.RequestLimit > 0 ? (double)used / rule.RequestLimit * 100 : 0,
                                WindowStart = usage?.WindowStart ?? now,
                                WindowEnd = usage?.WindowEnd ?? now.AddSeconds(rule.TimeWindowSeconds)
                            });
                        }

                        return Ok(quotas);
                    }
                }
                """
        });

        project.Files.Add(new FileModel("RateLimitRulesController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using RateLimiting.Core.DTOs;
                using RateLimiting.Core.Entities;
                using RateLimiting.Core.Interfaces;

                namespace RateLimiting.Api.Controllers;

                [ApiController]
                [Route("api/ratelimit-rules")]
                public class RateLimitRulesController : ControllerBase
                {
                    private readonly IRateLimitRepository repository;

                    public RateLimitRulesController(IRateLimitRepository repository)
                    {
                        this.repository = repository;
                    }

                    [HttpGet]
                    public async Task<ActionResult<IEnumerable<RateLimitRuleDto>>> GetAll(CancellationToken cancellationToken)
                    {
                        var rules = await repository.GetAllAsync(cancellationToken);
                        return Ok(rules.Select(r => new RateLimitRuleDto
                        {
                            RateLimitRuleId = r.RateLimitRuleId,
                            Name = r.Name,
                            Description = r.Description,
                            Endpoint = r.Endpoint,
                            RequestLimit = r.RequestLimit,
                            TimeWindowSeconds = r.TimeWindowSeconds,
                            Scope = r.Scope.ToString(),
                            IsEnabled = r.IsEnabled,
                            CreatedAt = r.CreatedAt
                        }));
                    }

                    [HttpGet("{id:guid}")]
                    public async Task<ActionResult<RateLimitRuleDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var rule = await repository.GetByIdAsync(id, cancellationToken);
                        if (rule == null) return NotFound();

                        return Ok(new RateLimitRuleDto
                        {
                            RateLimitRuleId = rule.RateLimitRuleId,
                            Name = rule.Name,
                            Description = rule.Description,
                            Endpoint = rule.Endpoint,
                            RequestLimit = rule.RequestLimit,
                            TimeWindowSeconds = rule.TimeWindowSeconds,
                            Scope = rule.Scope.ToString(),
                            IsEnabled = rule.IsEnabled,
                            CreatedAt = rule.CreatedAt
                        });
                    }

                    [HttpPost]
                    public async Task<ActionResult<RateLimitRuleDto>> Create([FromBody] CreateRateLimitRuleRequest request, CancellationToken cancellationToken)
                    {
                        var existing = await repository.GetByEndpointAsync(request.Endpoint, cancellationToken);
                        if (existing != null)
                        {
                            return BadRequest("A rate limit rule already exists for this endpoint");
                        }

                        var rule = new RateLimitRule
                        {
                            Name = request.Name,
                            Description = request.Description,
                            Endpoint = request.Endpoint,
                            RequestLimit = request.RequestLimit,
                            TimeWindowSeconds = request.TimeWindowSeconds,
                            Scope = Enum.TryParse<RateLimitScope>(request.Scope, out var scope) ? scope : RateLimitScope.PerUser
                        };

                        var created = await repository.AddAsync(rule, cancellationToken);

                        return CreatedAtAction(nameof(GetById), new { id = created.RateLimitRuleId }, new RateLimitRuleDto
                        {
                            RateLimitRuleId = created.RateLimitRuleId,
                            Name = created.Name,
                            Description = created.Description,
                            Endpoint = created.Endpoint,
                            RequestLimit = created.RequestLimit,
                            TimeWindowSeconds = created.TimeWindowSeconds,
                            Scope = created.Scope.ToString(),
                            IsEnabled = created.IsEnabled,
                            CreatedAt = created.CreatedAt
                        });
                    }

                    [HttpDelete("{id:guid}")]
                    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
                    {
                        var rule = await repository.GetByIdAsync(id, cancellationToken);
                        if (rule == null) return NotFound();

                        await repository.DeleteAsync(id, cancellationToken);
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

                builder.Services.AddRateLimitingInfrastructure(builder.Configuration);
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
                    "RateLimitingDb": "Server=.\\SQLEXPRESS;Database=RateLimitingDb;Trusted_Connection=True;TrustServerCertificate=True"
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
