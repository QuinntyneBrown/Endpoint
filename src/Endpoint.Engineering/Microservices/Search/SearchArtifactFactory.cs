// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.Search;

public class SearchArtifactFactory : ISearchArtifactFactory
{
    private readonly ILogger<SearchArtifactFactory> logger;

    public SearchArtifactFactory(ILogger<SearchArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding Search.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("SearchIndex", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Search.Core.Entities;

                public class SearchIndex
                {
                    public Guid IndexId { get; set; }
                    public required string Name { get; set; }
                    public required string DocumentType { get; set; }
                    public long DocumentCount { get; set; }
                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
                    public DateTime? LastUpdatedAt { get; set; }
                    public IndexStatus Status { get; set; } = IndexStatus.Active;
                }

                public enum IndexStatus { Active, Building, Inactive }
                """
        });

        project.Files.Add(new FileModel("SearchQuery", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Search.Core.Entities;

                public class SearchQuery
                {
                    public Guid QueryId { get; set; }
                    public required string QueryText { get; set; }
                    public string? IndexName { get; set; }
                    public int Skip { get; set; }
                    public int Take { get; set; } = 10;
                    public Dictionary<string, string> Filters { get; set; } = new();
                    public string? SortBy { get; set; }
                    public bool SortDescending { get; set; }
                    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("SearchResult", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Search.Core.Entities;

                public class SearchResult
                {
                    public Guid ResultId { get; set; }
                    public required string DocumentId { get; set; }
                    public required string IndexName { get; set; }
                    public double Score { get; set; }
                    public Dictionary<string, object> Document { get; set; } = new();
                    public Dictionary<string, List<string>> Highlights { get; set; } = new();
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("ISearchRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Search.Core.Entities;

                namespace Search.Core.Interfaces;

                public interface ISearchRepository
                {
                    Task<SearchIndex?> GetIndexByIdAsync(Guid indexId, CancellationToken cancellationToken = default);
                    Task<SearchIndex?> GetIndexByNameAsync(string name, CancellationToken cancellationToken = default);
                    Task<IEnumerable<SearchIndex>> GetAllIndexesAsync(CancellationToken cancellationToken = default);
                    Task<SearchIndex> CreateIndexAsync(SearchIndex index, CancellationToken cancellationToken = default);
                    Task UpdateIndexAsync(SearchIndex index, CancellationToken cancellationToken = default);
                    Task DeleteIndexAsync(Guid indexId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("ISearchService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Search.Core.Entities;

                namespace Search.Core.Interfaces;

                public interface ISearchService
                {
                    Task<IEnumerable<SearchResult>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);
                    Task<SearchResult?> GetDocumentAsync(string indexName, string documentId, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IIndexer", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Search.Core.Interfaces;

                public interface IIndexer
                {
                    Task IndexDocumentAsync(string indexName, string documentId, Dictionary<string, object> document, CancellationToken cancellationToken = default);
                    Task IndexDocumentsAsync(string indexName, IEnumerable<(string documentId, Dictionary<string, object> document)> documents, CancellationToken cancellationToken = default);
                    Task RemoveDocumentAsync(string indexName, string documentId, CancellationToken cancellationToken = default);
                    Task RemoveDocumentsAsync(string indexName, IEnumerable<string> documentIds, CancellationToken cancellationToken = default);
                    Task RebuildIndexAsync(string indexName, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("DocumentIndexedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Search.Core.Events;

                public sealed class DocumentIndexedEvent
                {
                    public required string IndexName { get; init; }
                    public required string DocumentId { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("DocumentRemovedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Search.Core.Events;

                public sealed class DocumentRemovedEvent
                {
                    public required string IndexName { get; init; }
                    public required string DocumentId { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        project.Files.Add(new FileModel("SearchPerformedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Search.Core.Events;

                public sealed class SearchPerformedEvent
                {
                    public Guid QueryId { get; init; }
                    public required string QueryText { get; init; }
                    public string? IndexName { get; init; }
                    public int ResultCount { get; init; }
                    public long ExecutionTimeMs { get; init; }
                    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
                }
                """
        });

