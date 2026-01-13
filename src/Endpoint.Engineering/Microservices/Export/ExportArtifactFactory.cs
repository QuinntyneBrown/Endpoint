// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Export;

public class ExportArtifactFactory : IExportArtifactFactory
{
    private readonly ILogger<ExportArtifactFactory> logger;

    public ExportArtifactFactory(ILogger<ExportArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Export.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("ExportJob", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Export.Core.Entities;

                public class ExportJob
                {
                    public Guid ExportJobId { get; set; }
                    public Guid UserId { get; set; }
                    public Guid? TemplateId { get; set; }
                    public required string Name { get; set; }
                    public required string Format { get; set; }
                    public ExportJobStatus Status { get; set; } = ExportJobStatus.Pending;
                    public string? Parameters { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? StartedAt { get; set; }
                    public DateTime? CompletedAt { get; set; }
                    public ExportTemplate? Template { get; set; }
                    public ExportResult? Result { get; set; }
                }

                public enum ExportJobStatus { Pending, Processing, Completed, Failed, Cancelled }
                """
        });

        project.Files.Add(new FileModel("ExportTemplate", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Export.Core.Entities;

                public class ExportTemplate
                {
                    public Guid TemplateId { get; set; }
                    public required string Name { get; set; }
                    public required string Description { get; set; }
                    public required string Format { get; set; }
                    public required string TemplateDefinition { get; set; }
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("ExportResult", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Export.Core.Entities;

                public class ExportResult
                {
                    public Guid ExportResultId { get; set; }
                    public Guid ExportJobId { get; set; }
                    public ExportJob ExportJob { get; set; } = null!;
                    public required string FileName { get; set; }
                    public required string ContentType { get; set; }
                    public required string FilePath { get; set; }
                    public long FileSize { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? ExpiresAt { get; set; }
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IExportRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Export.Core.Entities;

                namespace Export.Core.Interfaces;

                public interface IExportRepository
                {
                    Task<ExportJob?> GetByIdAsync(Guid exportJobId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ExportJob>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ExportJob>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<ExportJob> AddAsync(ExportJob exportJob, CancellationToken cancellationToken = default);
                    Task UpdateAsync(ExportJob exportJob, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid exportJobId, CancellationToken cancellationToken = default);
                    Task<ExportResult?> GetResultByJobIdAsync(Guid exportJobId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IExportService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Export.Core.DTOs;

                namespace Export.Core.Interfaces;

                public interface IExportService
                {
                    Task<ExportJobDto> GenerateAsync(GenerateExportRequest request, CancellationToken cancellationToken = default);
                    Task<ExportJobDto?> GetJobByIdAsync(Guid exportJobId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ExportJobDto>> GetJobsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
                    Task<ExportDownloadDto?> GetDownloadAsync(Guid exportJobId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IReportGenerator", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Export.Core.Entities;

                namespace Export.Core.Interfaces;

                public interface IReportGenerator
                {
                    Task<ExportResult> GenerateReportAsync(ExportJob job, CancellationToken cancellationToken = default);
                    bool SupportsFormat(string format);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("ExportCompletedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Export.Core.Events;

                public sealed class ExportCompletedEvent
                {
                    public Guid ExportJobId { get; init; }
                    public Guid UserId { get; init; }
                    public required string FileName { get; init; }
                    public long FileSize { get; init; }
                    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("ExportFailedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Export.Core.Events;

                public sealed class ExportFailedEvent
                {
                    public Guid ExportJobId { get; init; }
                    public Guid UserId { get; init; }
                    public required string Reason { get; init; }
                    public DateTime FailedAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("ExportJobDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Export.Core.DTOs;

                public sealed class ExportJobDto
                {
                    public Guid ExportJobId { get; init; }
                    public Guid UserId { get; init; }
                    public required string Name { get; init; }
                    public required string Format { get; init; }
                    public string Status { get; init; } = "Pending";
                    public DateTime CreatedAt { get; init; }
                    public DateTime? StartedAt { get; init; }
                    public DateTime? CompletedAt { get; init; }
                    public string? FileName { get; init; }
                    public long? FileSize { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("GenerateExportRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Export.Core.DTOs;

                public sealed class GenerateExportRequest
                {
                    [Required]
                    public Guid UserId { get; init; }

                    [Required]
                    public required string Name { get; init; }

                    [Required]
                    public required string Format { get; init; }

                    public Guid? TemplateId { get; init; }

                    public string? Parameters { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("ExportDownloadDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Export.Core.DTOs;

                public sealed class ExportDownloadDto
                {
                    public Guid ExportJobId { get; init; }
                    public required string FileName { get; init; }
                    public required string ContentType { get; init; }
                    public required string FilePath { get; init; }
                    public long FileSize { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Export.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("ExportDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Export.Core.Entities;

                namespace Export.Infrastructure.Data;

                public class ExportDbContext : DbContext
                {
                    public ExportDbContext(DbContextOptions<ExportDbContext> options) : base(options) { }

                    public DbSet<ExportJob> ExportJobs => Set<ExportJob>();
                    public DbSet<ExportTemplate> ExportTemplates => Set<ExportTemplate>();
                    public DbSet<ExportResult> ExportResults => Set<ExportResult>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<ExportJob>(entity =>
                        {
                            entity.HasKey(e => e.ExportJobId);
                            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                            entity.Property(e => e.Format).IsRequired().HasMaxLength(50);
                            entity.HasOne(e => e.Template).WithMany().HasForeignKey(e => e.TemplateId);
                            entity.HasOne(e => e.Result).WithOne(r => r.ExportJob).HasForeignKey<ExportResult>(r => r.ExportJobId);
                        });

                        modelBuilder.Entity<ExportTemplate>(entity =>
                        {
                            entity.HasKey(t => t.TemplateId);
                            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                            entity.HasIndex(t => t.Name).IsUnique();
                        });

                        modelBuilder.Entity<ExportResult>(entity =>
                        {
                            entity.HasKey(r => r.ExportResultId);
                            entity.Property(r => r.FileName).IsRequired().HasMaxLength(255);
                            entity.Property(r => r.ContentType).IsRequired().HasMaxLength(100);
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("ExportRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Export.Core.Entities;
                using Export.Core.Interfaces;
                using Export.Infrastructure.Data;

                namespace Export.Infrastructure.Repositories;

                public class ExportRepository : IExportRepository
                {
                    private readonly ExportDbContext context;

                    public ExportRepository(ExportDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<ExportJob?> GetByIdAsync(Guid exportJobId, CancellationToken cancellationToken = default)
                        => await context.ExportJobs.Include(e => e.Template).Include(e => e.Result).FirstOrDefaultAsync(e => e.ExportJobId == exportJobId, cancellationToken);

                    public async Task<IEnumerable<ExportJob>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
                        => await context.ExportJobs.Include(e => e.Template).Include(e => e.Result).Where(e => e.UserId == userId).OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);

                    public async Task<IEnumerable<ExportJob>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.ExportJobs.Include(e => e.Template).Include(e => e.Result).ToListAsync(cancellationToken);

                    public async Task<ExportJob> AddAsync(ExportJob exportJob, CancellationToken cancellationToken = default)
                    {
                        exportJob.ExportJobId = Guid.NewGuid();
                        await context.ExportJobs.AddAsync(exportJob, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return exportJob;
                    }

                    public async Task UpdateAsync(ExportJob exportJob, CancellationToken cancellationToken = default)
                    {
                        context.ExportJobs.Update(exportJob);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid exportJobId, CancellationToken cancellationToken = default)
                    {
                        var exportJob = await context.ExportJobs.FindAsync(new object[] { exportJobId }, cancellationToken);
                        if (exportJob != null)
                        {
                            context.ExportJobs.Remove(exportJob);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task<ExportResult?> GetResultByJobIdAsync(Guid exportJobId, CancellationToken cancellationToken = default)
                        => await context.ExportResults.FirstOrDefaultAsync(r => r.ExportJobId == exportJobId, cancellationToken);
                }
                """
        });

        project.Files.Add(new FileModel("ExportService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Export.Core.DTOs;
                using Export.Core.Entities;
                using Export.Core.Interfaces;

                namespace Export.Infrastructure.Services;

                public class ExportService : IExportService
                {
                    private readonly IExportRepository repository;
                    private readonly IReportGenerator reportGenerator;

                    public ExportService(IExportRepository repository, IReportGenerator reportGenerator)
                    {
                        this.repository = repository;
                        this.reportGenerator = reportGenerator;
                    }

                    public async Task<ExportJobDto> GenerateAsync(GenerateExportRequest request, CancellationToken cancellationToken = default)
                    {
                        var exportJob = new ExportJob
                        {
                            UserId = request.UserId,
                            Name = request.Name,
                            Format = request.Format,
                            TemplateId = request.TemplateId,
                            Parameters = request.Parameters,
                            Status = ExportJobStatus.Processing,
                            StartedAt = DateTime.UtcNow
                        };

                        var created = await repository.AddAsync(exportJob, cancellationToken);

                        // Generate the report asynchronously
                        try
                        {
                            var result = await reportGenerator.GenerateReportAsync(created, cancellationToken);
                            created.Status = ExportJobStatus.Completed;
                            created.CompletedAt = DateTime.UtcNow;
                            created.Result = result;
                            await repository.UpdateAsync(created, cancellationToken);
                        }
                        catch
                        {
                            created.Status = ExportJobStatus.Failed;
                            await repository.UpdateAsync(created, cancellationToken);
                            throw;
                        }

                        return new ExportJobDto
                        {
                            ExportJobId = created.ExportJobId,
                            UserId = created.UserId,
                            Name = created.Name,
                            Format = created.Format,
                            Status = created.Status.ToString(),
                            CreatedAt = created.CreatedAt,
                            StartedAt = created.StartedAt,
                            CompletedAt = created.CompletedAt,
                            FileName = created.Result?.FileName,
                            FileSize = created.Result?.FileSize
                        };
                    }

                    public async Task<ExportJobDto?> GetJobByIdAsync(Guid exportJobId, CancellationToken cancellationToken = default)
                    {
                        var exportJob = await repository.GetByIdAsync(exportJobId, cancellationToken);
                        if (exportJob == null) return null;

                        return new ExportJobDto
                        {
                            ExportJobId = exportJob.ExportJobId,
                            UserId = exportJob.UserId,
                            Name = exportJob.Name,
                            Format = exportJob.Format,
                            Status = exportJob.Status.ToString(),
                            CreatedAt = exportJob.CreatedAt,
                            StartedAt = exportJob.StartedAt,
                            CompletedAt = exportJob.CompletedAt,
                            FileName = exportJob.Result?.FileName,
                            FileSize = exportJob.Result?.FileSize
                        };
                    }

                    public async Task<IEnumerable<ExportJobDto>> GetJobsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
                    {
                        var exportJobs = await repository.GetByUserIdAsync(userId, cancellationToken);
                        return exportJobs.Select(e => new ExportJobDto
                        {
                            ExportJobId = e.ExportJobId,
                            UserId = e.UserId,
                            Name = e.Name,
                            Format = e.Format,
                            Status = e.Status.ToString(),
                            CreatedAt = e.CreatedAt,
                            StartedAt = e.StartedAt,
                            CompletedAt = e.CompletedAt,
                            FileName = e.Result?.FileName,
                            FileSize = e.Result?.FileSize
                        });
                    }

                    public async Task<ExportDownloadDto?> GetDownloadAsync(Guid exportJobId, CancellationToken cancellationToken = default)
                    {
                        var result = await repository.GetResultByJobIdAsync(exportJobId, cancellationToken);
                        if (result == null) return null;

                        return new ExportDownloadDto
                        {
                            ExportJobId = result.ExportJobId,
                            FileName = result.FileName,
                            ContentType = result.ContentType,
                            FilePath = result.FilePath,
                            FileSize = result.FileSize
                        };
                    }
                }
                """
        });

        project.Files.Add(new FileModel("DefaultReportGenerator", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Export.Core.Entities;
                using Export.Core.Interfaces;

                namespace Export.Infrastructure.Services;

                public class DefaultReportGenerator : IReportGenerator
                {
                    private static readonly HashSet<string> SupportedFormats = new(StringComparer.OrdinalIgnoreCase) { "pdf", "csv", "xlsx", "json" };

                    public async Task<ExportResult> GenerateReportAsync(ExportJob job, CancellationToken cancellationToken = default)
                    {
                        // Simulate report generation
                        await Task.Delay(100, cancellationToken);

                        var fileName = $"{job.Name}_{DateTime.UtcNow:yyyyMMddHHmmss}.{job.Format.ToLowerInvariant()}";
                        var contentType = GetContentType(job.Format);
                        var filePath = Path.Combine("exports", job.UserId.ToString(), fileName);

                        return new ExportResult
                        {
                            ExportResultId = Guid.NewGuid(),
                            ExportJobId = job.ExportJobId,
                            FileName = fileName,
                            ContentType = contentType,
                            FilePath = filePath,
                            FileSize = 0,
                            ExpiresAt = DateTime.UtcNow.AddDays(7)
                        };
                    }

                    public bool SupportsFormat(string format) => SupportedFormats.Contains(format);

                    private static string GetContentType(string format) => format.ToLowerInvariant() switch
                    {
                        "pdf" => "application/pdf",
                        "csv" => "text/csv",
                        "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "json" => "application/json",
                        _ => "application/octet-stream"
                    };
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
                using Export.Core.Interfaces;
                using Export.Infrastructure.Data;
                using Export.Infrastructure.Repositories;
                using Export.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddExportInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<ExportDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("ExportDb") ??
                                @"Server=.\SQLEXPRESS;Database=ExportDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IExportRepository, ExportRepository>();
                        services.AddScoped<IExportService, ExportService>();
                        services.AddScoped<IReportGenerator, DefaultReportGenerator>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Export.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("ExportController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Export.Core.DTOs;
                using Export.Core.Interfaces;

                namespace Export.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class ExportController : ControllerBase
                {
                    private readonly IExportService service;

                    public ExportController(IExportService service)
                    {
                        this.service = service;
                    }

                    [HttpPost("generate")]
                    public async Task<ActionResult<ExportJobDto>> Generate([FromBody] GenerateExportRequest request, CancellationToken cancellationToken)
                    {
                        var exportJob = await service.GenerateAsync(request, cancellationToken);
                        return CreatedAtAction(nameof(GetJob), new { id = exportJob.ExportJobId }, exportJob);
                    }

                    [HttpGet("jobs/{id:guid}")]
                    public async Task<ActionResult<ExportJobDto>> GetJob(Guid id, CancellationToken cancellationToken)
                    {
                        var exportJob = await service.GetJobByIdAsync(id, cancellationToken);
                        if (exportJob == null) return NotFound();
                        return Ok(exportJob);
                    }

                    [HttpGet("download/{id:guid}")]
                    public async Task<ActionResult<ExportDownloadDto>> GetDownload(Guid id, CancellationToken cancellationToken)
                    {
                        var download = await service.GetDownloadAsync(id, cancellationToken);
                        if (download == null) return NotFound();
                        return Ok(download);
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

                builder.Services.AddExportInfrastructure(builder.Configuration);
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
                    "ExportDb": "Server=.\\SQLEXPRESS;Database=ExportDb;Trusted_Connection=True;TrustServerCertificate=True"
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
