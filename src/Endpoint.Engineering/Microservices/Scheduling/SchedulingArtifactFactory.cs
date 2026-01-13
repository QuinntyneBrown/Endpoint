// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Scheduling;

public class SchedulingArtifactFactory : ISchedulingArtifactFactory
{
    private readonly ILogger<SchedulingArtifactFactory> logger;

    public SchedulingArtifactFactory(ILogger<SchedulingArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Scheduling.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("Appointment", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Scheduling.Core.Entities;

                public class Appointment
                {
                    public Guid AppointmentId { get; set; }
                    public Guid CalendarId { get; set; }
                    public required string Title { get; set; }
                    public string? Description { get; set; }
                    public DateTime StartTime { get; set; }
                    public DateTime EndTime { get; set; }
                    public string? Location { get; set; }
                    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public Calendar Calendar { get; set; } = null!;
                    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
                }

                public enum AppointmentStatus { Scheduled, Confirmed, Cancelled, Completed }
                """
        });

        project.Files.Add(new FileModel("Reminder", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Scheduling.Core.Entities;

                public class Reminder
                {
                    public Guid ReminderId { get; set; }
                    public Guid AppointmentId { get; set; }
                    public DateTime ReminderTime { get; set; }
                    public ReminderType Type { get; set; } = ReminderType.Email;
                    public bool IsSent { get; set; } = false;
                    public DateTime? SentAt { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public Appointment Appointment { get; set; } = null!;
                }

                public enum ReminderType { Email, Push, Sms }
                """
        });

