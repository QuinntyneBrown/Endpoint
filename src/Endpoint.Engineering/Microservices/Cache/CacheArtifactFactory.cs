// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Expressions;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Attributes;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Cache;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class CacheArtifactFactory : ICacheArtifactFactory
{
    private readonly ILogger<CacheArtifactFactory> logger;

    public CacheArtifactFactory(ILogger<CacheArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Cache.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(CreateCacheEntryFile(entitiesDir));
        project.Files.Add(CreateCacheStatisticsFile(entitiesDir));

        // Interfaces
        project.Files.Add(CreateICacheServiceFile(interfacesDir));
        project.Files.Add(CreateICacheInvalidationFile(interfacesDir));

        // Events
        project.Files.Add(CreateCacheUpdatedEventFile(eventsDir));
        project.Files.Add(CreateCacheInvalidatedEventFile(eventsDir));

        // DTOs
        project.Files.Add(CreateCacheEntryDtoFile(dtosDir));
        project.Files.Add(CreateSetCacheRequestDtoFile(dtosDir));
        project.Files.Add(CreateCacheStatisticsDtoFile(dtosDir));
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Cache.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(CreateCacheDbContextFile(dataDir));
        project.Files.Add(CreateCacheRepositoryFile(repositoriesDir));
        project.Files.Add(CreateCacheServiceFile(servicesDir));
        project.Files.Add(CreateCacheInvalidationServiceFile(servicesDir));
        project.Files.Add(CreateInfrastructureConfigureServicesFile(project.Directory));
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Cache.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(CreateCacheControllerFile(controllersDir));
        project.Files.Add(CreateProgramFile(project.Directory));
        project.Files.Add(CreateAppSettingsFile(project.Directory));
    }

    #region Core Layer Files

    private static CodeFileModel<ClassModel> CreateCacheEntryFile(string directory)
    {
        var classModel = new ClassModel("CacheEntry");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "EntryId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Key", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "ExpiresAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "LastAccessedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "SizeBytes", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Dictionary") { GenericTypeParameters = [new TypeModel("string"), new TypeModel("string")] }, "Tags", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "new()" });

        return new CodeFileModel<ClassModel>(classModel, "CacheEntry", directory, CSharp)
        {
            Namespace = "Cache.Core.Entities"
        };
    }

    private static CodeFileModel<ClassModel> CreateCacheStatisticsFile(string directory)
    {
        var classModel = new ClassModel("CacheStatistics");

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("Guid"), "StatisticsId", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "TotalEntries", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "TotalSizeBytes", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "HitCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "MissCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "HitRatio", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "EvictionCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "LastResetAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CollectedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Set)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "CacheStatistics", directory, CSharp)
        {
            Namespace = "Cache.Core.Entities"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateICacheServiceFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ICacheService");

        interfaceModel.Usings.Add(new UsingModel("Cache.Core.Entities"));

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("CacheEntry") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("T") { Nullable = true }] },
            GenericTypeParameters = ["T"],
            GenericTypeParameterConstraints = ["T : class"],
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "SetAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "value", Type = new TypeModel("string") },
                new ParamModel { Name = "expiration", Type = new TypeModel("TimeSpan") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "SetAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            GenericTypeParameters = ["T"],
            GenericTypeParameterConstraints = ["T : class"],
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "value", Type = new TypeModel("T") },
                new ParamModel { Name = "expiration", Type = new TypeModel("TimeSpan") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "ExistsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "GetStatisticsAsync",
            Interface = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("CacheStatistics")] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ICacheService", directory, CSharp)
        {
            Namespace = "Cache.Core.Interfaces"
        };
    }

    private static CodeFileModel<InterfaceModel> CreateICacheInvalidationFile(string directory)
    {
        var interfaceModel = new InterfaceModel("ICacheInvalidation");

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "InvalidateAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "InvalidateByPatternAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "pattern", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "InvalidateByTagAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "tag", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        interfaceModel.Methods.Add(new MethodModel
        {
            Name = "InvalidateAllAsync",
            Interface = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ]
        });

        return new CodeFileModel<InterfaceModel>(interfaceModel, "ICacheInvalidation", directory, CSharp)
        {
            Namespace = "Cache.Core.Interfaces"
        };
    }

    private static CodeFileModel<ClassModel> CreateCacheUpdatedEventFile(string directory)
    {
        var classModel = new ClassModel("CacheUpdatedEvent")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Key", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "OldValue", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "NewValue", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "ExpiresAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "OccurredAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]) { DefaultValue = "DateTime.UtcNow" });

        return new CodeFileModel<ClassModel>(classModel, "CacheUpdatedEvent", directory, CSharp)
        {
            Namespace = "Cache.Core.Events"
        };
    }

    private static FileModel CreateCacheInvalidatedEventFile(string directory)
    {
        // Keep as FileModel because it contains an enum which can't be expressed with syntax models
        return new FileModel("CacheInvalidatedEvent", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Cache.Core.Events;

                public sealed class CacheInvalidatedEvent
                {
                    public required string Key { get; init; }
                    public string? Pattern { get; init; }
                    public string? Tag { get; init; }
                    public InvalidationReason Reason { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }

                public enum InvalidationReason { Manual, Expiration, Eviction, PatternMatch, TagMatch }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateCacheEntryDtoFile(string directory)
    {
        var classModel = new ClassModel("CacheEntryDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Key", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string"), "Value", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)], required: true));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("string") { Nullable = true }, "ContentType", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CreatedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime") { Nullable = true }, "ExpiresAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "SizeBytes", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "CacheEntryDto", directory, CSharp)
        {
            Namespace = "Cache.Core.DTOs"
        };
    }

    private static FileModel CreateSetCacheRequestDtoFile(string directory)
    {
        // Keep as FileModel because it contains validation attributes which require additional using statements
        return new FileModel("SetCacheRequestDto", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Cache.Core.DTOs;

                public sealed class SetCacheRequestDto
                {
                    [Required]
                    public required string Value { get; init; }

                    public string? ContentType { get; init; }

                    public int? ExpirationSeconds { get; init; }

                    public Dictionary<string, string>? Tags { get; init; }
                }
                """
        };
    }

    private static CodeFileModel<ClassModel> CreateCacheStatisticsDtoFile(string directory)
    {
        var classModel = new ClassModel("CacheStatisticsDto")
        {
            Sealed = true
        };

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "TotalEntries", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "TotalSizeBytes", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "HitCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "MissCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("double"), "HitRatio", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("long"), "EvictionCount", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DateTime"), "CollectedAt", [new PropertyAccessorModel(PropertyAccessorType.Get), new PropertyAccessorModel(PropertyAccessorType.Init)]));

        return new CodeFileModel<ClassModel>(classModel, "CacheStatisticsDto", directory, CSharp)
        {
            Namespace = "Cache.Core.DTOs"
        };
    }

    #endregion

    #region Infrastructure Layer Files

    private static CodeFileModel<ClassModel> CreateCacheDbContextFile(string directory)
    {
        var classModel = new ClassModel("CacheDbContext");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Cache.Core.Entities"));

        classModel.Implements.Add(new TypeModel("DbContext"));

        var constructor = new ConstructorModel(classModel, "CacheDbContext")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "options", Type = new TypeModel("DbContextOptions") { GenericTypeParameters = [new TypeModel("CacheDbContext")] } }],
            BaseParams = ["options"]
        };
        classModel.Constructors.Add(constructor);

        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("CacheEntry")] }, "CacheEntries", [new PropertyAccessorModel(PropertyAccessorType.Get)]));
        classModel.Properties.Add(new PropertyModel(classModel, AccessModifier.Public, new TypeModel("DbSet") { GenericTypeParameters = [new TypeModel("CacheStatistics")] }, "CacheStatistics", [new PropertyAccessorModel(PropertyAccessorType.Get)]));

        classModel.Methods.Add(new MethodModel
        {
            Name = "OnModelCreating",
            AccessModifier = AccessModifier.Protected,
            Override = true,
            ReturnType = new TypeModel("void"),
            Params = [new ParamModel { Name = "modelBuilder", Type = new TypeModel("ModelBuilder") }],
            Body = new ExpressionModel(@"modelBuilder.Entity<CacheEntry>(entity =>
        {
            entity.HasKey(e => e.EntryId);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
        });

        modelBuilder.Entity<CacheStatistics>(entity =>
        {
            entity.HasKey(s => s.StatisticsId);
        });")
        });

        return new CodeFileModel<ClassModel>(classModel, "CacheDbContext", directory, CSharp)
        {
            Namespace = "Cache.Infrastructure.Data"
        };
    }

    private static CodeFileModel<ClassModel> CreateCacheRepositoryFile(string directory)
    {
        var classModel = new ClassModel("CacheRepository");

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Cache.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Cache.Infrastructure.Data"));

        classModel.Fields.Add(new FieldModel
        {
            Name = "context",
            Type = new TypeModel("CacheDbContext"),
            AccessModifier = AccessModifier.Private,
            ReadOnly = true
        });

        var constructor = new ConstructorModel(classModel, "CacheRepository")
        {
            AccessModifier = AccessModifier.Public,
            Params = [new ParamModel { Name = "context", Type = new TypeModel("CacheDbContext") }],
            Body = new ExpressionModel("this.context = context;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetByKeyAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("CacheEntry") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel("=> await context.CacheEntries.FirstOrDefaultAsync(e => e.Key == key, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "CreateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("CacheEntry")] },
            Params =
            [
                new ParamModel { Name = "entry", Type = new TypeModel("CacheEntry") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"entry.EntryId = Guid.NewGuid();
        await context.CacheEntries.AddAsync(entry, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return entry;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "UpdateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "entry", Type = new TypeModel("CacheEntry") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"context.CacheEntries.Update(entry);
        await context.SaveChangesAsync(cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteByKeyAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var entry = await context.CacheEntries.FirstOrDefaultAsync(e => e.Key == key, cancellationToken);
        if (entry == null) return false;

        context.CacheEntries.Remove(entry);
        await context.SaveChangesAsync(cancellationToken);
        return true;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteExpiredAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("int")] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var now = DateTime.UtcNow;
        var expired = await context.CacheEntries
            .Where(e => e.ExpiresAt != null && e.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        context.CacheEntries.RemoveRange(expired);
        await context.SaveChangesAsync(cancellationToken);
        return expired.Count;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteByPatternAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("int")] },
            Params =
            [
                new ParamModel { Name = "pattern", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var entries = await context.CacheEntries
            .Where(e => EF.Functions.Like(e.Key, pattern))
            .ToListAsync(cancellationToken);

        context.CacheEntries.RemoveRange(entries);
        await context.SaveChangesAsync(cancellationToken);
        return entries.Count;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("int")] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var count = await context.CacheEntries.CountAsync(cancellationToken);
        context.CacheEntries.RemoveRange(context.CacheEntries);
        await context.SaveChangesAsync(cancellationToken);
        return count;")
        });

        return new CodeFileModel<ClassModel>(classModel, "CacheRepository", directory, CSharp)
        {
            Namespace = "Cache.Infrastructure.Repositories"
        };
    }

    private static CodeFileModel<ClassModel> CreateCacheServiceFile(string directory)
    {
        var classModel = new ClassModel("CacheService");

        classModel.Usings.Add(new UsingModel("System.Text.Json"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));
        classModel.Usings.Add(new UsingModel("Cache.Core.Entities"));
        classModel.Usings.Add(new UsingModel("Cache.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Cache.Infrastructure.Repositories"));

        classModel.Implements.Add(new TypeModel("ICacheService"));

        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("CacheRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("CacheService")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "hitCount", Type = new TypeModel("long"), AccessModifier = AccessModifier.Private });
        classModel.Fields.Add(new FieldModel { Name = "missCount", Type = new TypeModel("long"), AccessModifier = AccessModifier.Private });

        var constructor = new ConstructorModel(classModel, "CacheService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "repository", Type = new TypeModel("CacheRepository") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("CacheService")] } }
            ],
            Body = new ExpressionModel(@"this.repository = repository;
        this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("CacheEntry") { Nullable = true }] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var entry = await repository.GetByKeyAsync(key, cancellationToken);

        if (entry == null)
        {
            Interlocked.Increment(ref missCount);
            logger.LogDebug(""Cache miss for key: {Key}"", key);
            return null;
        }

        if (entry.ExpiresAt.HasValue && entry.ExpiresAt < DateTime.UtcNow)
        {
            await repository.DeleteByKeyAsync(key, cancellationToken);
            Interlocked.Increment(ref missCount);
            logger.LogDebug(""Cache entry expired for key: {Key}"", key);
            return null;
        }

        Interlocked.Increment(ref hitCount);
        entry.LastAccessedAt = DateTime.UtcNow;
        await repository.UpdateAsync(entry, cancellationToken);
        logger.LogDebug(""Cache hit for key: {Key}"", key);
        return entry;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("T") { Nullable = true }] },
            GenericTypeParameters = ["T"],
            GenericTypeParameterConstraints = ["T : class"],
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var entry = await GetAsync(key, cancellationToken);
        if (entry == null) return null;

        return JsonSerializer.Deserialize<T>(entry.Value);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "SetAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "value", Type = new TypeModel("string") },
                new ParamModel { Name = "expiration", Type = new TypeModel("TimeSpan") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var existingEntry = await repository.GetByKeyAsync(key, cancellationToken);

        if (existingEntry != null)
        {
            existingEntry.Value = value;
            existingEntry.ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null;
            existingEntry.SizeBytes = System.Text.Encoding.UTF8.GetByteCount(value);
            await repository.UpdateAsync(existingEntry, cancellationToken);
        }
        else
        {
            var entry = new CacheEntry
            {
                Key = key,
                Value = value,
                ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null,
                SizeBytes = System.Text.Encoding.UTF8.GetByteCount(value)
            };
            await repository.CreateAsync(entry, cancellationToken);
        }

        logger.LogDebug(""Cache set for key: {Key}"", key);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "SetAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            GenericTypeParameters = ["T"],
            GenericTypeParameterConstraints = ["T : class"],
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "value", Type = new TypeModel("T") },
                new ParamModel { Name = "expiration", Type = new TypeModel("TimeSpan") { Nullable = true }, DefaultValue = "null" },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var json = JsonSerializer.Serialize(value);
        await SetAsync(key, json, expiration, cancellationToken);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "ExistsAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var entry = await repository.GetByKeyAsync(key, cancellationToken);
        return entry != null && (!entry.ExpiresAt.HasValue || entry.ExpiresAt >= DateTime.UtcNow);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "DeleteAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("bool")] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var result = await repository.DeleteByKeyAsync(key, cancellationToken);
        logger.LogDebug(""Cache delete for key: {Key}, result: {Result}"", key, result);
        return result;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "GetStatisticsAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("CacheStatistics")] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var stats = new CacheStatistics
        {
            StatisticsId = Guid.NewGuid(),
            HitCount = Interlocked.Read(ref hitCount),
            MissCount = Interlocked.Read(ref missCount),
            CollectedAt = DateTime.UtcNow
        };
        return Task.FromResult(stats);")
        });

        return new CodeFileModel<ClassModel>(classModel, "CacheService", directory, CSharp)
        {
            Namespace = "Cache.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateCacheInvalidationServiceFile(string directory)
    {
        var classModel = new ClassModel("CacheInvalidationService");

        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Logging"));
        classModel.Usings.Add(new UsingModel("Cache.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Cache.Infrastructure.Repositories"));

        classModel.Implements.Add(new TypeModel("ICacheInvalidation"));

        classModel.Fields.Add(new FieldModel { Name = "repository", Type = new TypeModel("CacheRepository"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("CacheInvalidationService")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "CacheInvalidationService")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "repository", Type = new TypeModel("CacheRepository") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("CacheInvalidationService")] } }
            ],
            Body = new ExpressionModel(@"this.repository = repository;
        this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        classModel.Methods.Add(new MethodModel
        {
            Name = "InvalidateAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"await repository.DeleteByKeyAsync(key, cancellationToken);
        logger.LogInformation(""Invalidated cache entry: {Key}"", key);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "InvalidateByPatternAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "pattern", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var count = await repository.DeleteByPatternAsync(pattern, cancellationToken);
        logger.LogInformation(""Invalidated {Count} cache entries matching pattern: {Pattern}"", count, pattern);")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "InvalidateByTagAsync",
            AccessModifier = AccessModifier.Public,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "tag", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"// Tag-based invalidation requires additional implementation
        logger.LogWarning(""Tag-based invalidation not yet implemented for tag: {Tag}"", tag);
        return Task.CompletedTask;")
        });

        classModel.Methods.Add(new MethodModel
        {
            Name = "InvalidateAllAsync",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task"),
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken"), DefaultValue = "default" }
            ],
            Body = new ExpressionModel(@"var count = await repository.DeleteAllAsync(cancellationToken);
        logger.LogInformation(""Invalidated all {Count} cache entries"", count);")
        });

        return new CodeFileModel<ClassModel>(classModel, "CacheInvalidationService", directory, CSharp)
        {
            Namespace = "Cache.Infrastructure.Services"
        };
    }

    private static CodeFileModel<ClassModel> CreateInfrastructureConfigureServicesFile(string directory)
    {
        var classModel = new ClassModel("ConfigureServices")
        {
            Static = true
        };

        classModel.Usings.Add(new UsingModel("Microsoft.EntityFrameworkCore"));
        classModel.Usings.Add(new UsingModel("Microsoft.Extensions.Configuration"));
        classModel.Usings.Add(new UsingModel("Cache.Core.Interfaces"));
        classModel.Usings.Add(new UsingModel("Cache.Infrastructure.Data"));
        classModel.Usings.Add(new UsingModel("Cache.Infrastructure.Repositories"));
        classModel.Usings.Add(new UsingModel("Cache.Infrastructure.Services"));

        classModel.Methods.Add(new MethodModel
        {
            Name = "AddCacheInfrastructure",
            AccessModifier = AccessModifier.Public,
            Static = true,
            ReturnType = new TypeModel("IServiceCollection"),
            Params =
            [
                new ParamModel { Name = "services", Type = new TypeModel("IServiceCollection"), ExtensionMethodParam = true },
                new ParamModel { Name = "configuration", Type = new TypeModel("IConfiguration") }
            ],
            Body = new ExpressionModel(@"services.AddDbContext<CacheDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(""CacheDb"") ??
                @""Server=.\SQLEXPRESS;Database=CacheDb;Trusted_Connection=True;TrustServerCertificate=True""));

        services.AddScoped<CacheRepository>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ICacheInvalidation, CacheInvalidationService>();
        return services;")
        });

        return new CodeFileModel<ClassModel>(classModel, "ConfigureServices", directory, CSharp)
        {
            Namespace = "Microsoft.Extensions.DependencyInjection"
        };
    }

    #endregion

    #region API Layer Files

    private static CodeFileModel<ClassModel> CreateCacheControllerFile(string directory)
    {
        var classModel = new ClassModel("CacheController");

        classModel.Usings.Add(new UsingModel("Microsoft.AspNetCore.Mvc"));
        classModel.Usings.Add(new UsingModel("Cache.Core.DTOs"));
        classModel.Usings.Add(new UsingModel("Cache.Core.Interfaces"));

        classModel.Implements.Add(new TypeModel("ControllerBase"));

        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ApiController" });
        classModel.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "Route", Template = "\"api/[controller]\"" });

        classModel.Fields.Add(new FieldModel { Name = "cacheService", Type = new TypeModel("ICacheService"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "cacheInvalidation", Type = new TypeModel("ICacheInvalidation"), AccessModifier = AccessModifier.Private, ReadOnly = true });
        classModel.Fields.Add(new FieldModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("CacheController")] }, AccessModifier = AccessModifier.Private, ReadOnly = true });

        var constructor = new ConstructorModel(classModel, "CacheController")
        {
            AccessModifier = AccessModifier.Public,
            Params =
            [
                new ParamModel { Name = "cacheService", Type = new TypeModel("ICacheService") },
                new ParamModel { Name = "cacheInvalidation", Type = new TypeModel("ICacheInvalidation") },
                new ParamModel { Name = "logger", Type = new TypeModel("ILogger") { GenericTypeParameters = [new TypeModel("CacheController")] } }
            ],
            Body = new ExpressionModel(@"this.cacheService = cacheService;
        this.cacheInvalidation = cacheInvalidation;
        this.logger = logger;")
        };
        classModel.Constructors.Add(constructor);

        // Get method
        var getMethod = new MethodModel
        {
            Name = "Get",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("CacheEntryDto")] }] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var entry = await cacheService.GetAsync(key, cancellationToken);

        if (entry == null)
        {
            return NotFound();
        }

        return Ok(new CacheEntryDto
        {
            Key = entry.Key,
            Value = entry.Value,
            ContentType = entry.ContentType,
            CreatedAt = entry.CreatedAt,
            ExpiresAt = entry.ExpiresAt,
            SizeBytes = entry.SizeBytes
        });")
        };
        getMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"{key}\"" });
        getMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(CacheEntryDto), StatusCodes.Status200OK" });
        getMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(getMethod);

        // Set method
        var setMethod = new MethodModel
        {
            Name = "Set",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "request", Type = new TypeModel("SetCacheRequestDto"), Attribute = new AttributeModel() { Name = "FromBody" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(""Key is required"");
        }

        TimeSpan? expiration = request.ExpirationSeconds.HasValue
            ? TimeSpan.FromSeconds(request.ExpirationSeconds.Value)
            : null;

        await cacheService.SetAsync(key, request.Value, expiration, cancellationToken);
        logger.LogInformation(""Cache entry set for key: {Key}"", key);

        return NoContent();")
        };
        setMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPut", Template = "\"{key}\"" });
        setMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status204NoContent" });
        setMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status400BadRequest" });
        classModel.Methods.Add(setMethod);

        // Delete method
        var deleteMethod = new MethodModel
        {
            Name = "Delete",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "key", Type = new TypeModel("string") },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var deleted = await cacheService.DeleteAsync(key, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        logger.LogInformation(""Cache entry deleted for key: {Key}"", key);
        return NoContent();")
        };
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpDelete", Template = "\"{key}\"" });
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status204NoContent" });
        deleteMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status404NotFound" });
        classModel.Methods.Add(deleteMethod);

        // GetStatistics method
        var getStatisticsMethod = new MethodModel
        {
            Name = "GetStatistics",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("ActionResult") { GenericTypeParameters = [new TypeModel("CacheStatisticsDto")] }] },
            Params =
            [
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"var stats = await cacheService.GetStatisticsAsync(cancellationToken);

        return Ok(new CacheStatisticsDto
        {
            TotalEntries = stats.TotalEntries,
            TotalSizeBytes = stats.TotalSizeBytes,
            HitCount = stats.HitCount,
            MissCount = stats.MissCount,
            HitRatio = stats.HitRatio,
            EvictionCount = stats.EvictionCount,
            CollectedAt = stats.CollectedAt
        });")
        };
        getStatisticsMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpGet", Template = "\"statistics\"" });
        getStatisticsMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "typeof(CacheStatisticsDto), StatusCodes.Status200OK" });
        classModel.Methods.Add(getStatisticsMethod);

        // Invalidate method
        var invalidateMethod = new MethodModel
        {
            Name = "Invalidate",
            AccessModifier = AccessModifier.Public,
            Async = true,
            ReturnType = new TypeModel("Task") { GenericTypeParameters = [new TypeModel("IActionResult")] },
            Params =
            [
                new ParamModel { Name = "pattern", Type = new TypeModel("string") { Nullable = true }, Attribute = new AttributeModel() { Name = "FromQuery" } },
                new ParamModel { Name = "tag", Type = new TypeModel("string") { Nullable = true }, Attribute = new AttributeModel() { Name = "FromQuery" } },
                new ParamModel { Name = "cancellationToken", Type = new TypeModel("CancellationToken") }
            ],
            Body = new ExpressionModel(@"if (!string.IsNullOrWhiteSpace(pattern))
        {
            await cacheInvalidation.InvalidateByPatternAsync(pattern, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(tag))
        {
            await cacheInvalidation.InvalidateByTagAsync(tag, cancellationToken);
        }
        else
        {
            await cacheInvalidation.InvalidateAllAsync(cancellationToken);
        }

        return NoContent();")
        };
        invalidateMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "HttpPost", Template = "\"invalidate\"" });
        invalidateMethod.Attributes.Add(new Endpoint.DotNet.Syntax.Attributes.AttributeModel { Name = "ProducesResponseType", Template = "StatusCodes.Status204NoContent" });
        classModel.Methods.Add(invalidateMethod);

        return new CodeFileModel<ClassModel>(classModel, "CacheController", directory, CSharp)
        {
            Namespace = "Cache.Api.Controllers"
        };
    }

    private static FileModel CreateProgramFile(string directory)
    {
        // Keep as FileModel because it uses top-level statements
        return new FileModel("Program", directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddCacheInfrastructure(builder.Configuration);
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
        };
    }

    private static FileModel CreateAppSettingsFile(string directory)
    {
        // Keep as FileModel for JSON files
        return new FileModel("appsettings", directory, ".json")
        {
            Body = """
                {
                  "ConnectionStrings": {
                    "CacheDb": "Server=.\\SQLEXPRESS;Database=CacheDb;Trusted_Connection=True;TrustServerCertificate=True"
                  },
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information"
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        };
    }

    #endregion
}
