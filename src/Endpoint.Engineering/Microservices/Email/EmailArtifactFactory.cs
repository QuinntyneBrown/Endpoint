// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Email;

public class EmailArtifactFactory : IEmailArtifactFactory
{
    private readonly ILogger<EmailArtifactFactory> logger;

    public EmailArtifactFactory(ILogger<EmailArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Email.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("Email", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Email.Core.Entities;

                public class Email
                {
                    public Guid EmailId { get; set; }
                    public Guid? TemplateId { get; set; }
                    public required string From { get; set; }
                    public required string To { get; set; }
                    public string? Cc { get; set; }
                    public string? Bcc { get; set; }
                    public required string Subject { get; set; }
                    public required string Body { get; set; }
                    public bool IsHtml { get; set; } = true;
                    public EmailStatus Status { get; set; } = EmailStatus.Pending;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? SentAt { get; set; }
                    public DateTime? DeliveredAt { get; set; }
                    public string? FailureReason { get; set; }
                    public EmailTemplate? Template { get; set; }
                    public ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
                }

                public enum EmailStatus { Pending, Sent, Delivered, Bounced, Failed }
                """
        });

        project.Files.Add(new FileModel("EmailTemplate", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Email.Core.Entities;

                public class EmailTemplate
                {
                    public Guid TemplateId { get; set; }
                    public required string Name { get; set; }
                    public required string Subject { get; set; }
                    public required string BodyTemplate { get; set; }
                    public bool IsHtml { get; set; } = true;
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("EmailAttachment", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Email.Core.Entities;

                public class EmailAttachment
                {
                    public Guid AttachmentId { get; set; }
                    public Guid EmailId { get; set; }
                    public required string FileName { get; set; }
                    public required string ContentType { get; set; }
                    public required byte[] Content { get; set; }
                    public long Size { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public Email Email { get; set; } = null!;
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IEmailRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Email.Core.Entities;

                namespace Email.Core.Interfaces;

                public interface IEmailRepository
                {
                    Task<Entities.Email?> GetByIdAsync(Guid emailId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Entities.Email>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<Entities.Email> AddAsync(Entities.Email email, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Entities.Email email, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid emailId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<EmailTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);
                    Task<EmailTemplate?> GetTemplateByIdAsync(Guid templateId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IEmailSender", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Email.Core.DTOs;

                namespace Email.Core.Interfaces;

                public interface IEmailSender
                {
                    Task<SendEmailResult> SendAsync(SendEmailRequest request, CancellationToken cancellationToken = default);
                    Task<SendEmailResult> SendWithTemplateAsync(SendTemplateEmailRequest request, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ITemplateEngine", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Email.Core.Entities;

                namespace Email.Core.Interfaces;

                public interface ITemplateEngine
                {
                    string RenderTemplate(EmailTemplate template, IDictionary<string, object> parameters);
                    string RenderSubject(EmailTemplate template, IDictionary<string, object> parameters);
                    bool ValidateTemplate(string templateContent);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("EmailSentEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Email.Core.Events;

                public sealed class EmailSentEvent
                {
                    public Guid EmailId { get; init; }
                    public required string To { get; init; }
                    public required string Subject { get; init; }
                    public DateTime SentAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("EmailDeliveredEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Email.Core.Events;

                public sealed class EmailDeliveredEvent
                {
                    public Guid EmailId { get; init; }
                    public required string To { get; init; }
                    public DateTime DeliveredAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("EmailBouncedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Email.Core.Events;

                public sealed class EmailBouncedEvent
                {
                    public Guid EmailId { get; init; }
                    public required string To { get; init; }
                    public required string BounceReason { get; init; }
                    public DateTime BouncedAt { get; init; } = DateTime.UtcNow;
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("EmailDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Email.Core.DTOs;

                public sealed class EmailDto
                {
                    public Guid EmailId { get; init; }
                    public required string From { get; init; }
                    public required string To { get; init; }
                    public string? Cc { get; init; }
                    public string? Bcc { get; init; }
                    public required string Subject { get; init; }
                    public required string Body { get; init; }
                    public bool IsHtml { get; init; }
                    public string Status { get; init; } = "Pending";
                    public DateTime CreatedAt { get; init; }
                    public DateTime? SentAt { get; init; }
                    public DateTime? DeliveredAt { get; init; }
                    public int AttachmentCount { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("EmailTemplateDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Email.Core.DTOs;

                public sealed class EmailTemplateDto
                {
                    public Guid TemplateId { get; init; }
                    public required string Name { get; init; }
                    public required string Subject { get; init; }
                    public required string BodyTemplate { get; init; }
                    public bool IsHtml { get; init; }
                    public bool IsActive { get; init; }
                    public DateTime CreatedAt { get; init; }
                    public DateTime? UpdatedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("SendEmailRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Email.Core.DTOs;

                public sealed class SendEmailRequest
                {
                    [Required]
                    [EmailAddress]
                    public required string From { get; init; }

                    [Required]
                    [EmailAddress]
                    public required string To { get; init; }

                    [EmailAddress]
                    public string? Cc { get; init; }

                    [EmailAddress]
                    public string? Bcc { get; init; }

                    [Required]
                    public required string Subject { get; init; }

                    [Required]
                    public required string Body { get; init; }

                    public bool IsHtml { get; init; } = true;

                    public List<AttachmentRequest>? Attachments { get; init; }
                }

                public sealed class AttachmentRequest
                {
                    [Required]
                    public required string FileName { get; init; }

                    [Required]
                    public required string ContentType { get; init; }

                    [Required]
                    public required string Base64Content { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("SendTemplateEmailRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Email.Core.DTOs;

                public sealed class SendTemplateEmailRequest
                {
                    [Required]
                    public Guid TemplateId { get; init; }

                    [Required]
                    [EmailAddress]
                    public required string From { get; init; }

                    [Required]
                    [EmailAddress]
                    public required string To { get; init; }

                    [EmailAddress]
                    public string? Cc { get; init; }

                    [EmailAddress]
                    public string? Bcc { get; init; }

                    public Dictionary<string, object> Parameters { get; init; } = new();

                    public List<AttachmentRequest>? Attachments { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("SendEmailResult", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Email.Core.DTOs;

                public sealed class SendEmailResult
                {
                    public Guid EmailId { get; init; }
                    public bool Success { get; init; }
                    public string? ErrorMessage { get; init; }
                    public DateTime? SentAt { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Email.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("EmailDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Email.Core.Entities;

                namespace Email.Infrastructure.Data;

                public class EmailDbContext : DbContext
                {
                    public EmailDbContext(DbContextOptions<EmailDbContext> options) : base(options) { }

                    public DbSet<Core.Entities.Email> Emails => Set<Core.Entities.Email>();
                    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
                    public DbSet<EmailAttachment> EmailAttachments => Set<EmailAttachment>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<Core.Entities.Email>(entity =>
                        {
                            entity.HasKey(e => e.EmailId);
                            entity.Property(e => e.From).IsRequired().HasMaxLength(256);
                            entity.Property(e => e.To).IsRequired().HasMaxLength(256);
                            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
                            entity.Property(e => e.Body).IsRequired();
                            entity.HasOne(e => e.Template).WithMany().HasForeignKey(e => e.TemplateId);
                            entity.HasMany(e => e.Attachments).WithOne(a => a.Email).HasForeignKey(a => a.EmailId);
                        });

                        modelBuilder.Entity<EmailTemplate>(entity =>
                        {
                            entity.HasKey(t => t.TemplateId);
                            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                            entity.HasIndex(t => t.Name).IsUnique();
                            entity.Property(t => t.Subject).IsRequired().HasMaxLength(500);
                            entity.Property(t => t.BodyTemplate).IsRequired();
                        });

                        modelBuilder.Entity<EmailAttachment>(entity =>
                        {
                            entity.HasKey(a => a.AttachmentId);
                            entity.Property(a => a.FileName).IsRequired().HasMaxLength(256);
                            entity.Property(a => a.ContentType).IsRequired().HasMaxLength(100);
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("EmailRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Email.Core.Entities;
                using Email.Core.Interfaces;
                using Email.Infrastructure.Data;

                namespace Email.Infrastructure.Repositories;

                public class EmailRepository : IEmailRepository
                {
                    private readonly EmailDbContext context;

                    public EmailRepository(EmailDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Core.Entities.Email?> GetByIdAsync(Guid emailId, CancellationToken cancellationToken = default)
                        => await context.Emails
                            .Include(e => e.Template)
                            .Include(e => e.Attachments)
                            .FirstOrDefaultAsync(e => e.EmailId == emailId, cancellationToken);

                    public async Task<IEnumerable<Core.Entities.Email>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.Emails
                            .Include(e => e.Template)
                            .Include(e => e.Attachments)
                            .OrderByDescending(e => e.CreatedAt)
                            .ToListAsync(cancellationToken);

                    public async Task<Core.Entities.Email> AddAsync(Core.Entities.Email email, CancellationToken cancellationToken = default)
                    {
                        email.EmailId = Guid.NewGuid();
                        await context.Emails.AddAsync(email, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return email;
                    }

                    public async Task UpdateAsync(Core.Entities.Email email, CancellationToken cancellationToken = default)
                    {
                        context.Emails.Update(email);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid emailId, CancellationToken cancellationToken = default)
                    {
                        var email = await context.Emails.FindAsync(new object[] { emailId }, cancellationToken);
                        if (email != null)
                        {
                            context.Emails.Remove(email);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task<IEnumerable<EmailTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default)
                        => await context.EmailTemplates
                            .Where(t => t.IsActive)
                            .OrderBy(t => t.Name)
                            .ToListAsync(cancellationToken);

                    public async Task<EmailTemplate?> GetTemplateByIdAsync(Guid templateId, CancellationToken cancellationToken = default)
                        => await context.EmailTemplates.FindAsync(new object[] { templateId }, cancellationToken);
                }
                """
        });

