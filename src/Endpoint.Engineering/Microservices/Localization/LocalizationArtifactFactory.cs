// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Localization;

public class LocalizationArtifactFactory : ILocalizationArtifactFactory
{
    private readonly ILogger<LocalizationArtifactFactory> logger;

    public LocalizationArtifactFactory(ILogger<LocalizationArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Localization.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("Translation", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Localization.Core.Entities;

                public class Translation
                {
                    public Guid TranslationId { get; set; }
                    public Guid TranslationKeyId { get; set; }
                    public Guid LanguageId { get; set; }
                    public required string Value { get; set; }
                    public bool IsApproved { get; set; } = false;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public TranslationKey TranslationKey { get; set; } = null!;
                    public Language Language { get; set; } = null!;
                }
                """
        });

        project.Files.Add(new FileModel("Language", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Localization.Core.Entities;

                public class Language
                {
                    public Guid LanguageId { get; set; }
                    public required string Code { get; set; }
                    public required string Name { get; set; }
                    public required string NativeName { get; set; }
                    public bool IsEnabled { get; set; } = true;
                    public bool IsDefault { get; set; } = false;
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public ICollection<Translation> Translations { get; set; } = new List<Translation>();
                }
                """
        });

        project.Files.Add(new FileModel("TranslationKey", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Localization.Core.Entities;

                public class TranslationKey
                {
                    public Guid TranslationKeyId { get; set; }
                    public required string Key { get; set; }
                    public string? Description { get; set; }
                    public string? Context { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? UpdatedAt { get; set; }
                    public ICollection<Translation> Translations { get; set; } = new List<Translation>();
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("ITranslationRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Localization.Core.Entities;

                namespace Localization.Core.Interfaces;

                public interface ITranslationRepository
                {
                    Task<Translation?> GetByIdAsync(Guid translationId, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Translation>> GetByLanguageCodeAsync(string languageCode, CancellationToken cancellationToken = default);
                    Task<Translation?> GetByKeyAndLanguageAsync(string key, string languageCode, CancellationToken cancellationToken = default);
                    Task<IEnumerable<Translation>> GetAllAsync(CancellationToken cancellationToken = default);
                    Task<Translation> AddAsync(Translation translation, CancellationToken cancellationToken = default);
                    Task UpdateAsync(Translation translation, CancellationToken cancellationToken = default);
                    Task DeleteAsync(Guid translationId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ILocalizationService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Localization.Core.DTOs;

                namespace Localization.Core.Interfaces;

                public interface ILocalizationService
                {
                    Task<IEnumerable<TranslationDto>> GetTranslationsByLanguageAsync(string languageCode, CancellationToken cancellationToken = default);
                    Task<TranslationDto> CreateTranslationAsync(CreateTranslationRequest request, CancellationToken cancellationToken = default);
                    Task<TranslationDto> UpdateTranslationAsync(string key, UpdateTranslationRequest request, CancellationToken cancellationToken = default);
                    Task<IEnumerable<LanguageDto>> GetEnabledLanguagesAsync(CancellationToken cancellationToken = default);
                    Task EnableLanguageAsync(Guid languageId, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("TranslationAddedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Localization.Core.Events;

                public sealed class TranslationAddedEvent
                {
                    public Guid TranslationId { get; init; }
                    public required string Key { get; init; }
                    public required string LanguageCode { get; init; }
                    public required string Value { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("LanguageEnabledEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Localization.Core.Events;

                public sealed class LanguageEnabledEvent
                {
                    public Guid LanguageId { get; init; }
                    public required string Code { get; init; }
                    public required string Name { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("TranslationDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Localization.Core.DTOs;

                public sealed class TranslationDto
                {
                    public Guid TranslationId { get; init; }
                    public required string Key { get; init; }
                    public required string LanguageCode { get; init; }
                    public required string Value { get; init; }
                    public bool IsApproved { get; init; }
                    public DateTime CreatedAt { get; init; }
                    public DateTime? UpdatedAt { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("LanguageDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Localization.Core.DTOs;

                public sealed class LanguageDto
                {
                    public Guid LanguageId { get; init; }
                    public required string Code { get; init; }
                    public required string Name { get; init; }
                    public required string NativeName { get; init; }
                    public bool IsEnabled { get; init; }
                    public bool IsDefault { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("CreateTranslationRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Localization.Core.DTOs;

                public sealed class CreateTranslationRequest
                {
                    [Required]
                    public required string Key { get; init; }

                    [Required]
                    public required string LanguageCode { get; init; }

                    [Required]
                    public required string Value { get; init; }

                    public string? Description { get; init; }
                    public string? Context { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("UpdateTranslationRequest", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Localization.Core.DTOs;

                public sealed class UpdateTranslationRequest
                {
                    [Required]
                    public required string LanguageCode { get; init; }

                    [Required]
                    public required string Value { get; init; }

                    public bool? IsApproved { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Localization.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("LocalizationDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Localization.Core.Entities;

                namespace Localization.Infrastructure.Data;

                public class LocalizationDbContext : DbContext
                {
                    public LocalizationDbContext(DbContextOptions<LocalizationDbContext> options) : base(options) { }

                    public DbSet<Translation> Translations => Set<Translation>();
                    public DbSet<Language> Languages => Set<Language>();
                    public DbSet<TranslationKey> TranslationKeys => Set<TranslationKey>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<Translation>(entity =>
                        {
                            entity.HasKey(t => t.TranslationId);
                            entity.Property(t => t.Value).IsRequired();
                            entity.HasOne(t => t.TranslationKey).WithMany(k => k.Translations).HasForeignKey(t => t.TranslationKeyId);
                            entity.HasOne(t => t.Language).WithMany(l => l.Translations).HasForeignKey(t => t.LanguageId);
                            entity.HasIndex(t => new { t.TranslationKeyId, t.LanguageId }).IsUnique();
                        });

                        modelBuilder.Entity<Language>(entity =>
                        {
                            entity.HasKey(l => l.LanguageId);
                            entity.Property(l => l.Code).IsRequired().HasMaxLength(10);
                            entity.Property(l => l.Name).IsRequired().HasMaxLength(100);
                            entity.Property(l => l.NativeName).IsRequired().HasMaxLength(100);
                            entity.HasIndex(l => l.Code).IsUnique();
                        });

                        modelBuilder.Entity<TranslationKey>(entity =>
                        {
                            entity.HasKey(k => k.TranslationKeyId);
                            entity.Property(k => k.Key).IsRequired().HasMaxLength(500);
                            entity.HasIndex(k => k.Key).IsUnique();
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("TranslationRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Localization.Core.Entities;
                using Localization.Core.Interfaces;
                using Localization.Infrastructure.Data;

                namespace Localization.Infrastructure.Repositories;

                public class TranslationRepository : ITranslationRepository
                {
                    private readonly LocalizationDbContext context;

                    public TranslationRepository(LocalizationDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<Translation?> GetByIdAsync(Guid translationId, CancellationToken cancellationToken = default)
                        => await context.Translations
                            .Include(t => t.TranslationKey)
                            .Include(t => t.Language)
                            .FirstOrDefaultAsync(t => t.TranslationId == translationId, cancellationToken);

                    public async Task<IEnumerable<Translation>> GetByLanguageCodeAsync(string languageCode, CancellationToken cancellationToken = default)
                        => await context.Translations
                            .Include(t => t.TranslationKey)
                            .Include(t => t.Language)
                            .Where(t => t.Language.Code == languageCode)
                            .ToListAsync(cancellationToken);

                    public async Task<Translation?> GetByKeyAndLanguageAsync(string key, string languageCode, CancellationToken cancellationToken = default)
                        => await context.Translations
                            .Include(t => t.TranslationKey)
                            .Include(t => t.Language)
                            .FirstOrDefaultAsync(t => t.TranslationKey.Key == key && t.Language.Code == languageCode, cancellationToken);

                    public async Task<IEnumerable<Translation>> GetAllAsync(CancellationToken cancellationToken = default)
                        => await context.Translations
                            .Include(t => t.TranslationKey)
                            .Include(t => t.Language)
                            .ToListAsync(cancellationToken);

                    public async Task<Translation> AddAsync(Translation translation, CancellationToken cancellationToken = default)
                    {
                        translation.TranslationId = Guid.NewGuid();
                        await context.Translations.AddAsync(translation, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return translation;
                    }

                    public async Task UpdateAsync(Translation translation, CancellationToken cancellationToken = default)
                    {
                        translation.UpdatedAt = DateTime.UtcNow;
                        context.Translations.Update(translation);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteAsync(Guid translationId, CancellationToken cancellationToken = default)
                    {
                        var translation = await context.Translations.FindAsync(new object[] { translationId }, cancellationToken);
                        if (translation != null)
                        {
                            context.Translations.Remove(translation);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("LocalizationService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Localization.Core.DTOs;
                using Localization.Core.Entities;
                using Localization.Core.Interfaces;
                using Localization.Infrastructure.Data;

                namespace Localization.Infrastructure.Services;

                public class LocalizationService : ILocalizationService
                {
                    private readonly ITranslationRepository repository;
                    private readonly LocalizationDbContext context;

                    public LocalizationService(ITranslationRepository repository, LocalizationDbContext context)
                    {
                        this.repository = repository;
                        this.context = context;
                    }

                    public async Task<IEnumerable<TranslationDto>> GetTranslationsByLanguageAsync(string languageCode, CancellationToken cancellationToken = default)
                    {
                        var translations = await repository.GetByLanguageCodeAsync(languageCode, cancellationToken);
                        return translations.Select(t => new TranslationDto
                        {
                            TranslationId = t.TranslationId,
                            Key = t.TranslationKey.Key,
                            LanguageCode = t.Language.Code,
                            Value = t.Value,
                            IsApproved = t.IsApproved,
                            CreatedAt = t.CreatedAt,
                            UpdatedAt = t.UpdatedAt
                        });
                    }

                    public async Task<TranslationDto> CreateTranslationAsync(CreateTranslationRequest request, CancellationToken cancellationToken = default)
                    {
                        var language = await context.Languages.FirstOrDefaultAsync(l => l.Code == request.LanguageCode, cancellationToken)
                            ?? throw new InvalidOperationException($"Language '{request.LanguageCode}' not found");

                        var translationKey = await context.TranslationKeys.FirstOrDefaultAsync(k => k.Key == request.Key, cancellationToken);
                        if (translationKey == null)
                        {
                            translationKey = new TranslationKey
                            {
                                TranslationKeyId = Guid.NewGuid(),
                                Key = request.Key,
                                Description = request.Description,
                                Context = request.Context
                            };
                            await context.TranslationKeys.AddAsync(translationKey, cancellationToken);
                            await context.SaveChangesAsync(cancellationToken);
                        }

                        var translation = new Translation
                        {
                            TranslationKeyId = translationKey.TranslationKeyId,
                            LanguageId = language.LanguageId,
                            Value = request.Value
                        };

                        var created = await repository.AddAsync(translation, cancellationToken);

                        return new TranslationDto
                        {
                            TranslationId = created.TranslationId,
                            Key = request.Key,
                            LanguageCode = request.LanguageCode,
                            Value = created.Value,
                            IsApproved = created.IsApproved,
                            CreatedAt = created.CreatedAt,
                            UpdatedAt = created.UpdatedAt
                        };
                    }

                    public async Task<TranslationDto> UpdateTranslationAsync(string key, UpdateTranslationRequest request, CancellationToken cancellationToken = default)
                    {
                        var translation = await repository.GetByKeyAndLanguageAsync(key, request.LanguageCode, cancellationToken)
                            ?? throw new InvalidOperationException($"Translation not found for key '{key}' and language '{request.LanguageCode}'");

                        translation.Value = request.Value;
                        if (request.IsApproved.HasValue)
                        {
                            translation.IsApproved = request.IsApproved.Value;
                        }

                        await repository.UpdateAsync(translation, cancellationToken);

                        return new TranslationDto
                        {
                            TranslationId = translation.TranslationId,
                            Key = translation.TranslationKey.Key,
                            LanguageCode = translation.Language.Code,
                            Value = translation.Value,
                            IsApproved = translation.IsApproved,
                            CreatedAt = translation.CreatedAt,
                            UpdatedAt = translation.UpdatedAt
                        };
                    }

                    public async Task<IEnumerable<LanguageDto>> GetEnabledLanguagesAsync(CancellationToken cancellationToken = default)
                    {
                        var languages = await context.Languages.Where(l => l.IsEnabled).ToListAsync(cancellationToken);
                        return languages.Select(l => new LanguageDto
                        {
                            LanguageId = l.LanguageId,
                            Code = l.Code,
                            Name = l.Name,
                            NativeName = l.NativeName,
                            IsEnabled = l.IsEnabled,
                            IsDefault = l.IsDefault
                        });
                    }

                    public async Task EnableLanguageAsync(Guid languageId, CancellationToken cancellationToken = default)
                    {
                        var language = await context.Languages.FindAsync(new object[] { languageId }, cancellationToken)
                            ?? throw new InvalidOperationException($"Language with ID '{languageId}' not found");

                        language.IsEnabled = true;
                        language.UpdatedAt = DateTime.UtcNow;
                        await context.SaveChangesAsync(cancellationToken);
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
                using Localization.Core.Interfaces;
                using Localization.Infrastructure.Data;
                using Localization.Infrastructure.Repositories;
                using Localization.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddLocalizationInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<LocalizationDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("LocalizationDb") ??
                                @"Server=.\SQLEXPRESS;Database=LocalizationDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<ITranslationRepository, TranslationRepository>();
                        services.AddScoped<ILocalizationService, LocalizationService>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Localization.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("TranslationsController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using Localization.Core.DTOs;
                using Localization.Core.Interfaces;

                namespace Localization.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class TranslationsController : ControllerBase
                {
                    private readonly ILocalizationService service;

                    public TranslationsController(ILocalizationService service)
                    {
                        this.service = service;
                    }

                    [HttpGet("{languageCode}")]
                    public async Task<ActionResult<IEnumerable<TranslationDto>>> GetByLanguage(string languageCode, CancellationToken cancellationToken)
                    {
                        var translations = await service.GetTranslationsByLanguageAsync(languageCode, cancellationToken);
                        return Ok(translations);
                    }

                    [HttpPost]
                    public async Task<ActionResult<TranslationDto>> Create([FromBody] CreateTranslationRequest request, CancellationToken cancellationToken)
                    {
                        var translation = await service.CreateTranslationAsync(request, cancellationToken);
                        return CreatedAtAction(nameof(GetByLanguage), new { languageCode = translation.LanguageCode }, translation);
                    }

                    [HttpPut("{key}")]
                    public async Task<ActionResult<TranslationDto>> Update(string key, [FromBody] UpdateTranslationRequest request, CancellationToken cancellationToken)
                    {
                        var translation = await service.UpdateTranslationAsync(key, request, cancellationToken);
                        return Ok(translation);
                    }

                    [HttpGet("languages")]
                    public async Task<ActionResult<IEnumerable<LanguageDto>>> GetLanguages(CancellationToken cancellationToken)
                    {
                        var languages = await service.GetEnabledLanguagesAsync(cancellationToken);
                        return Ok(languages);
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

                builder.Services.AddLocalizationInfrastructure(builder.Configuration);
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
                    "LocalizationDb": "Server=.\\SQLEXPRESS;Database=LocalizationDb;Trusted_Connection=True;TrustServerCertificate=True"
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