        project.Files.Add(new FileModel("RecurringEvent", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Scheduling.Core.Entities;

                public class RecurringEvent
                {
                    public Guid RecurringEventId { get; set; }
                    public Guid CalendarId { get; set; }
                    public required string Title { get; set; }
                    public string? Description { get; set; }
                    public TimeSpan Duration { get; set; }
                    public RecurrencePattern Pattern { get; set; } = RecurrencePattern.Weekly;
                    public int Interval { get; set; } = 1;
                    public DateTime StartDate { get; set; }
                    public DateTime? EndDate { get; set; }
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public Calendar Calendar { get; set; } = null!;
                }

                public enum RecurrencePattern { Daily, Weekly, Monthly, Yearly }
                """
        });

        project.Files.Add(new FileModel("Calendar", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Scheduling.Core.Entities;

                public class Calendar
                {
                    public Guid CalendarId { get; set; }
                    public Guid OwnerId { get; set; }
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public string TimeZone { get; set; } = "UTC";
                    public bool IsDefault { get; set; } = false;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
                    public ICollection<RecurringEvent> RecurringEvents { get; set; } = new List<RecurringEvent>();
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IAppointmentRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Scheduling.Core.Entities;

                namespace Scheduling.Core.Interfaces;

                public interface IAppointmentRepository
                {
                    Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Appointment>> GetByCalendarIdAsync(Guid calendarId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Appointment>> GetByDateRangeAsync(Guid calendarId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
                    Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid appointmentId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ISchedulingService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Scheduling.Core.DTOs;

                namespace Scheduling.Core.Interfaces;

                public interface ISchedulingService
                {
                    Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);
                    Task<AppointmentDto?> GetAppointmentByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<AppointmentDto>> GetAppointmentsByCalendarAsync(Guid calendarId, CancellationToken cancellationToken = default);
                    Task CancelAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("AppointmentCreatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Scheduling.Core.Events;

                public sealed class AppointmentCreatedEvent
                {
                    public Guid AppointmentId { get; init; }
                    public Guid CalendarId { get; init; }
                    public required string Title { get; init; }
                    public DateTime StartTime { get; init; }
                    public DateTime EndTime { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("AppointmentCancelledEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Scheduling.Core.Events;

                public sealed class AppointmentCancelledEvent
                {
                    public Guid AppointmentId { get; init; }
                    public Guid CalendarId { get; init; }
                    public required string Reason { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("ReminderSentEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Scheduling.Core.Events;

                public sealed class ReminderSentEvent
                {
                    public Guid ReminderId { get; init; }
                    public Guid AppointmentId { get; init; }
                    public string ReminderType { get; init; } = "Email";
                    public DateTime SentAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("AppointmentDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Scheduling.Core.DTOs;

                public sealed class AppointmentDto
                {
                    public Guid AppointmentId { get; init; }
                    public Guid CalendarId { get; init; }
                    public required string Title { get; init; }
                    public string? Description { get; init; }
                    public DateTime StartTime { get; init; }
                    public DateTime EndTime { get; init; }
                    public string? Location { get; init; }
                    public string Status { get; init; } = "Scheduled";
                    public DateTime CreatedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateAppointmentRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Scheduling.Core.DTOs;

                public sealed class CreateAppointmentRequest
                {
                    [Required]
                    public Guid CalendarId { get; init; }

                    [Required]
                    public required string Title { get; init; }

                    public string? Description { get; init; }

                    [Required]
                    public DateTime StartTime { get; init; }

                    [Required]
                    public DateTime EndTime { get; init; }

                    public string? Location { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Scheduling.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("SchedulingDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Scheduling.Core.Entities;

                namespace Scheduling.Infrastructure.Data;

                public class SchedulingDbContext : DbContext
                {
                    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options) : base(options) { }

                    public DbSet<Appointment> Appointments => Set<Appointment>();
                    public DbSet<Reminder> Reminders => Set<Reminder>();
                    public DbSet<RecurringEvent> RecurringEvents => Set<RecurringEvent>();
                    public DbSet<Calendar> Calendars => Set<Calendar>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<Appointment>(entity =>
                        {
                            entity.HasKey(a => a.AppointmentId);
                            entity.Property(a => a.Title).IsRequired().HasMaxLength(200);
                            entity.HasOne(a => a.Calendar).WithMany(c => c.Appointments).HasForeignKey(a => a.CalendarId);
                            entity.HasMany(a => a.Reminders).WithOne(r => r.Appointment).HasForeignKey(r => r.AppointmentId);
                        });

                        modelBuilder.Entity<Reminder>(entity =>
                        {
                            entity.HasKey(r => r.ReminderId);
                        });

                        modelBuilder.Entity<RecurringEvent>(entity =>
                        {
                            entity.HasKey(e => e.RecurringEventId);
                            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                            entity.HasOne(e => e.Calendar).WithMany(c => c.RecurringEvents).HasForeignKey(e => e.CalendarId);
                        });

                        modelBuilder.Entity<Calendar>(entity =>
                        {
                            entity.HasKey(c => c.CalendarId);
                            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("AppointmentRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Scheduling.Core.Entities;
                using Scheduling.Core.Interfaces;
                using Scheduling.Infrastructure.Data;

                namespace Scheduling.Infrastructure.Repositories;

                public class AppointmentRepository : IAppointmentRepository
                {
                    private readonly SchedulingDbContext context;

                    public AppointmentRepository(SchedulingDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
                        => await context.Appointments.Include(a => a.Reminders).FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);

                    public async Task<IEnumerable<Appointment>> GetByCalendarIdAsync(Guid calendarId, CancellationToken cancellationToken = default)
                        => await context.Appointments.Include(a => a.Reminders).Where(a => a.CalendarId == calendarId).OrderBy(a => a.StartTime).ToListAsync(cancellationToken);

                    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(Guid calendarId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
                        => await context.Appointments.Include(a => a.Reminders).Where(a => a.CalendarId == calendarId && a.StartTime >= startDate && a.EndTime <= endDate).OrderBy(a => a.StartTime).ToListAsync(cancellationToken);

                    public async Task<Appointment> AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
                    {
                        appointment.AppointmentId = Guid.NewGuid();
                        await context.Appointments.AddAsync(appointment, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return appointment;
                    }

                    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
                    {
                        appointment.UpdatedAt = DateTime.UtcNow;
                        context.Appointments.Update(appointment);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid appointmentId, CancellationToken cancellationToken = default)
                    {
                        var appointment = await context.Appointments.FindAsync(new object[] { appointmentId }, cancellationToken);
                        if (appointment != null)
                        {
                            context.Appointments.Remove(appointment);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("SchedulingService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Scheduling.Core.DTOs;
                using Scheduling.Core.Entities;
                using Scheduling.Core.Interfaces;

                namespace Scheduling.Infrastructure.Services;

                public class SchedulingService : ISchedulingService
                {
                    private readonly IAppointmentRepository repository;

                    public SchedulingService(IAppointmentRepository repository)
                    {
                        this.repository = repository;
                    }

                    public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default)
                    {
                        var appointment = new Appointment
                        {
                            CalendarId = request.CalendarId,
                            Title = request.Title,
                            Description = request.Description,
                            StartTime = request.StartTime,
                            EndTime = request.EndTime,
                            Location = request.Location,
                            Status = AppointmentStatus.Scheduled
                        };

                        var created = await repository.AddAsync(appointment, cancellationToken);

                        return new AppointmentDto
                        {
                            AppointmentId = created.AppointmentId,
                            CalendarId = created.CalendarId,
                            Title = created.Title,
                            Description = created.Description,
                            StartTime = created.StartTime,
                            EndTime = created.EndTime,
                            Location = created.Location,
                            Status = created.Status.ToString(),
                            CreatedAt = created.CreatedAt
                        };
                    }

                    public async Task<AppointmentDto?> GetAppointmentByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
                    {
                        var appointment = await repository.GetByIdAsync(appointmentId, cancellationToken);
                        if (appointment == null) return null;

                        return new AppointmentDto
                        {
                            AppointmentId = appointment.AppointmentId,
                            CalendarId = appointment.CalendarId,
                            Title = appointment.Title,
                            Description = appointment.Description,
                            StartTime = appointment.StartTime,
                            EndTime = appointment.EndTime,
                            Location = appointment.Location,
                            Status = appointment.Status.ToString(),
                            CreatedAt = appointment.CreatedAt
                        };
                    }

                    public async Task<IEnumerable<AppointmentDto>> GetAppointmentsByCalendarAsync(Guid calendarId, CancellationToken cancellationToken = default)
                    {
                        var appointments = await repository.GetByCalendarIdAsync(calendarId, cancellationToken);
                        return appointments.Select(a => new AppointmentDto
                        {
                            AppointmentId = a.AppointmentId,
                            CalendarId = a.CalendarId,
                            Title = a.Title,
                            Description = a.Description,
                            StartTime = a.StartTime,
                            EndTime = a.EndTime,
                            Location = a.Location,
                            Status = a.Status.ToString(),
                            CreatedAt = a.CreatedAt
                        });
                    }

                    public async Task CancelAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
                    {
                        var appointment = await repository.GetByIdAsync(appointmentId, cancellationToken);
                        if (appointment != null)
                        {
                            appointment.Status = AppointmentStatus.Cancelled;
                            await repository.UpdateAsync(appointment, cancellationToken);
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
                using Scheduling.Core.Interfaces;
                using Scheduling.Infrastructure.Data;
                using Scheduling.Infrastructure.Repositories;
                using Scheduling.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddSchedulingInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<SchedulingDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("SchedulingDb") ??
                                @"Server=.\SQLEXPRESS;Database=SchedulingDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
                        services.AddScoped<ISchedulingService, SchedulingService>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Scheduling.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("AppointmentsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Scheduling.Core.DTOs;
                using Scheduling.Core.Interfaces;

                namespace Scheduling.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class AppointmentsController : ControllerBase
                {
                    private readonly ISchedulingService service;

                    public AppointmentsController(ISchedulingService service)
                    {
                        this.service = service;
                    }

                    [HttpPost]
                    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
                    {
                        var appointment = await service.CreateAppointmentAsync(request, cancellationToken);
                        return CreatedAtAction(nameof(GetById), new { id = appointment.AppointmentId }, appointment);
                    }

                    [HttpGet("{id:guid}")]
                    public async Task<ActionResult<AppointmentDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var appointment = await service.GetAppointmentByIdAsync(id, cancellationToken);
                        if (appointment == null) return NotFound();
                        return Ok(appointment);
                    }

                    [HttpDelete("{id:guid}")]
                    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
                    {
                        await service.CancelAppointmentAsync(id, cancellationToken);
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

                builder.Services.AddSchedulingInfrastructure(builder.Configuration);
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
                    "SchedulingDb": "Server=.\\SQLEXPRESS;Database=SchedulingDb;Trusted_Connection=True;TrustServerCertificate=True"
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
