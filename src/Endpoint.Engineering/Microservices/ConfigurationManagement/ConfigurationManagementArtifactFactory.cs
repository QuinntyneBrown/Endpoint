// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.ConfigurationManagement;

/// <summary>
/// Factory for creating Configuration Management microservice artifacts.
/// Acts as the single source of truth for definitions and configuration files.
/// </summary>
public class ConfigurationManagementArtifactFactory : IConfigurationManagementArtifactFactory
{
    private readonly ILogger<ConfigurationManagementArtifactFactory> logger;

    public ConfigurationManagementArtifactFactory(ILogger<ConfigurationManagementArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding ConfigurationManagement.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("ConfigurationFile", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.ConfigurationManagement.Core.Entities;

                public class ConfigurationFile
                {
                    public Guid ConfigurationFileId { get; set; }
                    public required string Name { get; set; }
                    public required string Path { get; set; }
                    public required string ContentType { get; set; }
                    public string? Description { get; set; }
                    public string? Version { get; set; }
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public ICollection<ConfigurationFileItem> Items { get; set; } = new List<ConfigurationFileItem>();
                }
                """
        });

        project.Files.Add(new FileModel("ConfigurationFileItem", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.ConfigurationManagement.Core.Entities;

                public class ConfigurationFileItem
                {
                    public Guid ConfigurationFileItemId { get; set; }
                    public Guid ConfigurationFileId { get; set; }
                    public required string Key { get; set; }
                    public required string Value { get; set; }
                    public string? Description { get; set; }
                    public ConfigurationValueType ValueType { get; set; } = ConfigurationValueType.String;
                    public bool IsEncrypted { get; set; } = false;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public ConfigurationFile? ConfigurationFile { get; set; }
                }

                public enum ConfigurationValueType { String, Integer, Boolean, Json, Xml }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IConfigurationFileRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.ConfigurationManagement.Core.Entities;

                namespace EventMonitoring.ConfigurationManagement.Core.Interfaces;

                public interface IConfigurationFileRepository
                {
                    Task<ConfigurationFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
                    Task<ConfigurationFile?> GetByPathAsync(string path, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ConfigurationFile>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<ConfigurationFile> AddAsync(ConfigurationFile configurationFile, CancellationToken cancellationToken = default);
                    Task UpdateAsync(ConfigurationFile configurationFile, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IConfigurationFileItemRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.ConfigurationManagement.Core.Entities;

                namespace EventMonitoring.ConfigurationManagement.Core.Interfaces;

                public interface IConfigurationFileItemRepository
                {
                    Task<ConfigurationFileItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ConfigurationFileItem>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default);
                    Task<ConfigurationFileItem> AddAsync(ConfigurationFileItem item, CancellationToken cancellationToken = default);
                    Task UpdateAsync(ConfigurationFileItem item, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("ConfigurationFileCreatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.ConfigurationManagement.Core.Events;

                public record ConfigurationFileCreatedEvent(Guid ConfigurationFileId, string Name, string Path, DateTime CreatedAt);
                """
        });

        project.Files.Add(new FileModel("ConfigurationFileUpdatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.ConfigurationManagement.Core.Events;

                public record ConfigurationFileUpdatedEvent(Guid ConfigurationFileId, string Name, DateTime UpdatedAt);
                """
        });

        // DTOs
        project.Files.Add(new FileModel("ConfigurationFileDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.ConfigurationManagement.Core.DTOs;

                public class ConfigurationFileDto
                {
                    public Guid ConfigurationFileId { get; set; }
                    public required string Name { get; set; }
                    public required string Path { get; set; }
                    public string? Description { get; set; }
                    public string? Version { get; set; }
                    public bool IsActive { get; set; }
                    public DateTime CreatedAt { get; set; }
                    public DateTime? UpdatedAt { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("ConfigurationFileItemDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace EventMonitoring.ConfigurationManagement.Core.DTOs;

                public class ConfigurationFileItemDto
                {
                    public Guid ConfigurationFileItemId { get; set; }
                    public required string Key { get; set; }
                    public required string Value { get; set; }
                    public string? Description { get; set; }
                    public DateTime CreatedAt { get; set; }
                    public DateTime? UpdatedAt { get; set; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding ConfigurationManagement.Infrastructure files");

        var dataDir = Path.Combine(project.Directory, "Data");
        var configurationsDir = Path.Combine(project.Directory, "Data", "Configurations");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");

        // DbContext
        project.Files.Add(new FileModel("ConfigurationManagementDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.ConfigurationManagement.Core.Entities;
                using Microsoft.EntityFrameworkCore;

                namespace EventMonitoring.ConfigurationManagement.Infrastructure.Data;

                public class ConfigurationManagementDbContext : DbContext
                {
                    public ConfigurationManagementDbContext(DbContextOptions<ConfigurationManagementDbContext> options)
                        : base(options)
                    {
                    }

                    public DbSet<ConfigurationFile> ConfigurationFiles => Set<ConfigurationFile>();
                    public DbSet<ConfigurationFileItem> ConfigurationFileItems => Set<ConfigurationFileItem>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        base.OnModelCreating(modelBuilder);
                        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfigurationManagementDbContext).Assembly);
                    }
                }
                """
        });

