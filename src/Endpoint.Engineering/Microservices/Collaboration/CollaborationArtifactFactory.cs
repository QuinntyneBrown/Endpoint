// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Collaboration;

public class CollaborationArtifactFactory : ICollaborationArtifactFactory
{
    private readonly ILogger<CollaborationArtifactFactory> logger;

    public CollaborationArtifactFactory(ILogger<CollaborationArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Collaboration.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("Share", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Collaboration.Core.Entities;

                public class Share
                {
                    public Guid ShareId { get; set; }
                    public Guid EntityId { get; set; }
                    public required string EntityType { get; set; }
                    public Guid SharedByUserId { get; set; }
                    public Guid SharedWithUserId { get; set; }
                    public SharePermission Permission { get; set; } = SharePermission.View;
                    public DateTime SharedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? ExpiresAt { get; set; }
                    public bool IsActive { get; set; } = true;
                }

                public enum SharePermission { View, Edit, Admin }
                """
        });

        project.Files.Add(new FileModel("Comment", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Collaboration.Core.Entities;

                public class Comment
                {
                    public Guid CommentId { get; set; }
                    public Guid EntityId { get; set; }
                    public required string EntityType { get; set; }
                    public Guid UserId { get; set; }
                    public required string Content { get; set; }
                    public Guid? ParentCommentId { get; set; }
                    public Comment? ParentComment { get; set; }
                    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
                    public ICollection<Mention> Mentions { get; set; } = new List<Mention>();
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public bool IsDeleted { get; set; } = false;
                }
                """
        });

