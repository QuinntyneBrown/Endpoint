// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Integration;

public class IntegrationArtifactFactory : IIntegrationArtifactFactory
{
    private readonly ILogger<IntegrationArtifactFactory> logger;

    public IntegrationArtifactFactory(ILogger<IntegrationArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Integration.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("Integration", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.Entities;

                public class Integration
                {
                    public Guid IntegrationId { get; set; }
                    public Guid TenantId { get; set; }
                    public required string Name { get; set; }
                    public required string Provider { get; set; }
                    public IntegrationStatus Status { get; set; } = IntegrationStatus.Pending;
                    public string? Configuration { get; set; }
                    public string? Credentials { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? ConnectedAt { get; set; }
                    public DateTime? LastSyncAt { get; set; }
                    public ICollection<Webhook> Webhooks { get; set; } = new List<Webhook>();
                    public ICollection<SyncJob> SyncJobs { get; set; } = new List<SyncJob>();
                }

                public enum IntegrationStatus { Pending, Connected, Disconnected, Error }
                """
        });

        project.Files.Add(new FileModel("Webhook", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.Entities;

                public class Webhook
                {
                    public Guid WebhookId { get; set; }
                    public Guid IntegrationId { get; set; }
                    public Integration Integration { get; set; } = null!;
                    public required string Endpoint { get; set; }
                    public required string Secret { get; set; }
                    public string? EventTypes { get; set; }
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? LastTriggeredAt { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("SyncJob", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.Entities;

                public class SyncJob
                {
                    public Guid SyncJobId { get; set; }
                    public Guid IntegrationId { get; set; }
                    public Integration Integration { get; set; } = null!;
                    public required string JobType { get; set; }
                    public SyncJobStatus Status { get; set; } = SyncJobStatus.Pending;
                    public string? Parameters { get; set; }
                    public string? Result { get; set; }
                    public int RecordsProcessed { get; set; }
                    public int RecordsFailed { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? StartedAt { get; set; }
                    public DateTime? CompletedAt { get; set; }
                }

                public enum SyncJobStatus { Pending, Running, Completed, Failed, Cancelled }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IIntegrationRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Integration.Core.Entities;

                namespace Integration.Core.Interfaces;

                public interface IIntegrationRepository
                {
                    Task<Entities.Integration?> GetByIdAsync(Guid integrationId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Entities.Integration>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Entities.Integration>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<Entities.Integration> AddAsync(Entities.Integration integration, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Entities.Integration integration, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid integrationId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IWebhookHandler", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Integration.Core.DTOs;

                namespace Integration.Core.Interfaces;

                public interface IWebhookHandler
                {
                    Task<WebhookResponseDto> HandleAsync(Guid integrationId, WebhookPayloadDto payload, CancellationToken cancellationToken = default);
                    Task<bool> ValidateSignatureAsync(Guid integrationId, string signature, string payload, CancellationToken cancellationToken = default);
                    Task<WebhookDto> RegisterAsync(Guid integrationId, RegisterWebhookRequest request, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ISyncService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Integration.Core.DTOs;

                namespace Integration.Core.Interfaces;

                public interface ISyncService
                {
                    Task<SyncJobDto> StartSyncAsync(Guid integrationId, StartSyncRequest request, CancellationToken cancellationToken = default);
                    Task<SyncJobDto?> GetSyncJobAsync(Guid syncJobId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<SyncJobDto>> GetSyncJobsByIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default);
                    Task CancelSyncAsync(Guid syncJobId, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("IntegrationConnectedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.Events;

                public sealed class IntegrationConnectedEvent
                {
                    public Guid IntegrationId { get; init; }
                    public Guid TenantId { get; init; }
                    public required string Provider { get; init; }
                    public DateTime ConnectedAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("WebhookReceivedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.Events;

                public sealed class WebhookReceivedEvent
                {
                    public Guid WebhookId { get; init; }
                    public Guid IntegrationId { get; init; }
                    public required string EventType { get; init; }
                    public required string Payload { get; init; }
                    public DateTime ReceivedAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("SyncCompletedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.Events;

                public sealed class SyncCompletedEvent
                {
                    public Guid SyncJobId { get; init; }
                    public Guid IntegrationId { get; init; }
                    public required string JobType { get; init; }
                    public int RecordsProcessed { get; init; }
                    public int RecordsFailed { get; init; }
                    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("IntegrationDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.DTOs;

                public sealed class IntegrationDto
                {
                    public Guid IntegrationId { get; init; }
                    public Guid TenantId { get; init; }
                    public required string Name { get; init; }
                    public required string Provider { get; init; }
                    public string Status { get; init; } = "Pending";
                    public DateTime CreatedAt { get; init; }
                    public DateTime? ConnectedAt { get; init; }
                    public DateTime? LastSyncAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateIntegrationRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Integration.Core.DTOs;

                public sealed class CreateIntegrationRequest
                {
                    [Required]
                    public Guid TenantId { get; init; }

                    [Required]
                    public required string Name { get; init; }

                    [Required]
                    public required string Provider { get; init; }

                    public string? Configuration { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("WebhookDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.DTOs;

                public sealed class WebhookDto
                {
                    public Guid WebhookId { get; init; }
                    public Guid IntegrationId { get; init; }
                    public required string Endpoint { get; init; }
                    public string? EventTypes { get; init; }
                    public bool IsActive { get; init; }
                    public DateTime CreatedAt { get; init; }
                    public DateTime? LastTriggeredAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("WebhookPayloadDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.DTOs;

                public sealed class WebhookPayloadDto
                {
                    public required string EventType { get; init; }
                    public required string Payload { get; init; }
                    public string? Signature { get; init; }
                    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("WebhookResponseDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.DTOs;

                public sealed class WebhookResponseDto
                {
                    public bool Success { get; init; }
                    public string? Message { get; init; }
                    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("RegisterWebhookRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Integration.Core.DTOs;

                public sealed class RegisterWebhookRequest
                {
                    [Required]
                    public required string Endpoint { get; init; }

                    public string? EventTypes { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("SyncJobDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Integration.Core.DTOs;

                public sealed class SyncJobDto
                {
                    public Guid SyncJobId { get; init; }
                    public Guid IntegrationId { get; init; }
                    public required string JobType { get; init; }
                    public string Status { get; init; } = "Pending";
                    public int RecordsProcessed { get; init; }
                    public int RecordsFailed { get; init; }
                    public DateTime CreatedAt { get; init; }
                    public DateTime? StartedAt { get; init; }
                    public DateTime? CompletedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("StartSyncRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Integration.Core.DTOs;

                public sealed class StartSyncRequest
                {
                    [Required]
                    public required string JobType { get; init; }

                    public string? Parameters { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Integration.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("IntegrationDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Integration.Core.Entities;

                namespace Integration.Infrastructure.Data;

                public class IntegrationDbContext : DbContext
                {
                    public IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : base(options) { }

                    public DbSet<Core.Entities.Integration> Integrations => Set<Core.Entities.Integration>();
                    public DbSet<Webhook> Webhooks => Set<Webhook>();
                    public DbSet<SyncJob> SyncJobs => Set<SyncJob>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<Core.Entities.Integration>(entity =>
                        {
                            entity.HasKey(i => i.IntegrationId);
                            entity.Property(i => i.Name).IsRequired().HasMaxLength(200);
                            entity.Property(i => i.Provider).IsRequired().HasMaxLength(100);
                            entity.HasMany(i => i.Webhooks).WithOne(w => w.Integration).HasForeignKey(w => w.IntegrationId);
                            entity.HasMany(i => i.SyncJobs).WithOne(s => s.Integration).HasForeignKey(s => s.IntegrationId);
                        });

                        modelBuilder.Entity<Webhook>(entity =>
                        {
                            entity.HasKey(w => w.WebhookId);
                            entity.Property(w => w.Endpoint).IsRequired().HasMaxLength(500);
                            entity.Property(w => w.Secret).IsRequired().HasMaxLength(256);
                        });

                        modelBuilder.Entity<SyncJob>(entity =>
                        {
                            entity.HasKey(s => s.SyncJobId);
                            entity.Property(s => s.JobType).IsRequired().HasMaxLength(100);
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("IntegrationRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Integration.Core.Interfaces;
                using Integration.Infrastructure.Data;

                namespace Integration.Infrastructure.Repositories;

                public class IntegrationRepository : IIntegrationRepository
                {
                    private readonly IntegrationDbContext context;

                    public IntegrationRepository(IntegrationDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Core.Entities.Integration?> GetByIdAsync(Guid integrationId, CancellationToken cancellationToken = default)
                        => await context.Integrations.Include(i => i.Webhooks).Include(i => i.SyncJobs).FirstOrDefaultAsync(i => i.IntegrationId == integrationId, cancellationToken);

                    public async Task<IEnumerable<Core.Entities.Integration>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
                        => await context.Integrations.Include(i => i.Webhooks).Where(i => i.TenantId == tenantId).ToListAsync(cancellationToken);

                    public async Task<IEnumerable<Core.Entities.Integration>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.Integrations.Include(i => i.Webhooks).ToListAsync(cancellationToken);

                    public async Task<Core.Entities.Integration> AddAsync(Core.Entities.Integration integration, CancellationToken cancellationToken = default)
                    {
                        integration.IntegrationId = Guid.NewGuid();
                        await context.Integrations.AddAsync(integration, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return integration;
                    }

                    public async Task UpdateAsync(Core.Entities.Integration integration, CancellationToken cancellationToken = default)
                    {
                        context.Integrations.Update(integration);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid integrationId, CancellationToken cancellationToken = default)
                    {
                        var integration = await context.Integrations.FindAsync(new object[] { integrationId }, cancellationToken);
                        if (integration != null)
                        {
                            context.Integrations.Remove(integration);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("WebhookHandler", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Security.Cryptography;
                using System.Text;
                using Microsoft.EntityFrameworkCore;
                using Integration.Core.DTOs;
                using Integration.Core.Entities;
                using Integration.Core.Interfaces;
                using Integration.Infrastructure.Data;

                namespace Integration.Infrastructure.Services;

                public class WebhookHandler : IWebhookHandler
                {
                    private readonly IntegrationDbContext context;

                    public WebhookHandler(IntegrationDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<WebhookResponseDto> HandleAsync(Guid integrationId, WebhookPayloadDto payload, CancellationToken cancellationToken = default)
                    {
                        var integration = await context.Integrations.Include(i => i.Webhooks).FirstOrDefaultAsync(i => i.IntegrationId == integrationId, cancellationToken);
                        if (integration == null)
                        {
                            return new WebhookResponseDto { Success = false, Message = "Integration not found" };
                        }

                        var webhook = integration.Webhooks.FirstOrDefault(w => w.IsActive);
                        if (webhook != null)
                        {
                            webhook.LastTriggeredAt = DateTime.UtcNow;
                            await context.SaveChangesAsync(cancellationToken);
                        }

                        return new WebhookResponseDto { Success = true, Message = "Webhook processed successfully" };
                    }

                    public async Task<bool> ValidateSignatureAsync(Guid integrationId, string signature, string payload, CancellationToken cancellationToken = default)
                    {
                        var webhook = await context.Webhooks.FirstOrDefaultAsync(w => w.IntegrationId == integrationId && w.IsActive, cancellationToken);
                        if (webhook == null) return false;

                        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhook.Secret));
                        var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
                        return computedHash == signature;
                    }

                    public async Task<WebhookDto> RegisterAsync(Guid integrationId, RegisterWebhookRequest request, CancellationToken cancellationToken = default)
                    {
                        var webhook = new Webhook
                        {
                            WebhookId = Guid.NewGuid(),
                            IntegrationId = integrationId,
                            Endpoint = request.Endpoint,
                            Secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                            EventTypes = request.EventTypes
                        };

                        await context.Webhooks.AddAsync(webhook, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);

                        return new WebhookDto
                        {
                            WebhookId = webhook.WebhookId,
                            IntegrationId = webhook.IntegrationId,
                            Endpoint = webhook.Endpoint,
                            EventTypes = webhook.EventTypes,
                            IsActive = webhook.IsActive,
                            CreatedAt = webhook.CreatedAt
                        };
                    }
                }
                """
        });

        project.Files.Add(new FileModel("SyncService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Integration.Core.DTOs;
                using Integration.Core.Entities;
                using Integration.Core.Interfaces;
                using Integration.Infrastructure.Data;

                namespace Integration.Infrastructure.Services;

                public class SyncService : ISyncService
                {
                    private readonly IntegrationDbContext context;

                    public SyncService(IntegrationDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<SyncJobDto> StartSyncAsync(Guid integrationId, StartSyncRequest request, CancellationToken cancellationToken = default)
                    {
                        var syncJob = new SyncJob
                        {
                            SyncJobId = Guid.NewGuid(),
                            IntegrationId = integrationId,
                            JobType = request.JobType,
                            Parameters = request.Parameters,
                            Status = SyncJobStatus.Running,
                            StartedAt = DateTime.UtcNow
                        };

                        await context.SyncJobs.AddAsync(syncJob, cancellationToken);

                        var integration = await context.Integrations.FindAsync(new object[] { integrationId }, cancellationToken);
                        if (integration != null)
                        {
                            integration.LastSyncAt = DateTime.UtcNow;
                        }

                        await context.SaveChangesAsync(cancellationToken);

                        return new SyncJobDto
                        {
                            SyncJobId = syncJob.SyncJobId,
                            IntegrationId = syncJob.IntegrationId,
                            JobType = syncJob.JobType,
                            Status = syncJob.Status.ToString(),
                            CreatedAt = syncJob.CreatedAt,
                            StartedAt = syncJob.StartedAt
                        };
                    }

                    public async Task<SyncJobDto?> GetSyncJobAsync(Guid syncJobId, CancellationToken cancellationToken = default)
                    {
                        var syncJob = await context.SyncJobs.FindAsync(new object[] { syncJobId }, cancellationToken);
                        if (syncJob == null) return null;

                        return new SyncJobDto
                        {
                            SyncJobId = syncJob.SyncJobId,
                            IntegrationId = syncJob.IntegrationId,
                            JobType = syncJob.JobType,
                            Status = syncJob.Status.ToString(),
                            RecordsProcessed = syncJob.RecordsProcessed,
                            RecordsFailed = syncJob.RecordsFailed,
                            CreatedAt = syncJob.CreatedAt,
                            StartedAt = syncJob.StartedAt,
                            CompletedAt = syncJob.CompletedAt
                        };
                    }

                    public async Task<IEnumerable<SyncJobDto>> GetSyncJobsByIntegrationAsync(Guid integrationId, CancellationToken cancellationToken = default)
                    {
                        var syncJobs = await context.SyncJobs.Where(s => s.IntegrationId == integrationId).OrderByDescending(s => s.CreatedAt).ToListAsync(cancellationToken);
                        return syncJobs.Select(s => new SyncJobDto
                        {
                            SyncJobId = s.SyncJobId,
                            IntegrationId = s.IntegrationId,
                            JobType = s.JobType,
                            Status = s.Status.ToString(),
                            RecordsProcessed = s.RecordsProcessed,
                            RecordsFailed = s.RecordsFailed,
                            CreatedAt = s.CreatedAt,
                            StartedAt = s.StartedAt,
                            CompletedAt = s.CompletedAt
                        });
                    }

                    public async Task CancelSyncAsync(Guid syncJobId, CancellationToken cancellationToken = default)
                    {
                        var syncJob = await context.SyncJobs.FindAsync(new object[] { syncJobId }, cancellationToken);
                        if (syncJob != null && syncJob.Status == SyncJobStatus.Running)
                        {
                            syncJob.Status = SyncJobStatus.Cancelled;
                            syncJob.CompletedAt = DateTime.UtcNow;
                            await context.SaveChangesAsync(cancellationToken);
                        }
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
                using Integration.Core.Interfaces;
                using Integration.Infrastructure.Data;
                using Integration.Infrastructure.Repositories;
                using Integration.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddIntegrationInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<IntegrationDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("IntegrationDb") ??
                                @"Server=.\SQLEXPRESS;Database=IntegrationDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IIntegrationRepository, IntegrationRepository>();
                        services.AddScoped<IWebhookHandler, WebhookHandler>();
                        services.AddScoped<ISyncService, SyncService>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Integration.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("IntegrationsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Integration.Core.DTOs;
                using Integration.Core.Entities;
                using Integration.Core.Interfaces;

                namespace Integration.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class IntegrationsController : ControllerBase
                {
                    private readonly IIntegrationRepository repository;

                    public IntegrationsController(IIntegrationRepository repository)
                    {
                        this.repository = repository;
                    }

                    [HttpPost]
                    public async Task<ActionResult<IntegrationDto>> Create([FromBody] CreateIntegrationRequest request, CancellationToken cancellationToken)
                    {
                        var integration = new Core.Entities.Integration
                        {
                            TenantId = request.TenantId,
                            Name = request.Name,
                            Provider = request.Provider,
                            Configuration = request.Configuration
                        };

                        var created = await repository.AddAsync(integration, cancellationToken);

                        var dto = new IntegrationDto
                        {
                            IntegrationId = created.IntegrationId,
                            TenantId = created.TenantId,
                            Name = created.Name,
                            Provider = created.Provider,
                            Status = created.Status.ToString(),
                            CreatedAt = created.CreatedAt
                        };

                        return CreatedAtAction(nameof(GetById), new { id = dto.IntegrationId }, dto);
                    }

                    [HttpGet("{id:guid}")]
                    public async Task<ActionResult<IntegrationDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var integration = await repository.GetByIdAsync(id, cancellationToken);
                        if (integration == null) return NotFound();

                        return Ok(new IntegrationDto
                        {
                            IntegrationId = integration.IntegrationId,
                            TenantId = integration.TenantId,
                            Name = integration.Name,
                            Provider = integration.Provider,
                            Status = integration.Status.ToString(),
                            CreatedAt = integration.CreatedAt,
                            ConnectedAt = integration.ConnectedAt,
                            LastSyncAt = integration.LastSyncAt
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("WebhooksController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Integration.Core.DTOs;
                using Integration.Core.Interfaces;

                namespace Integration.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class WebhooksController : ControllerBase
                {
                    private readonly IWebhookHandler webhookHandler;

                    public WebhooksController(IWebhookHandler webhookHandler)
                    {
                        this.webhookHandler = webhookHandler;
                    }

                    [HttpPost("{integrationId:guid}")]
                    public async Task<ActionResult<WebhookResponseDto>> Handle(Guid integrationId, [FromBody] WebhookPayloadDto payload, CancellationToken cancellationToken)
                    {
                        var response = await webhookHandler.HandleAsync(integrationId, payload, cancellationToken);
                        if (!response.Success) return BadRequest(response);
                        return Ok(response);
                    }

                    [HttpPost("{integrationId:guid}/register")]
                    public async Task<ActionResult<WebhookDto>> Register(Guid integrationId, [FromBody] RegisterWebhookRequest request, CancellationToken cancellationToken)
                    {
                        var webhook = await webhookHandler.RegisterAsync(integrationId, request, cancellationToken);
                        return Created($"/api/webhooks/{integrationId}", webhook);
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

                builder.Services.AddIntegrationInfrastructure(builder.Configuration);
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
                    "IntegrationDb": "Server=.\\SQLEXPRESS;Database=IntegrationDb;Trusted_Connection=True;TrustServerCertificate=True"
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