        project.Files.Add(new FileModel("EmailSender", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Email.Core.DTOs;
                using Email.Core.Entities;
                using Email.Core.Interfaces;

                namespace Email.Infrastructure.Services;

                public class EmailSender : IEmailSender
                {
                    private readonly IEmailRepository repository;
                    private readonly ITemplateEngine templateEngine;

                    public EmailSender(IEmailRepository repository, ITemplateEngine templateEngine)
                    {
                        this.repository = repository;
                        this.templateEngine = templateEngine;
                    }

                    public async Task<SendEmailResult> SendAsync(SendEmailRequest request, CancellationToken cancellationToken = default)
                    {
                        try
                        {
                            var email = new Core.Entities.Email
                            {
                                From = request.From,
                                To = request.To,
                                Cc = request.Cc,
                                Bcc = request.Bcc,
                                Subject = request.Subject,
                                Body = request.Body,
                                IsHtml = request.IsHtml,
                                Status = EmailStatus.Sent,
                                SentAt = DateTime.UtcNow
                            };

                            if (request.Attachments != null)
                            {
                                foreach (var attachment in request.Attachments)
                                {
                                    var content = Convert.FromBase64String(attachment.Base64Content);
                                    email.Attachments.Add(new EmailAttachment
                                    {
                                        AttachmentId = Guid.NewGuid(),
                                        FileName = attachment.FileName,
                                        ContentType = attachment.ContentType,
                                        Content = content,
                                        Size = content.Length
                                    });
                                }
                            }

                            var created = await repository.AddAsync(email, cancellationToken);

                            return new SendEmailResult
                            {
                                EmailId = created.EmailId,
                                Success = true,
                                SentAt = created.SentAt
                            };
                        }
                        catch (Exception ex)
                        {
                            return new SendEmailResult
                            {
                                EmailId = Guid.Empty,
                                Success = false,
                                ErrorMessage = ex.Message
                            };
                        }
                    }

                    public async Task<SendEmailResult> SendWithTemplateAsync(SendTemplateEmailRequest request, CancellationToken cancellationToken = default)
                    {
                        try
                        {
                            var template = await repository.GetTemplateByIdAsync(request.TemplateId, cancellationToken);
                            if (template == null)
                            {
                                return new SendEmailResult
                                {
                                    EmailId = Guid.Empty,
                                    Success = false,
                                    ErrorMessage = "Template not found"
                                };
                            }

                            var subject = templateEngine.RenderSubject(template, request.Parameters);
                            var body = templateEngine.RenderTemplate(template, request.Parameters);

                            var email = new Core.Entities.Email
                            {
                                From = request.From,
                                To = request.To,
                                Cc = request.Cc,
                                Bcc = request.Bcc,
                                Subject = subject,
                                Body = body,
                                IsHtml = template.IsHtml,
                                TemplateId = template.TemplateId,
                                Status = EmailStatus.Sent,
                                SentAt = DateTime.UtcNow
                            };

                            if (request.Attachments != null)
                            {
                                foreach (var attachment in request.Attachments)
                                {
                                    var content = Convert.FromBase64String(attachment.Base64Content);
                                    email.Attachments.Add(new EmailAttachment
                                    {
                                        AttachmentId = Guid.NewGuid(),
                                        FileName = attachment.FileName,
                                        ContentType = attachment.ContentType,
                                        Content = content,
                                        Size = content.Length
                                    });
                                }
                            }

                            var created = await repository.AddAsync(email, cancellationToken);

                            return new SendEmailResult
                            {
                                EmailId = created.EmailId,
                                Success = true,
                                SentAt = created.SentAt
                            };
                        }
                        catch (Exception ex)
                        {
                            return new SendEmailResult
                            {
                                EmailId = Guid.Empty,
                                Success = false,
                                ErrorMessage = ex.Message
                            };
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("TemplateEngine", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Text.RegularExpressions;
                using Email.Core.Entities;
                using Email.Core.Interfaces;

                namespace Email.Infrastructure.Services;

                public class TemplateEngine : ITemplateEngine
                {
                    private static readonly Regex PlaceholderRegex = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

                    public string RenderTemplate(EmailTemplate template, IDictionary<string, object> parameters)
                    {
                        return ReplacePlaceholders(template.BodyTemplate, parameters);
                    }

                    public string RenderSubject(EmailTemplate template, IDictionary<string, object> parameters)
                    {
                        return ReplacePlaceholders(template.Subject, parameters);
                    }

                    public bool ValidateTemplate(string templateContent)
                    {
                        try
                        {
                            var matches = PlaceholderRegex.Matches(templateContent);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    private static string ReplacePlaceholders(string template, IDictionary<string, object> parameters)
                    {
                        return PlaceholderRegex.Replace(template, match =>
                        {
                            var key = match.Groups[1].Value;
                            return parameters.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : match.Value;
                        });
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
                using Email.Core.Interfaces;
                using Email.Infrastructure.Data;
                using Email.Infrastructure.Repositories;
                using Email.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddEmailInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<EmailDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("EmailDb") ??
                                @"Server=.\SQLEXPRESS;Database=EmailDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IEmailRepository, EmailRepository>();
                        services.AddScoped<IEmailSender, EmailSender>();
                        services.AddScoped<ITemplateEngine, TemplateEngine>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Email.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("EmailController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Email.Core.DTOs;
                using Email.Core.Interfaces;

                namespace Email.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class EmailController : ControllerBase
                {
                    private readonly IEmailSender emailSender;
                    private readonly IEmailRepository repository;

                    public EmailController(IEmailSender emailSender, IEmailRepository repository)
                    {
                        this.emailSender = emailSender;
                        this.repository = repository;
                    }

                    [HttpPost("send")]
                    public async Task<ActionResult<SendEmailResult>> Send([FromBody] SendEmailRequest request, CancellationToken cancellationToken)
                    {
                        var result = await emailSender.SendAsync(request, cancellationToken);
                        if (!result.Success)
                        {
                            return BadRequest(result);
                        }
                        return CreatedAtAction(nameof(GetById), new { id = result.EmailId }, result);
                    }

                    [HttpGet("{id:guid}")]
                    public async Task<ActionResult<EmailDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var email = await repository.GetByIdAsync(id, cancellationToken);
                        if (email == null) return NotFound();

                        return Ok(new EmailDto
                        {
                            EmailId = email.EmailId,
                            From = email.From,
                            To = email.To,
                            Cc = email.Cc,
                            Bcc = email.Bcc,
                            Subject = email.Subject,
                            Body = email.Body,
                            IsHtml = email.IsHtml,
                            Status = email.Status.ToString(),
                            CreatedAt = email.CreatedAt,
                            SentAt = email.SentAt,
                            DeliveredAt = email.DeliveredAt,
                            AttachmentCount = email.Attachments.Count
                        });
                    }

                    [HttpGet("templates")]
                    public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetTemplates(CancellationToken cancellationToken)
                    {
                        var templates = await repository.GetTemplatesAsync(cancellationToken);
                        return Ok(templates.Select(t => new EmailTemplateDto
                        {
                            TemplateId = t.TemplateId,
                            Name = t.Name,
                            Subject = t.Subject,
                            BodyTemplate = t.BodyTemplate,
                            IsHtml = t.IsHtml,
                            IsActive = t.IsActive,
                            CreatedAt = t.CreatedAt,
                            UpdatedAt = t.UpdatedAt
                        }));
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

                builder.Services.AddEmailInfrastructure(builder.Configuration);
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
                    "EmailDb": "Server=.\\SQLEXPRESS;Database=EmailDb;Trusted_Connection=True;TrustServerCertificate=True"
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
