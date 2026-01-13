// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Tenant;

public class TenantArtifactFactory : ITenantArtifactFactory
{
    private readonly ILogger<TenantArtifactFactory> logger;

    public TenantArtifactFactory(ILogger<TenantArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Tenant.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("Tenant", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tenant.Core.Entities;

                public class Tenant
                {
                    public Guid TenantId { get; set; }
                    public required string Name { get; set; }
                    public required string Slug { get; set; }
                    public TenantStatus Status { get; set; } = TenantStatus.Active;
                    public TenantTier Tier { get; set; } = TenantTier.Basic;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? SubscriptionExpiry { get; set; }
                    public ICollection<TenantSetting> Settings { get; set; } = new List<TenantSetting>();
                }

                public enum TenantStatus { Active, Suspended, Deactivated }
                public enum TenantTier { Basic, Standard, Enterprise }
                """
        });

        project.Files.Add(new FileModel("TenantSetting", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tenant.Core.Entities;

                public class TenantSetting
                {
                    public Guid SettingId { get; set; }
                    public Guid TenantId { get; set; }
                    public Tenant Tenant { get; set; } = null!;
                    public required string Key { get; set; }
                    public string? Value { get; set; }
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("ITenantRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Tenant.Core.Entities;

                namespace Tenant.Core.Interfaces;

                public interface ITenantRepository
                {
                    Task<Entities.Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
                    Task<Entities.Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Entities.Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<Entities.Tenant> AddAsync(Entities.Tenant tenant, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Entities.Tenant tenant, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid tenantId, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("TenantCreatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tenant.Core.Events;

                public sealed class TenantCreatedEvent
                {
                    public Guid TenantId { get; init; }
                    public required string Name { get; init; }
                    public required string Slug { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("TenantSuspendedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tenant.Core.Events;

                public sealed class TenantSuspendedEvent
                {
                    public Guid TenantId { get; init; }
                    public required string Reason { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("TenantDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tenant.Core.DTOs;

                public sealed class TenantDto
                {
                    public Guid TenantId { get; init; }
                    public required string Name { get; init; }
                    public required string Slug { get; init; }
                    public string Status { get; init; } = "Active";
                    public string Tier { get; init; } = "Basic";
                    public DateTime CreatedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateTenantRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Tenant.Core.DTOs;

                public sealed class CreateTenantRequest
                {
                    [Required]
                    public required string Name { get; init; }

                    [Required]
                    public required string Slug { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Tenant.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");

        project.Files.Add(new FileModel("TenantDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Tenant.Core.Entities;

                namespace Tenant.Infrastructure.Data;

                public class TenantDbContext : DbContext
                {
                    public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

                    public DbSet<Core.Entities.Tenant> Tenants => Set<Core.Entities.Tenant>();
                    public DbSet<TenantSetting> TenantSettings => Set<TenantSetting>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<Core.Entities.Tenant>(entity =>
                        {
                            entity.HasKey(t => t.TenantId);
                            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
                            entity.Property(t => t.Slug).IsRequired().HasMaxLength(100);
                            entity.HasIndex(t => t.Slug).IsUnique();
                        });

                        modelBuilder.Entity<TenantSetting>(entity =>
                        {
                            entity.HasKey(s => s.SettingId);
                            entity.HasOne(s => s.Tenant).WithMany(t => t.Settings).HasForeignKey(s => s.TenantId);
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("TenantRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Tenant.Core.Interfaces;
                using Tenant.Infrastructure.Data;

                namespace Tenant.Infrastructure.Repositories;

                public class TenantRepository : ITenantRepository
                {
                    private readonly TenantDbContext context;

                    public TenantRepository(TenantDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Core.Entities.Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
                        => await context.Tenants.Include(t => t.Settings).FirstOrDefaultAsync(t => t.TenantId == tenantId, cancellationToken);

                    public async Task<Core.Entities.Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
                        => await context.Tenants.Include(t => t.Settings).FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);

                    public async Task<IEnumerable<Core.Entities.Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.Tenants.Include(t => t.Settings).ToListAsync(cancellationToken);

                    public async Task<Core.Entities.Tenant> AddAsync(Core.Entities.Tenant tenant, CancellationToken cancellationToken = default)
                    {
                        tenant.TenantId = Guid.NewGuid();
                        await context.Tenants.AddAsync(tenant, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return tenant;
                    }

                    public async Task UpdateAsync(Core.Entities.Tenant tenant, CancellationToken cancellationToken = default)
                    {
                        context.Tenants.Update(tenant);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid tenantId, CancellationToken cancellationToken = default)
                    {
                        var tenant = await context.Tenants.FindAsync(new object[] { tenantId }, cancellationToken);
                        if (tenant != null)
                        {
                            context.Tenants.Remove(tenant);
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
                using Tenant.Core.Interfaces;
                using Tenant.Infrastructure.Data;
                using Tenant.Infrastructure.Repositories;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddTenantInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<TenantDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("TenantDb") ??
                                @"Server=.\SQLEXPRESS;Database=TenantDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<ITenantRepository, TenantRepository>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Tenant.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("TenantsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Tenant.Core.DTOs;
                using Tenant.Core.Interfaces;

                namespace Tenant.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class TenantsController : ControllerBase
                {
                    private readonly ITenantRepository repository;

                    public TenantsController(ITenantRepository repository)
                    {
                        this.repository = repository;
                    }

                    [HttpGet]
                    public async Task<ActionResult<IEnumerable<TenantDto>>> GetAll(CancellationToken cancellationToken)
                    {
                        var tenants = await repository.GetAllAsync(cancellationToken);
                        return Ok(tenants.Select(t => new TenantDto
                        {
                            TenantId = t.TenantId,
                            Name = t.Name,
                            Slug = t.Slug,
                            Status = t.Status.ToString(),
                            Tier = t.Tier.ToString(),
                            CreatedAt = t.CreatedAt
                        }));
                    }

                    [HttpGet("{id:guid}")]
                    public async Task<ActionResult<TenantDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var tenant = await repository.GetByIdAsync(id, cancellationToken);
                        if (tenant == null) return NotFound();
                        return Ok(new TenantDto
                        {
                            TenantId = tenant.TenantId,
                            Name = tenant.Name,
                            Slug = tenant.Slug,
                            Status = tenant.Status.ToString(),
                            Tier = tenant.Tier.ToString(),
                            CreatedAt = tenant.CreatedAt
                        });
                    }

                    [HttpPost]
                    public async Task<ActionResult<TenantDto>> Create([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
                    {
                        var existing = await repository.GetBySlugAsync(request.Slug, cancellationToken);
                        if (existing != null) return BadRequest("Slug already exists");

                        var tenant = new Core.Entities.Tenant { Name = request.Name, Slug = request.Slug };
                        var created = await repository.AddAsync(tenant, cancellationToken);
                        return CreatedAtAction(nameof(GetById), new { id = created.TenantId }, new TenantDto
                        {
                            TenantId = created.TenantId,
                            Name = created.Name,
                            Slug = created.Slug,
                            CreatedAt = created.CreatedAt
                        });
                    }

                    [HttpDelete("{id:guid}")]
                    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
                    {
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

                builder.Services.AddTenantInfrastructure(builder.Configuration);
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
                    "TenantDb": "Server=.\\SQLEXPRESS;Database=TenantDb;Trusted_Connection=True;TrustServerCertificate=True"
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
