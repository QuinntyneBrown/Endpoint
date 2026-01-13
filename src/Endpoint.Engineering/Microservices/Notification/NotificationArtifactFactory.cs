// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Notification;

public class NotificationArtifactFactory : INotificationArtifactFactory
{
    private readonly ILogger<NotificationArtifactFactory> logger;

    public NotificationArtifactFactory(ILogger<NotificationArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Notification.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("Notification", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Notification.Core.Entities;

                public class Notification
                {
                    public Guid NotificationId { get; set; }
                    public Guid UserId { get; set; }
                    public Guid? TemplateId { get; set; }
                    public required string Title { get; set; }
                    public required string Body { get; set; }
                    public NotificationType Type { get; set; } = NotificationType.Info;
                    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? SentAt { get; set; }
                    public DateTime? DeliveredAt { get; set; }
                    public NotificationTemplate? Template { get; set; }
                }

                public enum NotificationType { Info, Warning, Error, Success }
                public enum NotificationStatus { Pending, Sent, Delivered, Failed }
                """
        });

        project.Files.Add(new FileModel("NotificationTemplate", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Notification.Core.Entities;

                public class NotificationTemplate
                {
                    public Guid TemplateId { get; set; }
                    public required string Name { get; set; }
                    public required string Subject { get; set; }
                    public required string BodyTemplate { get; set; }
                    public NotificationType Type { get; set; } = NotificationType.Info;
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("NotificationPreference", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Notification.Core.Entities;

                public class NotificationPreference
                {
                    public Guid PreferenceId { get; set; }
                    public Guid UserId { get; set; }
                    public bool EmailEnabled { get; set; } = true;
                    public bool PushEnabled { get; set; } = true;
                    public bool SmsEnabled { get; set; } = false;
                    public bool InAppEnabled { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("NotificationHistory", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Notification.Core.Entities;

                public class NotificationHistory
                {
                    public Guid HistoryId { get; set; }
                    public Guid NotificationId { get; set; }
                    public Notification Notification { get; set; } = null!;
                    public required string Action { get; set; }
                    public string? Details { get; set; }
                    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("INotificationRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Notification.Core.Entities;

                namespace Notification.Core.Interfaces;

                public interface INotificationRepository
                {
                    Task<Entities.Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Entities.Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Entities.Notification>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<Entities.Notification> AddAsync(Entities.Notification notification, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Entities.Notification notification, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid notificationId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("INotificationService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Notification.Core.DTOs;

                namespace Notification.Core.Interfaces;

                public interface INotificationService
                {
                    Task<NotificationDto> SendAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);
                    Task<NotificationDto?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<NotificationDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
                    Task MarkAsDeliveredAsync(Guid notificationId, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("NotificationSentEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Notification.Core.Events;

                public sealed class NotificationSentEvent
                {
                    public Guid NotificationId { get; init; }
                    public Guid UserId { get; init; }
                    public required string Title { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("NotificationDeliveredEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Notification.Core.Events;

                public sealed class NotificationDeliveredEvent
                {
                    public Guid NotificationId { get; init; }
                    public Guid UserId { get; init; }
                    public DateTime DeliveredAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("NotificationFailedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Notification.Core.Events;

                public sealed class NotificationFailedEvent
                {
                    public Guid NotificationId { get; init; }
                    public Guid UserId { get; init; }
                    public required string Reason { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("NotificationDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Notification.Core.DTOs;

                public sealed class NotificationDto
                {
                    public Guid NotificationId { get; init; }
                    public Guid UserId { get; init; }
                    public required string Title { get; init; }
                    public required string Body { get; init; }
                    public string Type { get; init; } = "Info";
                    public string Status { get; init; } = "Pending";
                    public DateTime CreatedAt { get; init; }
                    public DateTime? SentAt { get; init; }
                    public DateTime? DeliveredAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("SendNotificationRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Notification.Core.DTOs;

                public sealed class SendNotificationRequest
                {
                    [Required]
                    public Guid UserId { get; init; }

                    [Required]
                    public required string Title { get; init; }

                    [Required]
                    public required string Body { get; init; }

                    public Guid? TemplateId { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Notification.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("NotificationDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Notification.Core.Entities;

                namespace Notification.Infrastructure.Data;

                public class NotificationDbContext : DbContext
                {
                    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

                    public DbSet<Core.Entities.Notification> Notifications => Set<Core.Entities.Notification>();
                    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
                    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
                    public DbSet<NotificationHistory> NotificationHistories => Set<NotificationHistory>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<Core.Entities.Notification>(entity =>
                        {
                            entity.HasKey(n => n.NotificationId);
                            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
                            entity.Property(n => n.Body).IsRequired();
                            entity.HasOne(n => n.Template).WithMany().HasForeignKey(n => n.TemplateId);
                        });

                        modelBuilder.Entity<NotificationTemplate>(entity =>
                        {
                            entity.HasKey(t => t.TemplateId);
                            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                            entity.HasIndex(t => t.Name).IsUnique();
                        });

                        modelBuilder.Entity<NotificationPreference>(entity =>
                        {
                            entity.HasKey(p => p.PreferenceId);
                            entity.HasIndex(p => p.UserId).IsUnique();
                        });

                        modelBuilder.Entity<NotificationHistory>(entity =>
                        {
                            entity.HasKey(h => h.HistoryId);
                            entity.HasOne(h => h.Notification).WithMany().HasForeignKey(h => h.NotificationId);
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("NotificationRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Notification.Core.Interfaces;
                using Notification.Infrastructure.Data;

                namespace Notification.Infrastructure.Repositories;

                public class NotificationRepository : INotificationRepository
                {
                    private readonly NotificationDbContext context;

                    public NotificationRepository(NotificationDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Core.Entities.Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
                        => await context.Notifications.Include(n => n.Template).FirstOrDefaultAsync(n => n.NotificationId == notificationId, cancellationToken);

                    public async Task<IEnumerable<Core.Entities.Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
                        => await context.Notifications.Include(n => n.Template).Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);

                    public async Task<IEnumerable<Core.Entities.Notification>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.Notifications.Include(n => n.Template).ToListAsync(cancellationToken);

                    public async Task<Core.Entities.Notification> AddAsync(Core.Entities.Notification notification, CancellationToken cancellationToken = default)
                    {
                        notification.NotificationId = Guid.NewGuid();
                        await context.Notifications.AddAsync(notification, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return notification;
                    }

                    public async Task UpdateAsync(Core.Entities.Notification notification, CancellationToken cancellationToken = default)
                    {
                        context.Notifications.Update(notification);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid notificationId, CancellationToken cancellationToken = default)
                    {
                        var notification = await context.Notifications.FindAsync(new object[] { notificationId }, cancellationToken);
                        if (notification != null)
                        {
                            context.Notifications.Remove(notification);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("NotificationService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Notification.Core.DTOs;
                using Notification.Core.Entities;
                using Notification.Core.Interfaces;

                namespace Notification.Infrastructure.Services;

                public class NotificationService : INotificationService
                {
                    private readonly INotificationRepository repository;

                    public NotificationService(INotificationRepository repository)
                    {
                        this.repository = repository;
                    }

                    public async Task<NotificationDto> SendAsync(SendNotificationRequest request, CancellationToken cancellationToken = default)
                    {
                        var notification = new Core.Entities.Notification
                        {
                            UserId = request.UserId,
                            Title = request.Title,
                            Body = request.Body,
                            TemplateId = request.TemplateId,
                            Status = NotificationStatus.Sent,
                            SentAt = DateTime.UtcNow
                        };

                        var created = await repository.AddAsync(notification, cancellationToken);

                        return new NotificationDto
                        {
                            NotificationId = created.NotificationId,
                            UserId = created.UserId,
                            Title = created.Title,
                            Body = created.Body,
                            Type = created.Type.ToString(),
                            Status = created.Status.ToString(),
                            CreatedAt = created.CreatedAt,
                            SentAt = created.SentAt
                        };
                    }

                    public async Task<NotificationDto?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken = default)
                    {
                        var notification = await repository.GetByIdAsync(notificationId, cancellationToken);
                        if (notification == null) return null;

                        return new NotificationDto
                        {
                            NotificationId = notification.NotificationId,
                            UserId = notification.UserId,
                            Title = notification.Title,
                            Body = notification.Body,
                            Type = notification.Type.ToString(),
                            Status = notification.Status.ToString(),
                            CreatedAt = notification.CreatedAt,
                            SentAt = notification.SentAt,
                            DeliveredAt = notification.DeliveredAt
                        };
                    }

                    public async Task<IEnumerable<NotificationDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
                    {
                        var notifications = await repository.GetByUserIdAsync(userId, cancellationToken);
                        return notifications.Select(n => new NotificationDto
                        {
                            NotificationId = n.NotificationId,
                            UserId = n.UserId,
                            Title = n.Title,
                            Body = n.Body,
                            Type = n.Type.ToString(),
                            Status = n.Status.ToString(),
                            CreatedAt = n.CreatedAt,
                            SentAt = n.SentAt,
                            DeliveredAt = n.DeliveredAt
                        });
                    }

                    public async Task MarkAsDeliveredAsync(Guid notificationId, CancellationToken cancellationToken = default)
                    {
                        var notification = await repository.GetByIdAsync(notificationId, cancellationToken);
                        if (notification != null)
                        {
                            notification.Status = NotificationStatus.Delivered;
                            notification.DeliveredAt = DateTime.UtcNow;
                            await repository.UpdateAsync(notification, cancellationToken);
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
                using Notification.Core.Interfaces;
                using Notification.Infrastructure.Data;
                using Notification.Infrastructure.Repositories;
                using Notification.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<NotificationDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("NotificationDb") ??
                                @"Server=.\SQLEXPRESS;Database=NotificationDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<INotificationRepository, NotificationRepository>();
                        services.AddScoped<INotificationService, NotificationService>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Notification.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("NotificationsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Notification.Core.DTOs;
                using Notification.Core.Interfaces;

                namespace Notification.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class NotificationsController : ControllerBase
                {
                    private readonly INotificationService service;

                    public NotificationsController(INotificationService service)
                    {
                        this.service = service;
                    }

                    [HttpPost("send")]
                    public async Task<ActionResult<NotificationDto>> Send([FromBody] SendNotificationRequest request, CancellationToken cancellationToken)
                    {
                        var notification = await service.SendAsync(request, cancellationToken);
                        return CreatedAtAction(nameof(GetById), new { id = notification.NotificationId }, notification);
                    }

                    [HttpGet("{id:guid}")]
                    public async Task<ActionResult<NotificationDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var notification = await service.GetByIdAsync(id, cancellationToken);
                        if (notification == null) return NotFound();
                        return Ok(notification);
                    }

                    [HttpGet("user/{userId:guid}")]
                    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetByUserId(Guid userId, CancellationToken cancellationToken)
                    {
                        var notifications = await service.GetByUserIdAsync(userId, cancellationToken);
                        return Ok(notifications);
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

                builder.Services.AddNotificationInfrastructure(builder.Configuration);
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
                    "NotificationDb": "Server=.\\SQLEXPRESS;Database=NotificationDb;Trusted_Connection=True;TrustServerCertificate=True"
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
