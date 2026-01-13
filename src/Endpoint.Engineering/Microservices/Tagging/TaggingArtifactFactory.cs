// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Tagging;

public class TaggingArtifactFactory : ITaggingArtifactFactory
{
    private readonly ILogger<TaggingArtifactFactory> logger;

    public TaggingArtifactFactory(ILogger<TaggingArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Tagging.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("Tag", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tagging.Core.Entities;

                public class Tag
                {
                    public Guid TagId { get; set; }
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public Guid? CategoryId { get; set; }
                    public Category? Category { get; set; }
                    public string? Color { get; set; }
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public ICollection<EntityTag> EntityTags { get; set; } = new List<EntityTag>();
                }
                """
        });

        project.Files.Add(new FileModel("Category", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tagging.Core.Entities;

                public class Category
                {
                    public Guid CategoryId { get; set; }
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public Guid? ParentCategoryId { get; set; }
                    public Category? ParentCategory { get; set; }
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
                    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
                }
                """
        });

        project.Files.Add(new FileModel("EntityTag", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tagging.Core.Entities;

                public class EntityTag
                {
                    public Guid EntityTagId { get; set; }
                    public Guid TagId { get; set; }
                    public Tag Tag { get; set; } = null!;
                    public required string EntityType { get; set; }
                    public Guid EntityId { get; set; }
                    public Guid? TaggedBy { get; set; }
                    public DateTime TaggedAt { get; set; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("Taxonomy", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tagging.Core.Entities;

                public class Taxonomy
                {
                    public Guid TaxonomyId { get; set; }
                    public required string Name { get; set; }
                    public string? Description { get; set; }
                    public bool IsHierarchical { get; set; } = false;
                    public bool AllowMultiple { get; set; } = true;
                    public bool IsActive { get; set; } = true;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("ITagRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Tagging.Core.Entities;

                namespace Tagging.Core.Interfaces;

                public interface ITagRepository
                {
                    Task<Tag?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken = default);
                    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<IEnumerable<Tag>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
                    Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid tagId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ITaggingService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Tagging.Core.DTOs;

                namespace Tagging.Core.Interfaces;

                public interface ITaggingService
                {
                    Task<TagDto> CreateTagAsync(CreateTagRequest request, CancellationToken cancellationToken = default);
                    Task<TagDto?> GetTagByIdAsync(Guid tagId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<TagDto>> GetAllTagsAsync(CancellationToken cancellationToken = default);
                    Task<EntityTagDto> TagEntityAsync(TagEntityRequest request, CancellationToken cancellationToken = default);
                    Task<IEnumerable<TagDto>> GetTagsForEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
                    Task UntagEntityAsync(string entityType, Guid entityId, Guid tagId, CancellationToken cancellationToken = default);
                    Task MergeTagsAsync(Guid sourceTagId, Guid targetTagId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ITaxonomyService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Tagging.Core.DTOs;

                namespace Tagging.Core.Interfaces;

                public interface ITaxonomyService
                {
                    Task<TaxonomyDto> CreateTaxonomyAsync(CreateTaxonomyRequest request, CancellationToken cancellationToken = default);
                    Task<TaxonomyDto?> GetTaxonomyByIdAsync(Guid taxonomyId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<TaxonomyDto>> GetAllTaxonomiesAsync(CancellationToken cancellationToken = default);
                    Task UpdateTaxonomyAsync(Guid taxonomyId, UpdateTaxonomyRequest request, CancellationToken cancellationToken = default);
                    Task DeleteTaxonomyAsync(Guid taxonomyId, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("TagCreatedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tagging.Core.Events;

                public sealed class TagCreatedEvent
                {
                    public Guid TagId { get; init; }
                    public required string Name { get; init; }
                    public Guid? CategoryId { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("EntityTaggedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tagging.Core.Events;

                public sealed class EntityTaggedEvent
                {
                    public Guid EntityTagId { get; init; }
                    public Guid TagId { get; init; }
                    public required string EntityType { get; init; }
                    public Guid EntityId { get; init; }
                    public Guid? TaggedBy { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("TagMergedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tagging.Core.Events;

                public sealed class TagMergedEvent
                {
                    public Guid SourceTagId { get; init; }
                    public Guid TargetTagId { get; init; }
                    public int EntitiesRetagged { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("TagDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tagging.Core.DTOs;

                public sealed class TagDto
                {
                    public Guid TagId { get; init; }
                    public required string Name { get; init; }
                    public string? Description { get; init; }
                    public Guid? CategoryId { get; init; }
                    public string? CategoryName { get; init; }
                    public string? Color { get; init; }
                    public bool IsActive { get; init; }
                    public DateTime CreatedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("EntityTagDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tagging.Core.DTOs;

                public sealed class EntityTagDto
                {
                    public Guid EntityTagId { get; init; }
                    public Guid TagId { get; init; }
                    public required string TagName { get; init; }
                    public required string EntityType { get; init; }
                    public Guid EntityId { get; init; }
                    public DateTime TaggedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("TaxonomyDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Tagging.Core.DTOs;

                public sealed class TaxonomyDto
                {
                    public Guid TaxonomyId { get; init; }
                    public required string Name { get; init; }
                    public string? Description { get; init; }
                    public bool IsHierarchical { get; init; }
                    public bool AllowMultiple { get; init; }
                    public bool IsActive { get; init; }
                    public DateTime CreatedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateTagRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Tagging.Core.DTOs;

                public sealed class CreateTagRequest
                {
                    [Required]
                    [MaxLength(100)]
                    public required string Name { get; init; }

                    [MaxLength(500)]
                    public string? Description { get; init; }

                    public Guid? CategoryId { get; init; }

                    [MaxLength(7)]
                    public string? Color { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("TagEntityRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Tagging.Core.DTOs;

                public sealed class TagEntityRequest
                {
                    [Required]
                    public Guid TagId { get; init; }

                    [Required]
                    public required string EntityType { get; init; }

                    [Required]
                    public Guid EntityId { get; init; }

                    public Guid? TaggedBy { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateTaxonomyRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Tagging.Core.DTOs;

                public sealed class CreateTaxonomyRequest
                {
                    [Required]
                    [MaxLength(100)]
                    public required string Name { get; init; }

                    [MaxLength(500)]
                    public string? Description { get; init; }

                    public bool IsHierarchical { get; init; } = false;

                    public bool AllowMultiple { get; init; } = true;
                }
                """
        });

        project.Files.Add(new FileModel("UpdateTaxonomyRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Tagging.Core.DTOs;

                public sealed class UpdateTaxonomyRequest
                {
                    [MaxLength(100)]
                    public string? Name { get; init; }

                    [MaxLength(500)]
                    public string? Description { get; init; }

                    public bool? IsHierarchical { get; init; }

                    public bool? AllowMultiple { get; init; }

                    public bool? IsActive { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Tagging.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("TaggingDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Tagging.Core.Entities;

                namespace Tagging.Infrastructure.Data;

                public class TaggingDbContext : DbContext
                {
                    public TaggingDbContext(DbContextOptions<TaggingDbContext> options) : base(options) { }

                    public DbSet<Tag> Tags => Set<Tag>();
                    public DbSet<Category> Categories => Set<Category>();
                    public DbSet<EntityTag> EntityTags => Set<EntityTag>();
                    public DbSet<Taxonomy> Taxonomies => Set<Taxonomy>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<Tag>(entity =>
                        {
                            entity.HasKey(t => t.TagId);
                            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                            entity.HasIndex(t => t.Name).IsUnique();
                            entity.Property(t => t.Color).HasMaxLength(7);
                            entity.HasOne(t => t.Category).WithMany(c => c.Tags).HasForeignKey(t => t.CategoryId);
                        });

                        modelBuilder.Entity<Category>(entity =>
                        {
                            entity.HasKey(c => c.CategoryId);
                            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                            entity.HasIndex(c => c.Name).IsUnique();
                            entity.HasOne(c => c.ParentCategory).WithMany(c => c.SubCategories).HasForeignKey(c => c.ParentCategoryId);
                        });

                        modelBuilder.Entity<EntityTag>(entity =>
                        {
                            entity.HasKey(et => et.EntityTagId);
                            entity.Property(et => et.EntityType).IsRequired().HasMaxLength(100);
                            entity.HasIndex(et => new { et.TagId, et.EntityType, et.EntityId }).IsUnique();
                            entity.HasOne(et => et.Tag).WithMany(t => t.EntityTags).HasForeignKey(et => et.TagId);
                        });

                        modelBuilder.Entity<Taxonomy>(entity =>
                        {
                            entity.HasKey(t => t.TaxonomyId);
                            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
                            entity.HasIndex(t => t.Name).IsUnique();
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("TagRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Tagging.Core.Entities;
                using Tagging.Core.Interfaces;
                using Tagging.Infrastructure.Data;

                namespace Tagging.Infrastructure.Repositories;

                public class TagRepository : ITagRepository
                {
                    private readonly TaggingDbContext context;

                    public TagRepository(TaggingDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Tag?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken = default)
                        => await context.Tags.Include(t => t.Category).FirstOrDefaultAsync(t => t.TagId == tagId, cancellationToken);

                    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
                        => await context.Tags.Include(t => t.Category).FirstOrDefaultAsync(t => t.Name == name, cancellationToken);

                    public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.Tags.Include(t => t.Category).Where(t => t.IsActive).ToListAsync(cancellationToken);

                    public async Task<IEnumerable<Tag>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
                        => await context.Tags.Include(t => t.Category).Where(t => t.CategoryId == categoryId && t.IsActive).ToListAsync(cancellationToken);

                    public async Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default)
                    {
                        tag.TagId = Guid.NewGuid();
                        await context.Tags.AddAsync(tag, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return tag;
                    }

                    public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
                    {
                        tag.UpdatedAt = DateTime.UtcNow;
                        context.Tags.Update(tag);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid tagId, CancellationToken cancellationToken = default)
                    {
                        var tag = await context.Tags.FindAsync(new object[] { tagId }, cancellationToken);
                        if (tag != null)
                        {
                            context.Tags.Remove(tag);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("TaggingService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Tagging.Core.DTOs;
                using Tagging.Core.Entities;
                using Tagging.Core.Interfaces;
                using Tagging.Infrastructure.Data;

                namespace Tagging.Infrastructure.Services;

                public class TaggingService : ITaggingService
                {
                    private readonly ITagRepository repository;
                    private readonly TaggingDbContext context;

                    public TaggingService(ITagRepository repository, TaggingDbContext context)
                    {
                        this.repository = repository;
                        this.context = context;
                    }

                    public async Task<TagDto> CreateTagAsync(CreateTagRequest request, CancellationToken cancellationToken = default)
                    {
                        var tag = new Tag
                        {
                            Name = request.Name,
                            Description = request.Description,
                            CategoryId = request.CategoryId,
                            Color = request.Color
                        };

                        var created = await repository.AddAsync(tag, cancellationToken);
                        var category = created.CategoryId.HasValue
                            ? await context.Categories.FindAsync(new object[] { created.CategoryId.Value }, cancellationToken)
                            : null;

                        return new TagDto
                        {
                            TagId = created.TagId,
                            Name = created.Name,
                            Description = created.Description,
                            CategoryId = created.CategoryId,
                            CategoryName = category?.Name,
                            Color = created.Color,
                            IsActive = created.IsActive,
                            CreatedAt = created.CreatedAt
                        };
                    }

                    public async Task<TagDto?> GetTagByIdAsync(Guid tagId, CancellationToken cancellationToken = default)
                    {
                        var tag = await repository.GetByIdAsync(tagId, cancellationToken);
                        if (tag == null) return null;

                        return new TagDto
                        {
                            TagId = tag.TagId,
                            Name = tag.Name,
                            Description = tag.Description,
                            CategoryId = tag.CategoryId,
                            CategoryName = tag.Category?.Name,
                            Color = tag.Color,
                            IsActive = tag.IsActive,
                            CreatedAt = tag.CreatedAt
                        };
                    }

                    public async Task<IEnumerable<TagDto>> GetAllTagsAsync(CancellationToken cancellationToken = default)
                    {
                        var tags = await repository.GetAllAsync(cancellationToken);
                        return tags.Select(t => new TagDto
                        {
                            TagId = t.TagId,
                            Name = t.Name,
                            Description = t.Description,
                            CategoryId = t.CategoryId,
                            CategoryName = t.Category?.Name,
                            Color = t.Color,
                            IsActive = t.IsActive,
                            CreatedAt = t.CreatedAt
                        });
                    }

                    public async Task<EntityTagDto> TagEntityAsync(TagEntityRequest request, CancellationToken cancellationToken = default)
                    {
                        var tag = await repository.GetByIdAsync(request.TagId, cancellationToken)
                            ?? throw new InvalidOperationException($"Tag with ID {request.TagId} not found.");

                        var entityTag = new EntityTag
                        {
                            EntityTagId = Guid.NewGuid(),
                            TagId = request.TagId,
                            EntityType = request.EntityType,
                            EntityId = request.EntityId,
                            TaggedBy = request.TaggedBy
                        };

                        await context.EntityTags.AddAsync(entityTag, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);

                        return new EntityTagDto
                        {
                            EntityTagId = entityTag.EntityTagId,
                            TagId = entityTag.TagId,
                            TagName = tag.Name,
                            EntityType = entityTag.EntityType,
                            EntityId = entityTag.EntityId,
                            TaggedAt = entityTag.TaggedAt
                        };
                    }

                    public async Task<IEnumerable<TagDto>> GetTagsForEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
                    {
                        var entityTags = await context.EntityTags
                            .Include(et => et.Tag)
                            .ThenInclude(t => t.Category)
                            .Where(et => et.EntityType == entityType && et.EntityId == entityId)
                            .ToListAsync(cancellationToken);

                        return entityTags.Select(et => new TagDto
                        {
                            TagId = et.Tag.TagId,
                            Name = et.Tag.Name,
                            Description = et.Tag.Description,
                            CategoryId = et.Tag.CategoryId,
                            CategoryName = et.Tag.Category?.Name,
                            Color = et.Tag.Color,
                            IsActive = et.Tag.IsActive,
                            CreatedAt = et.Tag.CreatedAt
                        });
                    }

                    public async Task UntagEntityAsync(string entityType, Guid entityId, Guid tagId, CancellationToken cancellationToken = default)
                    {
                        var entityTag = await context.EntityTags
                            .FirstOrDefaultAsync(et => et.EntityType == entityType && et.EntityId == entityId && et.TagId == tagId, cancellationToken);

                        if (entityTag != null)
                        {
                            context.EntityTags.Remove(entityTag);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task MergeTagsAsync(Guid sourceTagId, Guid targetTagId, CancellationToken cancellationToken = default)
                    {
                        var sourceTag = await repository.GetByIdAsync(sourceTagId, cancellationToken)
                            ?? throw new InvalidOperationException($"Source tag with ID {sourceTagId} not found.");

                        var targetTag = await repository.GetByIdAsync(targetTagId, cancellationToken)
                            ?? throw new InvalidOperationException($"Target tag with ID {targetTagId} not found.");

                        var entityTags = await context.EntityTags
                            .Where(et => et.TagId == sourceTagId)
                            .ToListAsync(cancellationToken);

                        foreach (var entityTag in entityTags)
                        {
                            var existingTarget = await context.EntityTags
                                .AnyAsync(et => et.TagId == targetTagId && et.EntityType == entityTag.EntityType && et.EntityId == entityTag.EntityId, cancellationToken);

                            if (!existingTarget)
                            {
                                entityTag.TagId = targetTagId;
                            }
                            else
                            {
                                context.EntityTags.Remove(entityTag);
                            }
                        }

                        sourceTag.IsActive = false;
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }
                """
        });

        project.Files.Add(new FileModel("TaxonomyService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Tagging.Core.DTOs;
                using Tagging.Core.Entities;
                using Tagging.Core.Interfaces;
                using Tagging.Infrastructure.Data;

                namespace Tagging.Infrastructure.Services;

                public class TaxonomyService : ITaxonomyService
                {
                    private readonly TaggingDbContext context;

                    public TaxonomyService(TaggingDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<TaxonomyDto> CreateTaxonomyAsync(CreateTaxonomyRequest request, CancellationToken cancellationToken = default)
                    {
                        var taxonomy = new Taxonomy
                        {
                            TaxonomyId = Guid.NewGuid(),
                            Name = request.Name,
                            Description = request.Description,
                            IsHierarchical = request.IsHierarchical,
                            AllowMultiple = request.AllowMultiple
                        };

                        await context.Taxonomies.AddAsync(taxonomy, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);

                        return new TaxonomyDto
                        {
                            TaxonomyId = taxonomy.TaxonomyId,
                            Name = taxonomy.Name,
                            Description = taxonomy.Description,
                            IsHierarchical = taxonomy.IsHierarchical,
                            AllowMultiple = taxonomy.AllowMultiple,
                            IsActive = taxonomy.IsActive,
                            CreatedAt = taxonomy.CreatedAt
                        };
                    }

                    public async Task<TaxonomyDto?> GetTaxonomyByIdAsync(Guid taxonomyId, CancellationToken cancellationToken = default)
                    {
                        var taxonomy = await context.Taxonomies.FindAsync(new object[] { taxonomyId }, cancellationToken);
                        if (taxonomy == null) return null;

                        return new TaxonomyDto
                        {
                            TaxonomyId = taxonomy.TaxonomyId,
                            Name = taxonomy.Name,
                            Description = taxonomy.Description,
                            IsHierarchical = taxonomy.IsHierarchical,
                            AllowMultiple = taxonomy.AllowMultiple,
                            IsActive = taxonomy.IsActive,
                            CreatedAt = taxonomy.CreatedAt
                        };
                    }

                    public async Task<IEnumerable<TaxonomyDto>> GetAllTaxonomiesAsync(CancellationToken cancellationToken = default)
                    {
                        var taxonomies = await context.Taxonomies.Where(t => t.IsActive).ToListAsync(cancellationToken);
                        return taxonomies.Select(t => new TaxonomyDto
                        {
                            TaxonomyId = t.TaxonomyId,
                            Name = t.Name,
                            Description = t.Description,
                            IsHierarchical = t.IsHierarchical,
                            AllowMultiple = t.AllowMultiple,
                            IsActive = t.IsActive,
                            CreatedAt = t.CreatedAt
                        });
                    }

                    public async Task UpdateTaxonomyAsync(Guid taxonomyId, UpdateTaxonomyRequest request, CancellationToken cancellationToken = default)
                    {
                        var taxonomy = await context.Taxonomies.FindAsync(new object[] { taxonomyId }, cancellationToken)
                            ?? throw new InvalidOperationException($"Taxonomy with ID {taxonomyId} not found.");

                        if (request.Name != null) taxonomy.Name = request.Name;
                        if (request.Description != null) taxonomy.Description = request.Description;
                        if (request.IsHierarchical.HasValue) taxonomy.IsHierarchical = request.IsHierarchical.Value;
                        if (request.AllowMultiple.HasValue) taxonomy.AllowMultiple = request.AllowMultiple.Value;
                        if (request.IsActive.HasValue) taxonomy.IsActive = request.IsActive.Value;
                        taxonomy.UpdatedAt = DateTime.UtcNow;

                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteTaxonomyAsync(Guid taxonomyId, CancellationToken cancellationToken = default)
                    {
                        var taxonomy = await context.Taxonomies.FindAsync(new object[] { taxonomyId }, cancellationToken);
                        if (taxonomy != null)
                        {
                            context.Taxonomies.Remove(taxonomy);
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
                using Tagging.Core.Interfaces;
                using Tagging.Infrastructure.Data;
                using Tagging.Infrastructure.Repositories;
                using Tagging.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddTaggingInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<TaggingDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("TaggingDb") ??
                                @"Server=.\SQLEXPRESS;Database=TaggingDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<ITagRepository, TagRepository>();
                        services.AddScoped<ITaggingService, TaggingService>();
                        services.AddScoped<ITaxonomyService, TaxonomyService>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Tagging.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("TagsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Tagging.Core.DTOs;
                using Tagging.Core.Interfaces;

                namespace Tagging.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class TagsController : ControllerBase
                {
                    private readonly ITaggingService service;

                    public TagsController(ITaggingService service)
                    {
                        this.service = service;
                    }

                    [HttpPost]
                    public async Task<ActionResult<TagDto>> Create([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
                    {
                        var tag = await service.CreateTagAsync(request, cancellationToken);
                        return CreatedAtAction(nameof(GetById), new { id = tag.TagId }, tag);
                    }

                    [HttpGet]
                    public async Task<ActionResult<IEnumerable<TagDto>>> GetAll(CancellationToken cancellationToken)
                    {
                        var tags = await service.GetAllTagsAsync(cancellationToken);
                        return Ok(tags);
                    }

                    [HttpGet("{id:guid}")]
                    public async Task<ActionResult<TagDto>> GetById(Guid id, CancellationToken cancellationToken)
                    {
                        var tag = await service.GetTagByIdAsync(id, cancellationToken);
                        if (tag == null) return NotFound();
                        return Ok(tag);
                    }

                    [HttpPost("{entityType}/{entityId:guid}")]
                    public async Task<ActionResult<EntityTagDto>> TagEntity(string entityType, Guid entityId, [FromBody] TagEntityBodyRequest body, CancellationToken cancellationToken)
                    {
                        var request = new TagEntityRequest
                        {
                            TagId = body.TagId,
                            EntityType = entityType,
                            EntityId = entityId,
                            TaggedBy = body.TaggedBy
                        };
                        var entityTag = await service.TagEntityAsync(request, cancellationToken);
                        return Created($"/api/tags/{entityType}/{entityId}", entityTag);
                    }

                    [HttpGet("{entityType}/{entityId:guid}")]
                    public async Task<ActionResult<IEnumerable<TagDto>>> GetTagsForEntity(string entityType, Guid entityId, CancellationToken cancellationToken)
                    {
                        var tags = await service.GetTagsForEntityAsync(entityType, entityId, cancellationToken);
                        return Ok(tags);
                    }

                    [HttpDelete("{entityType}/{entityId:guid}/{tagId:guid}")]
                    public async Task<IActionResult> UntagEntity(string entityType, Guid entityId, Guid tagId, CancellationToken cancellationToken)
                    {
                        await service.UntagEntityAsync(entityType, entityId, tagId, cancellationToken);
                        return NoContent();
                    }

                    [HttpPost("merge")]
                    public async Task<IActionResult> MergeTags([FromBody] MergeTagsRequest request, CancellationToken cancellationToken)
                    {
                        await service.MergeTagsAsync(request.SourceTagId, request.TargetTagId, cancellationToken);
                        return NoContent();
                    }
                }

                public sealed class TagEntityBodyRequest
                {
                    public Guid TagId { get; init; }
                    public Guid? TaggedBy { get; init; }
                }

                public sealed class MergeTagsRequest
                {
                    public Guid SourceTagId { get; init; }
                    public Guid TargetTagId { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("Program", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddTaggingInfrastructure(builder.Configuration);
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
                    "TaggingDb": "Server=.\\SQLEXPRESS;Database=TaggingDb;Trusted_Connection=True;TrustServerCertificate=True"
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