        // Entity Configurations
        project.Files.Add(new FileModel("ConfigurationFileConfiguration", configurationsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.ConfigurationManagement.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace EventMonitoring.ConfigurationManagement.Infrastructure.Data.Configurations;

                public class ConfigurationFileConfiguration : IEntityTypeConfiguration<ConfigurationFile>
                {
                    public void Configure(EntityTypeBuilder<ConfigurationFile> builder)
                    {
                        builder.HasKey(x => x.ConfigurationFileId);
                        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
                        builder.Property(x => x.Path).IsRequired().HasMaxLength(500);
                        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
                        builder.HasMany(x => x.Items)
                            .WithOne(x => x.ConfigurationFile)
                            .HasForeignKey(x => x.ConfigurationFileId)
                            .OnDelete(DeleteBehavior.Cascade);
                    }
                }
                """
        });

        project.Files.Add(new FileModel("ConfigurationFileItemConfiguration", configurationsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.ConfigurationManagement.Core.Entities;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                namespace EventMonitoring.ConfigurationManagement.Infrastructure.Data.Configurations;

                public class ConfigurationFileItemConfiguration : IEntityTypeConfiguration<ConfigurationFileItem>
                {
                    public void Configure(EntityTypeBuilder<ConfigurationFileItem> builder)
                    {
                        builder.HasKey(x => x.ConfigurationFileItemId);
                        builder.Property(x => x.Key).IsRequired().HasMaxLength(200);
                        builder.Property(x => x.Value).IsRequired();
                    }
                }
                """
        });