        project.Files.Add(new FileModel("Mention", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Collaboration.Core.Entities;

                public class Mention
                {
                    public Guid MentionId { get; set; }
                    public Guid CommentId { get; set; }
                    public Comment Comment { get; set; } = null!;
                    public Guid MentionedUserId { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public bool IsRead { get; set; } = false;
                }
                """
        });

        project.Files.Add(new FileModel("Activity", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Collaboration.Core.Entities;

                public class Activity
                {
                    public Guid ActivityId { get; set; }
                    public Guid EntityId { get; set; }
                    public required string EntityType { get; set; }
                    public Guid UserId { get; set; }
                    public required string ActionType { get; set; }
                    public string? Description { get; set; }
                    public string? Metadata { get; set; }
                    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IShareRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Collaboration.Core.Entities;

                namespace Collaboration.Core.Interfaces;

                public interface IShareRepository
                {
                    Task<Share?> GetByIdAsync(Guid shareId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Share>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Share>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
                    Task<Share> AddAsync(Share share, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Share share, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid shareId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ICommentRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Collaboration.Core.Entities;

                namespace Collaboration.Core.Interfaces;

                public interface ICommentRepository
                {
                    Task<Comment?> GetByIdAsync(Guid commentId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Comment>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default);
                    Task<Comment> AddAsync(Comment comment, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid commentId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ICollaborationService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Collaboration.Core.DTOs;

                namespace Collaboration.Core.Interfaces;

                public interface ICollaborationService
                {
                    Task<ShareDto> ShareEntityAsync(CreateShareRequest request, CancellationToken cancellationToken = default);
                    Task<IEnumerable<ShareDto>> GetSharesByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default);
                    Task RevokeShareAsync(Guid shareId, CancellationToken cancellationToken = default);
                    Task<CommentDto> AddCommentAsync(CreateCommentRequest request, CancellationToken cancellationToken = default);
                    Task<IEnumerable<CommentDto>> GetCommentsByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default);
                    Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("EntitySharedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Collaboration.Core.Events;

                public sealed class EntitySharedEvent
                {
                    public Guid ShareId { get; init; }
                    public Guid EntityId { get; init; }
                    public required string EntityType { get; init; }
                    public Guid SharedByUserId { get; init; }
                    public Guid SharedWithUserId { get; init; }
                    public required string Permission { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("CommentAddedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Collaboration.Core.Events;

                public sealed class CommentAddedEvent
                {
                    public Guid CommentId { get; init; }
                    public Guid EntityId { get; init; }
                    public required string EntityType { get; init; }
                    public Guid UserId { get; init; }
                    public required string Content { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("MentionCreatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Collaboration.Core.Events;

                public sealed class MentionCreatedEvent
                {
                    public Guid MentionId { get; init; }
                    public Guid CommentId { get; init; }
                    public Guid MentionedUserId { get; init; }
                    public Guid MentionedByUserId { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("ShareDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Collaboration.Core.DTOs;

                public sealed class ShareDto
                {
                    public Guid ShareId { get; init; }
                    public Guid EntityId { get; init; }
                    public required string EntityType { get; init; }
                    public Guid SharedByUserId { get; init; }
                    public Guid SharedWithUserId { get; init; }
                    public string Permission { get; init; } = "View";
                    public DateTime SharedAt { get; init; }
                    public DateTime? ExpiresAt { get; init; }
                    public bool IsActive { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateShareRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Collaboration.Core.DTOs;

                public sealed class CreateShareRequest
                {
                    [Required]
                    public Guid EntityId { get; init; }

                    [Required]
                    public required string EntityType { get; init; }

                    [Required]
                    public Guid SharedByUserId { get; init; }

                    [Required]
                    public Guid SharedWithUserId { get; init; }

                    public string Permission { get; init; } = "View";

                    public DateTime? ExpiresAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CommentDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Collaboration.Core.DTOs;

                public sealed class CommentDto
                {
                    public Guid CommentId { get; init; }
                    public Guid EntityId { get; init; }
                    public required string EntityType { get; init; }
                    public Guid UserId { get; init; }
                    public required string Content { get; init; }
                    public Guid? ParentCommentId { get; init; }
                    public DateTime CreatedAt { get; init; }
                    public DateTime? UpdatedAt { get; init; }
                    public IEnumerable<CommentDto> Replies { get; init; } = Enumerable.Empty<CommentDto>();
                }
                """
        });

        project.Files.Add(new FileModel("CreateCommentRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Collaboration.Core.DTOs;

                public sealed class CreateCommentRequest
                {
                    [Required]
                    public Guid EntityId { get; init; }

                    [Required]
                    public required string EntityType { get; init; }

                    [Required]
                    public Guid UserId { get; init; }

                    [Required]
                    public required string Content { get; init; }

                    public Guid? ParentCommentId { get; init; }

                    public IEnumerable<Guid> MentionedUserIds { get; init; } = Enumerable.Empty<Guid>();
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Collaboration.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("CollaborationDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Collaboration.Core.Entities;

                namespace Collaboration.Infrastructure.Data;

                public class CollaborationDbContext : DbContext
                {
                    public CollaborationDbContext(DbContextOptions<CollaborationDbContext> options) : base(options) { }

                    public DbSet<Share> Shares => Set<Share>();
                    public DbSet<Comment> Comments => Set<Comment>();
                    public DbSet<Mention> Mentions => Set<Mention>();
                    public DbSet<Activity> Activities => Set<Activity>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<Share>(entity =>
                        {
                            entity.HasKey(s => s.ShareId);
                            entity.Property(s => s.EntityType).IsRequired().HasMaxLength(100);
                            entity.HasIndex(s => new { s.EntityId, s.SharedWithUserId }).IsUnique();
                        });

                        modelBuilder.Entity<Comment>(entity =>
                        {
                            entity.HasKey(c => c.CommentId);
                            entity.Property(c => c.EntityType).IsRequired().HasMaxLength(100);
                            entity.Property(c => c.Content).IsRequired();
                            entity.HasOne(c => c.ParentComment).WithMany(c => c.Replies).HasForeignKey(c => c.ParentCommentId).OnDelete(DeleteBehavior.Restrict);
                        });

                        modelBuilder.Entity<Mention>(entity =>
                        {
                            entity.HasKey(m => m.MentionId);
                            entity.HasOne(m => m.Comment).WithMany(c => c.Mentions).HasForeignKey(m => m.CommentId);
                            entity.HasIndex(m => new { m.CommentId, m.MentionedUserId }).IsUnique();
                        });

                        modelBuilder.Entity<Activity>(entity =>
                        {
                            entity.HasKey(a => a.ActivityId);
                            entity.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
                            entity.Property(a => a.ActionType).IsRequired().HasMaxLength(50);
                            entity.HasIndex(a => new { a.EntityId, a.OccurredAt });
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("ShareRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Collaboration.Core.Entities;
                using Collaboration.Core.Interfaces;
                using Collaboration.Infrastructure.Data;

                namespace Collaboration.Infrastructure.Repositories;

                public class ShareRepository : IShareRepository
                {
                    private readonly CollaborationDbContext context;

                    public ShareRepository(CollaborationDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Share?> GetByIdAsync(Guid shareId, CancellationToken cancellationToken = default)
                        => await context.Shares.FirstOrDefaultAsync(s => s.ShareId == shareId, cancellationToken);

                    public async Task<IEnumerable<Share>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default)
                        => await context.Shares.Where(s => s.EntityId == entityId && s.IsActive).ToListAsync(cancellationToken);

                    public async Task<IEnumerable<Share>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
                        => await context.Shares.Where(s => s.SharedWithUserId == userId && s.IsActive).ToListAsync(cancellationToken);

                    public async Task<Share> AddAsync(Share share, CancellationToken cancellationToken = default)
                    {
                        share.ShareId = Guid.NewGuid();
                        await context.Shares.AddAsync(share, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return share;
                    }

                    public async Task UpdateAsync(Share share, CancellationToken cancellationToken = default)
                    {
                        context.Shares.Update(share);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid shareId, CancellationToken cancellationToken = default)
                    {
                        var share = await context.Shares.FindAsync(new object[] { shareId }, cancellationToken);
                        if (share != null)
                        {
                            share.IsActive = false;
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("CommentRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Collaboration.Core.Entities;
                using Collaboration.Core.Interfaces;
                using Collaboration.Infrastructure.Data;

                namespace Collaboration.Infrastructure.Repositories;

                public class CommentRepository : ICommentRepository
                {
                    private readonly CollaborationDbContext context;

                    public CommentRepository(CollaborationDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Comment?> GetByIdAsync(Guid commentId, CancellationToken cancellationToken = default)
                        => await context.Comments.Include(c => c.Mentions).Include(c => c.Replies).FirstOrDefaultAsync(c => c.CommentId == commentId, cancellationToken);

                    public async Task<IEnumerable<Comment>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default)
                        => await context.Comments.Include(c => c.Mentions).Include(c => c.Replies).Where(c => c.EntityId == entityId && !c.IsDeleted && c.ParentCommentId == null).OrderByDescending(c => c.CreatedAt).ToListAsync(cancellationToken);

                    public async Task<Comment> AddAsync(Comment comment, CancellationToken cancellationToken = default)
                    {
                        comment.CommentId = Guid.NewGuid();
                        await context.Comments.AddAsync(comment, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return comment;
                    }

                    public async Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default)
                    {
                        comment.UpdatedAt = DateTime.UtcNow;
                        context.Comments.Update(comment);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid commentId, CancellationToken cancellationToken = default)
                    {
                        var comment = await context.Comments.FindAsync(new object[] { commentId }, cancellationToken);
                        if (comment != null)
                        {
                            comment.IsDeleted = true;
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("CollaborationService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Collaboration.Core.DTOs;
                using Collaboration.Core.Entities;
                using Collaboration.Core.Interfaces;
                using Collaboration.Infrastructure.Data;

                namespace Collaboration.Infrastructure.Services;

                public class CollaborationService : ICollaborationService
                {
                    private readonly IShareRepository shareRepository;
                    private readonly ICommentRepository commentRepository;
                    private readonly CollaborationDbContext context;

                    public CollaborationService(IShareRepository shareRepository, ICommentRepository commentRepository, CollaborationDbContext context)
                    {
                        this.shareRepository = shareRepository;
                        this.commentRepository = commentRepository;
                        this.context = context;
                    }

                    public async Task<ShareDto> ShareEntityAsync(CreateShareRequest request, CancellationToken cancellationToken = default)
                    {
                        var share = new Share
                        {
                            EntityId = request.EntityId,
                            EntityType = request.EntityType,
                            SharedByUserId = request.SharedByUserId,
                            SharedWithUserId = request.SharedWithUserId,
                            Permission = Enum.Parse<SharePermission>(request.Permission, true),
                            ExpiresAt = request.ExpiresAt
                        };

                        var created = await shareRepository.AddAsync(share, cancellationToken);

                        return new ShareDto
                        {
                            ShareId = created.ShareId,
                            EntityId = created.EntityId,
                            EntityType = created.EntityType,
                            SharedByUserId = created.SharedByUserId,
                            SharedWithUserId = created.SharedWithUserId,
                            Permission = created.Permission.ToString(),
                            SharedAt = created.SharedAt,
                            ExpiresAt = created.ExpiresAt,
                            IsActive = created.IsActive
                        };
                    }

                    public async Task<IEnumerable<ShareDto>> GetSharesByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default)
                    {
                        var shares = await shareRepository.GetByEntityIdAsync(entityId, cancellationToken);
                        return shares.Select(s => new ShareDto
                        {
                            ShareId = s.ShareId,
                            EntityId = s.EntityId,
                            EntityType = s.EntityType,
                            SharedByUserId = s.SharedByUserId,
                            SharedWithUserId = s.SharedWithUserId,
                            Permission = s.Permission.ToString(),
                            SharedAt = s.SharedAt,
                            ExpiresAt = s.ExpiresAt,
                            IsActive = s.IsActive
                        });
                    }

                    public async Task RevokeShareAsync(Guid shareId, CancellationToken cancellationToken = default)
                    {
                        await shareRepository.DeleteAsync(shareId, cancellationToken);
                    }

                    public async Task<CommentDto> AddCommentAsync(CreateCommentRequest request, CancellationToken cancellationToken = default)
                    {
                        var comment = new Comment
                        {
                            EntityId = request.EntityId,
                            EntityType = request.EntityType,
                            UserId = request.UserId,
                            Content = request.Content,
                            ParentCommentId = request.ParentCommentId
                        };

                        var created = await commentRepository.AddAsync(comment, cancellationToken);

                        // Add mentions
                        foreach (var mentionedUserId in request.MentionedUserIds)
                        {
                            var mention = new Mention
                            {
                                MentionId = Guid.NewGuid(),
                                CommentId = created.CommentId,
                                MentionedUserId = mentionedUserId
                            };
                            await context.Mentions.AddAsync(mention, cancellationToken);
                        }
                        await context.SaveChangesAsync(cancellationToken);

                        return new CommentDto
                        {
                            CommentId = created.CommentId,
                            EntityId = created.EntityId,
                            EntityType = created.EntityType,
                            UserId = created.UserId,
                            Content = created.Content,
                            ParentCommentId = created.ParentCommentId,
                            CreatedAt = created.CreatedAt
                        };
                    }

                    public async Task<IEnumerable<CommentDto>> GetCommentsByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default)
                    {
                        var comments = await commentRepository.GetByEntityIdAsync(entityId, cancellationToken);
                        return comments.Select(MapCommentToDto);
                    }

                    public async Task DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken = default)
                    {
                        await commentRepository.DeleteAsync(commentId, cancellationToken);
                    }

                    private static CommentDto MapCommentToDto(Comment comment)
                    {
                        return new CommentDto
                        {
                            CommentId = comment.CommentId,
                            EntityId = comment.EntityId,
                            EntityType = comment.EntityType,
                            UserId = comment.UserId,
                            Content = comment.Content,
                            ParentCommentId = comment.ParentCommentId,
                            CreatedAt = comment.CreatedAt,
                            UpdatedAt = comment.UpdatedAt,
                            Replies = comment.Replies.Where(r => !r.IsDeleted).Select(MapCommentToDto)
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
                using Collaboration.Core.Interfaces;
                using Collaboration.Infrastructure.Data;
                using Collaboration.Infrastructure.Repositories;
                using Collaboration.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddCollaborationInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<CollaborationDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("CollaborationDb") ??
                                @"Server=.\SQLEXPRESS;Database=CollaborationDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<IShareRepository, ShareRepository>();
                        services.AddScoped<ICommentRepository, CommentRepository>();
                        services.AddScoped<ICollaborationService, CollaborationService>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Collaboration.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("SharesController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Collaboration.Core.DTOs;
                using Collaboration.Core.Interfaces;

                namespace Collaboration.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class SharesController : ControllerBase
                {
                    private readonly ICollaborationService service;

                    public SharesController(ICollaborationService service)
                    {
                        this.service = service;
                    }

                    [HttpPost]
                    public async Task<ActionResult<ShareDto>> Create([FromBody] CreateShareRequest request, CancellationToken cancellationToken)
                    {
                        var share = await service.ShareEntityAsync(request, cancellationToken);
                        return CreatedAtAction(nameof(GetByEntityId), new { entityId = share.EntityId }, share);
                    }

                    [HttpGet("{entityId:guid}")]
                    public async Task<ActionResult<IEnumerable<ShareDto>>> GetByEntityId(Guid entityId, CancellationToken cancellationToken)
                    {
                        var shares = await service.GetSharesByEntityIdAsync(entityId, cancellationToken);
                        return Ok(shares);
                    }

                    [HttpDelete("{shareId:guid}")]
                    public async Task<IActionResult> Revoke(Guid shareId, CancellationToken cancellationToken)
                    {
                        await service.RevokeShareAsync(shareId, cancellationToken);
                        return NoContent();
                    }
                }
                """
        });

        project.Files.Add(new FileModel("CommentsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Collaboration.Core.DTOs;
                using Collaboration.Core.Interfaces;

                namespace Collaboration.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class CommentsController : ControllerBase
                {
                    private readonly ICollaborationService service;

                    public CommentsController(ICollaborationService service)
                    {
                        this.service = service;
                    }

                    [HttpPost]
                    public async Task<ActionResult<CommentDto>> Create([FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
                    {
                        var comment = await service.AddCommentAsync(request, cancellationToken);
                        return CreatedAtAction(nameof(GetByEntityId), new { entityId = comment.EntityId }, comment);
                    }

                    [HttpGet("{entityId:guid}")]
                    public async Task<ActionResult<IEnumerable<CommentDto>>> GetByEntityId(Guid entityId, CancellationToken cancellationToken)
                    {
                        var comments = await service.GetCommentsByEntityIdAsync(entityId, cancellationToken);
                        return Ok(comments);
                    }

                    [HttpDelete("{commentId:guid}")]
                    public async Task<IActionResult> Delete(Guid commentId, CancellationToken cancellationToken)
                    {
                        await service.DeleteCommentAsync(commentId, cancellationToken);
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

                builder.Services.AddCollaborationInfrastructure(builder.Configuration);
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
                    "CollaborationDb": "Server=.\\SQLEXPRESS;Database=CollaborationDb;Trusted_Connection=True;TrustServerCertificate=True"
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