        // DTOs
        project.Files.Add(new FileModel("SearchRequestDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Search.Core.DTOs;

                public sealed class SearchRequestDto
                {
                    [Required]
                    public required string Query { get; init; }

                    public string? IndexName { get; init; }

                    public int Skip { get; init; }

                    public int Take { get; init; } = 10;

                    public Dictionary<string, string>? Filters { get; init; }

                    public string? SortBy { get; init; }

                    public bool SortDescending { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("SearchResultDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Search.Core.DTOs;

                public sealed class SearchResultDto
                {
                    public required string DocumentId { get; init; }
                    public required string IndexName { get; init; }
                    public double Score { get; init; }
                    public Dictionary<string, object> Document { get; init; } = new();
                    public Dictionary<string, List<string>> Highlights { get; init; } = new();
                }
                """
        });

        project.Files.Add(new FileModel("SearchResponseDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Search.Core.DTOs;

                public sealed class SearchResponseDto
                {
                    public IReadOnlyList<SearchResultDto> Results { get; init; } = Array.Empty<SearchResultDto>();
                    public int TotalCount { get; init; }
                    public int Skip { get; init; }
                    public int Take { get; init; }
                    public long ExecutionTimeMs { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("IndexDocumentRequestDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.ComponentModel.DataAnnotations;

                namespace Search.Core.DTOs;

                public sealed class IndexDocumentRequestDto
                {
                    [Required]
                    public required string DocumentId { get; init; }

                    [Required]
                    public required Dictionary<string, object> Document { get; init; }
                }
                """
        });

        project.Files.Add(new FileModel("IndexInfoDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace Search.Core.DTOs;

                public sealed class IndexInfoDto
                {
                    public Guid IndexId { get; init; }
                    public required string Name { get; init; }
                    public required string DocumentType { get; init; }
                    public long DocumentCount { get; init; }
                    public string Status { get; init; } = "Active";
                    public DateTime CreatedAt { get; init; }
                    public DateTime? LastUpdatedAt { get; init; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Search.Infrastructure files");
        var dataDir = Path.Combine(project.Directory, "Data");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var servicesDir = Path.Combine(project.Directory, "Services");

        project.Files.Add(new FileModel("SearchDbContext", dataDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Search.Core.Entities;

                namespace Search.Infrastructure.Data;

                public class SearchDbContext : DbContext
                {
                    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options) { }

                    public DbSet<SearchIndex> SearchIndexes => Set<SearchIndex>();

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {
                        modelBuilder.Entity<SearchIndex>(entity =>
                        {
                            entity.HasKey(i => i.IndexId);
                            entity.Property(i => i.Name).IsRequired().HasMaxLength(200);
                            entity.Property(i => i.DocumentType).IsRequired().HasMaxLength(100);
                            entity.HasIndex(i => i.Name).IsUnique();
                        });
                    }
                }
                """
        });

        project.Files.Add(new FileModel("SearchRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.EntityFrameworkCore;
                using Search.Core.Entities;
                using Search.Core.Interfaces;
                using Search.Infrastructure.Data;

                namespace Search.Infrastructure.Repositories;

                public class SearchRepository : ISearchRepository
                {
                    private readonly SearchDbContext context;

                    public SearchRepository(SearchDbContext context)
                    {
                        this.context = context;
                    }

                    public async Task<SearchIndex?> GetIndexByIdAsync(Guid indexId, CancellationToken cancellationToken = default)
                        => await context.SearchIndexes.FirstOrDefaultAsync(i => i.IndexId == indexId, cancellationToken);

                    public async Task<SearchIndex?> GetIndexByNameAsync(string name, CancellationToken cancellationToken = default)
                        => await context.SearchIndexes.FirstOrDefaultAsync(i => i.Name == name, cancellationToken);

                    public async Task<IEnumerable<SearchIndex>> GetAllIndexesAsync(CancellationToken cancellationToken = default)
                        => await context.SearchIndexes.ToListAsync(cancellationToken);

                    public async Task<SearchIndex> CreateIndexAsync(SearchIndex index, CancellationToken cancellationToken = default)
                    {
                        index.IndexId = Guid.NewGuid();
                        await context.SearchIndexes.AddAsync(index, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        return index;
                    }

                    public async Task UpdateIndexAsync(SearchIndex index, CancellationToken cancellationToken = default)
                    {
                        index.LastUpdatedAt = DateTime.UtcNow;
                        context.SearchIndexes.Update(index);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    public async Task DeleteIndexAsync(Guid indexId, CancellationToken cancellationToken = default)
                    {
                        var index = await context.SearchIndexes.FindAsync(new object[] { indexId }, cancellationToken);
                        if (index != null)
                        {
                            context.SearchIndexes.Remove(index);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                """
        });

        project.Files.Add(new FileModel("SearchService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.Extensions.Logging;
                using Search.Core.Entities;
                using Search.Core.Interfaces;

                namespace Search.Infrastructure.Services;

                public class SearchService : ISearchService
                {
                    private readonly ILogger<SearchService> logger;

                    public SearchService(ILogger<SearchService> logger)
                    {
                        this.logger = logger;
                    }

                    public async Task<IEnumerable<SearchResult>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Executing search query: {Query}", query.QueryText);

                        // Placeholder implementation - integrate with actual search engine (Elasticsearch, Azure Cognitive Search, etc.)
                        await Task.CompletedTask;
                        return Array.Empty<SearchResult>();
                    }

                    public async Task<SearchResult?> GetDocumentAsync(string indexName, string documentId, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Getting document {DocumentId} from index {IndexName}", documentId, indexName);

                        // Placeholder implementation
                        await Task.CompletedTask;
                        return null;
                    }
                }
                """
        });

        project.Files.Add(new FileModel("Indexer", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.Extensions.Logging;
                using Search.Core.Interfaces;

                namespace Search.Infrastructure.Services;

                public class Indexer : IIndexer
                {
                    private readonly ILogger<Indexer> logger;

                    public Indexer(ILogger<Indexer> logger)
                    {
                        this.logger = logger;
                    }

                    public async Task IndexDocumentAsync(string indexName, string documentId, Dictionary<string, object> document, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Indexing document {DocumentId} in index {IndexName}", documentId, indexName);

                        // Placeholder implementation - integrate with actual search engine
                        await Task.CompletedTask;
                    }

                    public async Task IndexDocumentsAsync(string indexName, IEnumerable<(string documentId, Dictionary<string, object> document)> documents, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Bulk indexing documents in index {IndexName}", indexName);

                        foreach (var (documentId, document) in documents)
                        {
                            await IndexDocumentAsync(indexName, documentId, document, cancellationToken);
                        }
                    }

                    public async Task RemoveDocumentAsync(string indexName, string documentId, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Removing document {DocumentId} from index {IndexName}", documentId, indexName);

                        // Placeholder implementation
                        await Task.CompletedTask;
                    }

                    public async Task RemoveDocumentsAsync(string indexName, IEnumerable<string> documentIds, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Bulk removing documents from index {IndexName}", indexName);

                        foreach (var documentId in documentIds)
                        {
                            await RemoveDocumentAsync(indexName, documentId, cancellationToken);
                        }
                    }

                    public async Task RebuildIndexAsync(string indexName, CancellationToken cancellationToken = default)
                    {
                        logger.LogInformation("Rebuilding index {IndexName}", indexName);

                        // Placeholder implementation
                        await Task.CompletedTask;
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
                using Search.Core.Interfaces;
                using Search.Infrastructure.Data;
                using Search.Infrastructure.Repositories;
                using Search.Infrastructure.Services;

                namespace Microsoft.Extensions.DependencyInjection;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddSearchInfrastructure(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddDbContext<SearchDbContext>(options =>
                            options.UseSqlServer(configuration.GetConnectionString("SearchDb") ??
                                @"Server=.\SQLEXPRESS;Database=SearchDb;Trusted_Connection=True;TrustServerCertificate=True"));

                        services.AddScoped<ISearchRepository, SearchRepository>();
                        services.AddScoped<ISearchService, SearchService>();
                        services.AddScoped<IIndexer, Indexer>();
                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding Search.Api files");
        var controllersDir = Path.Combine(project.Directory, "Controllers");

        project.Files.Add(new FileModel("SearchController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Diagnostics;
                using Microsoft.AspNetCore.Mvc;
                using Search.Core.DTOs;
                using Search.Core.Entities;
                using Search.Core.Interfaces;

                namespace Search.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class SearchController : ControllerBase
                {
                    private readonly ISearchService searchService;
                    private readonly IIndexer indexer;
                    private readonly ISearchRepository repository;
                    private readonly ILogger<SearchController> logger;

                    public SearchController(
                        ISearchService searchService,
                        IIndexer indexer,
                        ISearchRepository repository,
                        ILogger<SearchController> logger)
                    {
                        this.searchService = searchService;
                        this.indexer = indexer;
                        this.repository = repository;
                        this.logger = logger;
                    }

                    /// <summary>
                    /// Execute a search query.
                    /// </summary>
                    [HttpPost]
                    [ProducesResponseType(typeof(SearchResponseDto), StatusCodes.Status200OK)]
                    public async Task<ActionResult<SearchResponseDto>> Search([FromBody] SearchRequestDto request, CancellationToken cancellationToken)
                    {
                        var stopwatch = Stopwatch.StartNew();

                        var query = new SearchQuery
                        {
                            QueryId = Guid.NewGuid(),
                            QueryText = request.Query,
                            IndexName = request.IndexName,
                            Skip = request.Skip,
                            Take = request.Take,
                            Filters = request.Filters ?? new Dictionary<string, string>(),
                            SortBy = request.SortBy,
                            SortDescending = request.SortDescending
                        };

                        var results = await searchService.SearchAsync(query, cancellationToken);
                        stopwatch.Stop();

                        var resultList = results.Select(r => new SearchResultDto
                        {
                            DocumentId = r.DocumentId,
                            IndexName = r.IndexName,
                            Score = r.Score,
                            Document = r.Document,
                            Highlights = r.Highlights
                        }).ToList();

                        return Ok(new SearchResponseDto
                        {
                            Results = resultList,
                            TotalCount = resultList.Count,
                            Skip = request.Skip,
                            Take = request.Take,
                            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                        });
                    }

                    /// <summary>
                    /// Index a document.
                    /// </summary>
                    [HttpPost("index")]
                    [ProducesResponseType(StatusCodes.Status201Created)]
                    [ProducesResponseType(StatusCodes.Status400BadRequest)]
                    public async Task<IActionResult> IndexDocument(
                        [FromQuery] string indexName,
                        [FromBody] IndexDocumentRequestDto request,
                        CancellationToken cancellationToken)
                    {
                        if (string.IsNullOrWhiteSpace(indexName))
                        {
                            return BadRequest("Index name is required");
                        }

                        await indexer.IndexDocumentAsync(indexName, request.DocumentId, request.Document, cancellationToken);
                        logger.LogInformation("Document {DocumentId} indexed in {IndexName}", request.DocumentId, indexName);

                        return Created($"/api/search/index/{indexName}/{request.DocumentId}", null);
                    }

                    /// <summary>
                    /// Remove a document from the index.
                    /// </summary>
                    [HttpDelete("index/{id}")]
                    [ProducesResponseType(StatusCodes.Status204NoContent)]
                    public async Task<IActionResult> RemoveDocument(
                        string id,
                        [FromQuery] string indexName,
                        CancellationToken cancellationToken)
                    {
                        await indexer.RemoveDocumentAsync(indexName, id, cancellationToken);
                        logger.LogInformation("Document {DocumentId} removed from {IndexName}", id, indexName);

                        return NoContent();
                    }

                    /// <summary>
                    /// Get all search indexes.
                    /// </summary>
                    [HttpGet("indexes")]
                    [ProducesResponseType(typeof(IEnumerable<IndexInfoDto>), StatusCodes.Status200OK)]
                    public async Task<ActionResult<IEnumerable<IndexInfoDto>>> GetIndexes(CancellationToken cancellationToken)
                    {
                        var indexes = await repository.GetAllIndexesAsync(cancellationToken);
                        return Ok(indexes.Select(i => new IndexInfoDto
                        {
                            IndexId = i.IndexId,
                            Name = i.Name,
                            DocumentType = i.DocumentType,
                            DocumentCount = i.DocumentCount,
                            Status = i.Status.ToString(),
                            CreatedAt = i.CreatedAt,
                            LastUpdatedAt = i.LastUpdatedAt
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

                builder.Services.AddSearchInfrastructure(builder.Configuration);
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
                    "SearchDb": "Server=.\\SQLEXPRESS;Database=SearchDb;Trusted_Connection=True;TrustServerCertificate=True"
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