        // Repositories
        project.Files.Add(new FileModel("ConfigurationFileRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.ConfigurationManagement.Core.Entities;
                using EventMonitoring.ConfigurationManagement.Core.Interfaces;
                using EventMonitoring.ConfigurationManagement.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace EventMonitoring.ConfigurationManagement.Infrastructure.Repositories;

                public class ConfigurationFileRepository : IConfigurationFileRepository
                {
                    private readonly ConfigurationManagementDbContext context;

                    public ConfigurationFileRepository(ConfigurationManagementDbContext context)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                    }

                    public async Task<ConfigurationFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
                    {
                        return await context.ConfigurationFiles
                            .Include(x => x.Items)
                            .FirstOrDefaultAsync(x => x.ConfigurationFileId == id, cancellationToken);
                    }

                    public async Task<ConfigurationFile?> GetByPathAsync(string path, CancellationToken cancellationToken = default)
                    {
                        return await context.ConfigurationFiles
                            .Include(x => x.Items)
                            .FirstOrDefaultAsync(x => x.Path == path, cancellationToken);
                    }

                    public async Task<IEnumerable<ConfigurationFile>> GetAllAsync(CancellationToken cancellationToken = default)
                    {
                        return await context.ConfigurationFiles
                            .Include(x => x.Items)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<ConfigurationFile> AddAsync(ConfigurationFile configurationFile, CancellationToken cancellationToken = default)
                    {
                        await context.ConfigurationFiles.AddAsync(configurationFile, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return configurationFile;
                    }

                    public async Task UpdateAsync(ConfigurationFile configurationFile, CancellationToken cancellationToken = default)
                    {
                        context.ConfigurationFiles.Update(configurationFile);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
                    {
                        var configurationFile = await context.ConfigurationFiles.FindAsync(new object[] { id }, cancellationToken);
                        if (configurationFile != null)
                        {
                            context.ConfigurationFiles.Remove(configurationFile);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("ConfigurationFileItemRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.ConfigurationManagement.Core.Entities;
                using EventMonitoring.ConfigurationManagement.Core.Interfaces;
                using EventMonitoring.ConfigurationManagement.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                namespace EventMonitoring.ConfigurationManagement.Infrastructure.Repositories;

                public class ConfigurationFileItemRepository : IConfigurationFileItemRepository
                {
                    private readonly ConfigurationManagementDbContext context;

                    public ConfigurationFileItemRepository(ConfigurationManagementDbContext context)
                    {
                        this.context = context ?? throw new ArgumentNullException(nameof(context));
                    }

                    public async Task<ConfigurationFileItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
                    {
                        return await context.ConfigurationFileItems.FindAsync(new object[] { id }, cancellationToken);
                    }

                    public async Task<IEnumerable<ConfigurationFileItem>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default)
                    {
                        return await context.ConfigurationFileItems
                            .Where(x => x.ConfigurationFileId == fileId)
                            .ToListAsync(cancellationToken);
                    }

                    public async Task<ConfigurationFileItem> AddAsync(ConfigurationFileItem item, CancellationToken cancellationToken = default)
                    {
                        await context.ConfigurationFileItems.AddAsync(item, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return item;
                    }

                    public async Task UpdateAsync(ConfigurationFileItem item, CancellationToken cancellationToken = default)
                    {
                        context.ConfigurationFileItems.Update(item);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
                    {
                        var item = await context.ConfigurationFileItems.FindAsync(new object[] { id }, cancellationToken);
                        if (item != null)
                        {
                            context.ConfigurationFileItems.Remove(item);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        // ConfigureServices
        project.Files.Add(new FileModel("ConfigureServices", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.ConfigurationManagement.Core.Interfaces;
                using EventMonitoring.ConfigurationManagement.Infrastructure.Data;
                using EventMonitoring.ConfigurationManagement.Infrastructure.Repositories;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.Extensions.Configuration;
                using Microsoft.Extensions.DependencyInjection;

                namespace EventMonitoring.ConfigurationManagement.Infrastructure;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<ConfigurationManagementDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

                        services.AddScoped<IConfigurationFileRepository, ConfigurationFileRepository>();
                        services.AddScoped<IConfigurationFileItemRepository, ConfigurationFileItemRepository>();

                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding ConfigurationManagement.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // FileController - serves configuration data to frontend and other services
        project.Files.Add(new FileModel("FileController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.ConfigurationManagement.Core.DTOs;
                using EventMonitoring.ConfigurationManagement.Core.Interfaces;
                using Microsoft.AspNetCore.Mvc;

                namespace EventMonitoring.ConfigurationManagement.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class FileController : ControllerBase
                {
                    private readonly IConfigurationFileRepository configurationFileRepository;
                    private readonly ILogger<FileController> logger;

                    public FileController(
                        IConfigurationFileRepository configurationFileRepository,
                        ILogger<FileController> logger)
                    {
                        this.configurationFileRepository = configurationFileRepository ?? throw new ArgumentNullException(nameof(configurationFileRepository));
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                    }

                    [HttpGet]
                    public async Task<ActionResult<IEnumerable<ConfigurationFileDto>>> GetAll(CancellationToken cancellationToken)
                    {
                        logger.LogInformation("Getting all configuration files");
                        var files = await configurationFileRepository.GetAllAsync(cancellationToken);
                        var dtos = files.Select(f => new ConfigurationFileDto
                        {
                            ConfigurationFileId = f.ConfigurationFileId,
                            Name = f.Name,
                            Path = f.Path,
                            Description = f.Description,
                            Version = f.Version,
                            IsActive = f.IsActive,
                            CreatedAt = f.CreatedAt,
                            UpdatedAt = f.UpdatedAt
                        });
                        return Ok(dtos);
                    }

                    [HttpGet("{id}")]
                    public async Task<ActionResult<ConfigurationFileDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        logger.LogInformation("Getting configuration file by id: {Id}", id);
                        var file = await configurationFileRepository.GetByIdAsync(id, cancellationToken);
                        if (file == null)
                        {
                            return NotFound();
                        }

                        var dto = new ConfigurationFileDto
                        {
                            ConfigurationFileId = file.ConfigurationFileId,
                            Name = file.Name,
                            Path = file.Path,
                            Description = file.Description,
                            Version = file.Version,
                            IsActive = file.IsActive,
                            CreatedAt = file.CreatedAt,
                            UpdatedAt = file.UpdatedAt
                        };
                        return Ok(dto);
                    }

                    [HttpGet("by-path")]
                    public async Task<ActionResult<ConfigurationFileDto>> GetByPath([FromQuery] string path, CancellationToken cancellationToken)
                    {
                        logger.LogInformation("Getting configuration file by path: {Path}", path);
                        var file = await configurationFileRepository.GetByPathAsync(path, cancellationToken);
                        if (file == null)
                        {
                            return NotFound();
                        }

                        var dto = new ConfigurationFileDto
                        {
                            ConfigurationFileId = file.ConfigurationFileId,
                            Name = file.Name,
                            Path = file.Path,
                            Description = file.Description,
                            Version = file.Version,
                            IsActive = file.IsActive,
                            CreatedAt = file.CreatedAt,
                            UpdatedAt = file.UpdatedAt
                        };
                        return Ok(dto);
                    }
                }
                """
        });

        // appsettings.json with SQL Express connection string
        project.Files.Add(new FileModel("appsettings", project.Directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=ConfigurationManagement;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
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

        // Program.cs with database migration and seeding
        project.Files.Add(new FileModel("Program", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.ConfigurationManagement.Infrastructure;
                using EventMonitoring.ConfigurationManagement.Infrastructure.Data;
                using Microsoft.EntityFrameworkCore;

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddInfrastructureServices(builder.Configuration);

                var app = builder.Build();

                // Apply migrations and seed database
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ConfigurationManagementDbContext>();
                    dbContext.Database.Migrate();
                    await DatabaseSeeder.SeedAsync(dbContext);
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

    public void AddInfrastructureSeederFiles(ProjectModel project)
    {
        logger.LogInformation("Adding ConfigurationManagement.Infrastructure seeder files");

        var dataDir = Path.Combine(project.Directory, "Data");

        // Database seeder with 10 configuration files
        project.Files.Add(new FileModel("DatabaseSeeder", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using EventMonitoring.ConfigurationManagement.Core.Entities;
                using Microsoft.EntityFrameworkCore;

                namespace EventMonitoring.ConfigurationManagement.Infrastructure.Data;

                public static class DatabaseSeeder
                {
                    public static async Task SeedAsync(ConfigurationManagementDbContext context)
                    {
                        if (await context.ConfigurationFiles.AnyAsync())
                        {
                            return; // Database already seeded
                        }

                        var configurationFiles = new List<ConfigurationFile>
                        {
                            new ConfigurationFile
                            {
                                ConfigurationFileId = Guid.NewGuid(),
                                Name = "Application Settings",
                                Path = "/config/appsettings.json",
                                ContentType = "application/json",
                                Description = "Main application configuration settings",
                                Version = "1.0.0",
                                Items = new List<ConfigurationFileItem>
                                {
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "AppName", Value = "EventMonitoring", ValueType = ConfigurationValueType.String },
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "MaxConnections", Value = "100", ValueType = ConfigurationValueType.Integer },
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "EnableLogging", Value = "true", ValueType = ConfigurationValueType.Boolean }
                                }
                            },
                            new ConfigurationFile
                            {
                                ConfigurationFileId = Guid.NewGuid(),
                                Name = "Telemetry Settings",
                                Path = "/config/telemetry.json",
                                ContentType = "application/json",
                                Description = "Telemetry streaming configuration",
                                Version = "1.0.0",
                                Items = new List<ConfigurationFileItem>
                                {
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "UpdateRateMs", Value = "200", ValueType = ConfigurationValueType.Integer },
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "BufferSize", Value = "1000", ValueType = ConfigurationValueType.Integer }
                                }
                            },
                            new ConfigurationFile
                            {
                                ConfigurationFileId = Guid.NewGuid(),
                                Name = "Database Settings",
                                Path = "/config/database.json",
                                ContentType = "application/json",
                                Description = "Database connection and pool settings",
                                Version = "1.0.0",
                                Items = new List<ConfigurationFileItem>
                                {
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "ConnectionTimeout", Value = "30", ValueType = ConfigurationValueType.Integer },
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "MaxPoolSize", Value = "50", ValueType = ConfigurationValueType.Integer }
                                }
                            },
                            new ConfigurationFile
                            {
                                ConfigurationFileId = Guid.NewGuid(),
                                Name = "Logging Configuration",
                                Path = "/config/logging.json",
                                ContentType = "application/json",
                                Description = "Logging levels and output configuration",
                                Version = "1.0.0",
                                Items = new List<ConfigurationFileItem>
                                {
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "MinLevel", Value = "Information", ValueType = ConfigurationValueType.String },
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "WriteToFile", Value = "true", ValueType = ConfigurationValueType.Boolean }
                                }
                            },
                            new ConfigurationFile
                            {
                                ConfigurationFileId = Guid.NewGuid(),
                                Name = "Security Settings",
                                Path = "/config/security.json",
                                ContentType = "application/json",
                                Description = "Security and authentication configuration",
                                Version = "1.0.0",
                                Items = new List<ConfigurationFileItem>
                                {
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "TokenExpiryMinutes", Value = "60", ValueType = ConfigurationValueType.Integer },
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "RequireHttps", Value = "true", ValueType = ConfigurationValueType.Boolean }
                                }
                            },
                            new ConfigurationFile
                            {
                                ConfigurationFileId = Guid.NewGuid(),
                                Name = "Redis Cache Settings",
                                Path = "/config/redis.json",
                                ContentType = "application/json",
                                Description = "Redis cache and pub/sub configuration",
                                Version = "1.0.0",
                                Items = new List<ConfigurationFileItem>
                                {
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "Host", Value = "localhost", ValueType = ConfigurationValueType.String },
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "Port", Value = "6379", ValueType = ConfigurationValueType.Integer }
                                }
                            },
                            new ConfigurationFile
                            {
                                ConfigurationFileId = Guid.NewGuid(),
                                Name = "API Gateway Settings",
                                Path = "/config/gateway.json",
                                ContentType = "application/json",
                                Description = "API Gateway routing and CORS configuration",
                                Version = "1.0.0",
                                Items = new List<ConfigurationFileItem>
                                {
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "CorsOrigin", Value = "http://localhost:4200", ValueType = ConfigurationValueType.String },
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "RateLimitPerMinute", Value = "1000", ValueType = ConfigurationValueType.Integer }
                                }
                            },
                            new ConfigurationFile
                            {
                                ConfigurationFileId = Guid.NewGuid(),
                                Name = "Historical Data Settings",
                                Path = "/config/historical.json",
                                ContentType = "application/json",
                                Description = "Historical telemetry storage configuration",
                                Version = "1.0.0",
                                Items = new List<ConfigurationFileItem>
                                {
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "RetentionDays", Value = "90", ValueType = ConfigurationValueType.Integer },
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "PageSize", Value = "100", ValueType = ConfigurationValueType.Integer }
                                }
                            },
                            new ConfigurationFile
                            {
                                ConfigurationFileId = Guid.NewGuid(),
                                Name = "Dashboard Settings",
                                Path = "/config/dashboard.json",
                                ContentType = "application/json",
                                Description = "Dashboard layout and tile configuration",
                                Version = "1.0.0",
                                Items = new List<ConfigurationFileItem>
                                {
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "DefaultColumns", Value = "12", ValueType = ConfigurationValueType.Integer },
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "AnimationEnabled", Value = "true", ValueType = ConfigurationValueType.Boolean }
                                }
                            },
                            new ConfigurationFile
                            {
                                ConfigurationFileId = Guid.NewGuid(),
                                Name = "Space Vehicle Telemetry",
                                Path = "/config/vehicle-telemetry.json",
                                ContentType = "application/json",
                                Description = "Space vehicle telemetry types and thresholds",
                                Version = "1.0.0",
                                Items = new List<ConfigurationFileItem>
                                {
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "TelemetryTypes", Value = "50", ValueType = ConfigurationValueType.Integer },
                                    new ConfigurationFileItem { ConfigurationFileItemId = Guid.NewGuid(), Key = "AlertThreshold", Value = "0.95", ValueType = ConfigurationValueType.String }
                                }
                            }
                        };

                        await context.ConfigurationFiles.AddRangeAsync(configurationFiles);
                        await context.SaveChangesAsync();
                    }
                }
                """
        });
    }
}
