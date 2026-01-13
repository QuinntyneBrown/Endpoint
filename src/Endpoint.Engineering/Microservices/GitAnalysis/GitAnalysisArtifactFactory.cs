// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Engineering.Microservices.GitAnalysis;

/// <summary>
/// Factory for creating Git Analysis microservice artifacts.
/// Provides Git operations including branch isolation, diff generation, and .gitignore parsing.
/// </summary>
public class GitAnalysisArtifactFactory : IGitAnalysisArtifactFactory
{
    private readonly ILogger<GitAnalysisArtifactFactory> logger;

    public GitAnalysisArtifactFactory(ILogger<GitAnalysisArtifactFactory> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddCoreFiles(ProjectModel project)
    {
        logger.LogInformation("Adding GitAnalysis.Core files");
        var entitiesDir = Path.Combine(project.Directory, "Entities");
        var interfacesDir = Path.Combine(project.Directory, "Interfaces");
        var eventsDir = Path.Combine(project.Directory, "Events");
        var dtosDir = Path.Combine(project.Directory, "DTOs");

        // Entities
        project.Files.Add(new FileModel("GitComparisonRequest", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace GitAnalysis.Core.Entities;

                /// <summary>
                /// Request for Git comparison between two branches.
                /// </summary>
                public class GitComparisonRequest
                {
                    public Guid RequestId { get; set; }
                    public required string RepositoryPath { get; set; }
                    public required string SourceBranch { get; set; }
                    public required string TargetBranch { get; set; }
                    public string? UserId { get; set; }
                    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
                    public GitComparisonStatus Status { get; set; } = GitComparisonStatus.Pending;
                    public DateTime? CompletedAt { get; set; }
                    public string? ErrorMessage { get; set; }
                }

                public enum GitComparisonStatus
                {
                    Pending,
                    Processing,
                    Completed,
                    Failed
                }
                """
        });

        project.Files.Add(new FileModel("GitDiffResult", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace GitAnalysis.Core.Entities;

                /// <summary>
                /// Result of Git diff operation.
                /// </summary>
                public class GitDiffResult
                {
                    public Guid RequestId { get; set; }
                    public List<FileDiff> FileDiffs { get; set; } = new();
                    public int TotalAdditions { get; set; }
                    public int TotalDeletions { get; set; }
                    public int TotalModifications { get; set; }
                    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
                }

                public class FileDiff
                {
                    public required string FilePath { get; set; }
                    public FileChangeType ChangeType { get; set; }
                    public List<LineDiff> LineChanges { get; set; } = new();
                    public int Additions { get; set; }
                    public int Deletions { get; set; }
                }

                public class LineDiff
                {
                    public int LineNumber { get; set; }
                    public required string Content { get; set; }
                    public DiffType Type { get; set; }
                }

                public enum FileChangeType
                {
                    Added,
                    Modified,
                    Deleted
                }

                public enum DiffType
                {
                    Addition,
                    Deletion,
                    Context
                }
                """
        });

        project.Files.Add(new FileModel("GitIgnoreRule", entitiesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace GitAnalysis.Core.Entities;

                /// <summary>
                /// Represents a .gitignore rule with its pattern and negation status.
                /// </summary>
                public class GitIgnoreRule
                {
                    public required string Pattern { get; set; }
                    public bool IsNegation { get; set; }
                    public bool IsDirectoryOnly { get; set; }
                    public string? SourceFile { get; set; }
                }
                """
        });

        // Interfaces
        project.Files.Add(new FileModel("IGitService", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using GitAnalysis.Core.Entities;

                namespace GitAnalysis.Core.Interfaces;

                /// <summary>
                /// Service for Git operations including branch operations and diff generation.
                /// </summary>
                public interface IGitService
                {
                    Task<GitDiffResult> GenerateDiffAsync(string repositoryPath, string sourceBranch, string targetBranch, CancellationToken cancellationToken = default);
                    Task<bool> BranchExistsAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default);
                    Task<IEnumerable<string>> GetBranchesAsync(string repositoryPath, CancellationToken cancellationToken = default);
                    Task<IEnumerable<string>> GetFilesInBranchAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default);
                }
                """
        });

        project.Files.Add(new FileModel("IGitIgnoreEngine", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using GitAnalysis.Core.Entities;

                namespace GitAnalysis.Core.Interfaces;

                /// <summary>
                /// Engine for parsing and evaluating .gitignore patterns.
                /// </summary>
                public interface IGitIgnoreEngine
                {
                    IEnumerable<GitIgnoreRule> ParseGitIgnoreFile(string filePath);
                    bool IsIgnored(string filePath, IEnumerable<GitIgnoreRule> rules);
                    IEnumerable<GitIgnoreRule> LoadHierarchicalRules(string repositoryPath, string relativePath);
                }
                """
        });

        project.Files.Add(new FileModel("IComparisonRequestRepository", interfacesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using GitAnalysis.Core.Entities;

                namespace GitAnalysis.Core.Interfaces;

                /// <summary>
                /// Repository for managing Git comparison requests.
                /// </summary>
                public interface IComparisonRequestRepository
                {
                    Task<GitComparisonRequest> CreateAsync(GitComparisonRequest request, CancellationToken cancellationToken = default);
                    Task<GitComparisonRequest?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default);
                    Task<GitComparisonRequest> UpdateAsync(GitComparisonRequest request, CancellationToken cancellationToken = default);
                    Task<IEnumerable<GitComparisonRequest>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
                }
                """
        });

        // Events
        project.Files.Add(new FileModel("ComparisonRequestedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace GitAnalysis.Core.Events;

                public record ComparisonRequestedEvent(Guid RequestId, string RepositoryPath, string SourceBranch, string TargetBranch, string? UserId, DateTime RequestedAt);
                """
        });

        project.Files.Add(new FileModel("ComparisonCompletedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace GitAnalysis.Core.Events;

                public record ComparisonCompletedEvent(Guid RequestId, int TotalAdditions, int TotalDeletions, int TotalModifications, DateTime CompletedAt);
                """
        });

        project.Files.Add(new FileModel("ComparisonFailedEvent", eventsDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace GitAnalysis.Core.Events;

                public record ComparisonFailedEvent(Guid RequestId, string ErrorMessage, DateTime FailedAt);
                """
        });

        // DTOs
        project.Files.Add(new FileModel("ComparisonRequestDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace GitAnalysis.Core.DTOs;

                /// <summary>
                /// DTO for creating a new Git comparison request.
                /// </summary>
                public class ComparisonRequestDto
                {
                    public required string RepositoryPath { get; set; }
                    public required string SourceBranch { get; set; }
                    public required string TargetBranch { get; set; }
                }
                """
        });

        project.Files.Add(new FileModel("ComparisonResultDto", dtosDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                namespace GitAnalysis.Core.DTOs;

                /// <summary>
                /// DTO for Git comparison result.
                /// </summary>
                public class ComparisonResultDto
                {
                    public Guid RequestId { get; set; }
                    public string Status { get; set; } = string.Empty;
                    public List<FileDiffDto> FileDiffs { get; set; } = new();
                    public int TotalAdditions { get; set; }
                    public int TotalDeletions { get; set; }
                    public int TotalModifications { get; set; }
                    public DateTime? CompletedAt { get; set; }
                    public string? ErrorMessage { get; set; }
                }

                public class FileDiffDto
                {
                    public required string FilePath { get; set; }
                    public string ChangeType { get; set; } = string.Empty;
                    public int Additions { get; set; }
                    public int Deletions { get; set; }
                }
                """
        });
    }

    public void AddInfrastructureFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding GitAnalysis.Infrastructure files");

        var servicesDir = Path.Combine(project.Directory, "Services");
        var repositoriesDir = Path.Combine(project.Directory, "Repositories");
        var backgroundServicesDir = Path.Combine(project.Directory, "BackgroundServices");

        // GitService implementation
        project.Files.Add(new FileModel("GitService", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using LibGit2Sharp;
                using GitAnalysis.Core.Entities;
                using GitAnalysis.Core.Interfaces;
                using Microsoft.Extensions.Logging;

                namespace GitAnalysis.Infrastructure.Services;

                /// <summary>
                /// Git service implementation using LibGit2Sharp.
                /// Provides branch isolation, switching, and diff generation capabilities.
                /// </summary>
                public class GitService : IGitService
                {
                    private readonly ILogger<GitService> logger;
                    private readonly IGitIgnoreEngine gitIgnoreEngine;

                    public GitService(ILogger<GitService> logger, IGitIgnoreEngine gitIgnoreEngine)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.gitIgnoreEngine = gitIgnoreEngine ?? throw new ArgumentNullException(nameof(gitIgnoreEngine));
                    }

                    public async Task<GitDiffResult> GenerateDiffAsync(string repositoryPath, string sourceBranch, string targetBranch, CancellationToken cancellationToken = default)
                    {
                        return await Task.Run(() =>
                        {
                            using var repo = new Repository(repositoryPath);
                            
                            var sourceCommit = repo.Branches[sourceBranch]?.Tip 
                                ?? throw new InvalidOperationException($"Branch '{sourceBranch}' not found");
                            var targetCommit = repo.Branches[targetBranch]?.Tip 
                                ?? throw new InvalidOperationException($"Branch '{targetBranch}' not found");

                            var diff = repo.Diff.Compare<Patch>(targetCommit.Tree, sourceCommit.Tree);
                            var result = new GitDiffResult { RequestId = Guid.NewGuid() };

                            var ignoreRules = gitIgnoreEngine.LoadHierarchicalRules(repositoryPath, string.Empty);

                            foreach (var change in diff)
                            {
                                if (gitIgnoreEngine.IsIgnored(change.Path, ignoreRules))
                                    continue;

                                var fileDiff = new FileDiff
                                {
                                    FilePath = change.Path,
                                    ChangeType = MapChangeType(change.Status),
                                    Additions = change.LinesAdded,
                                    Deletions = change.LinesDeleted
                                };

                                foreach (var line in change.Patch.Split('\n').Select((content, index) => new { content, index }))
                                {
                                    if (line.content.StartsWith('+') && !line.content.StartsWith("+++"))
                                    {
                                        fileDiff.LineChanges.Add(new LineDiff
                                        {
                                            LineNumber = line.index,
                                            Content = line.content[1..],
                                            Type = DiffType.Addition
                                        });
                                    }
                                    else if (line.content.StartsWith('-') && !line.content.StartsWith("---"))
                                    {
                                        fileDiff.LineChanges.Add(new LineDiff
                                        {
                                            LineNumber = line.index,
                                            Content = line.content[1..],
                                            Type = DiffType.Deletion
                                        });
                                    }
                                }

                                result.FileDiffs.Add(fileDiff);
                                result.TotalAdditions += fileDiff.Additions;
                                result.TotalDeletions += fileDiff.Deletions;
                                if (fileDiff.ChangeType == FileChangeType.Modified)
                                    result.TotalModifications++;
                            }

                            logger.LogInformation("Generated diff between {Source} and {Target}: {Files} files changed",
                                sourceBranch, targetBranch, result.FileDiffs.Count);

                            return result;
                        }, cancellationToken);
                    }

                    public Task<bool> BranchExistsAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default)
                    {
                        return Task.Run(() =>
                        {
                            using var repo = new Repository(repositoryPath);
                            return repo.Branches[branchName] != null;
                        }, cancellationToken);
                    }

                    public Task<IEnumerable<string>> GetBranchesAsync(string repositoryPath, CancellationToken cancellationToken = default)
                    {
                        return Task.Run(() =>
                        {
                            using var repo = new Repository(repositoryPath);
                            return repo.Branches.Select(b => b.FriendlyName).AsEnumerable();
                        }, cancellationToken);
                    }

                    public Task<IEnumerable<string>> GetFilesInBranchAsync(string repositoryPath, string branchName, CancellationToken cancellationToken = default)
                    {
                        return Task.Run(() =>
                        {
                            using var repo = new Repository(repositoryPath);
                            var branch = repo.Branches[branchName] 
                                ?? throw new InvalidOperationException($"Branch '{branchName}' not found");
                            
                            return branch.Tip.Tree
                                .Select(entry => entry.Path)
                                .AsEnumerable();
                        }, cancellationToken);
                    }

                    private static FileChangeType MapChangeType(ChangeKind changeKind) => changeKind switch
                    {
                        ChangeKind.Added => FileChangeType.Added,
                        ChangeKind.Deleted => FileChangeType.Deleted,
                        ChangeKind.Modified => FileChangeType.Modified,
                        _ => FileChangeType.Modified
                    };
                }
                """
        });

        // GitIgnoreEngine implementation
        project.Files.Add(new FileModel("GitIgnoreEngine", servicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Text.RegularExpressions;
                using GitAnalysis.Core.Entities;
                using GitAnalysis.Core.Interfaces;

                namespace GitAnalysis.Infrastructure.Services;

                /// <summary>
                /// Robust .gitignore parser that mimics Git's internal logic.
                /// Respects hierarchical ignore files and precedence rules.
                /// </summary>
                public class GitIgnoreEngine : IGitIgnoreEngine
                {
                    public IEnumerable<GitIgnoreRule> ParseGitIgnoreFile(string filePath)
                    {
                        if (!File.Exists(filePath))
                            yield break;

                        foreach (var line in File.ReadLines(filePath))
                        {
                            var trimmed = line.Trim();
                            
                            // Skip empty lines and comments
                            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
                                continue;

                            var isNegation = trimmed.StartsWith('!');
                            var pattern = isNegation ? trimmed[1..] : trimmed;
                            var isDirectoryOnly = pattern.EndsWith('/');

                            if (isDirectoryOnly)
                                pattern = pattern[..^1];

                            yield return new GitIgnoreRule
                            {
                                Pattern = pattern,
                                IsNegation = isNegation,
                                IsDirectoryOnly = isDirectoryOnly,
                                SourceFile = filePath
                            };
                        }
                    }

                    public bool IsIgnored(string filePath, IEnumerable<GitIgnoreRule> rules)
                    {
                        var normalizedPath = filePath.Replace('\\', '/');
                        var isIgnored = false;

                        foreach (var rule in rules)
                        {
                            if (MatchesPattern(normalizedPath, rule.Pattern))
                            {
                                isIgnored = !rule.IsNegation;
                            }
                        }

                        return isIgnored;
                    }

                    public IEnumerable<GitIgnoreRule> LoadHierarchicalRules(string repositoryPath, string relativePath)
                    {
                        var rules = new List<GitIgnoreRule>();
                        var currentPath = repositoryPath;
                        var pathParts = string.IsNullOrEmpty(relativePath) 
                            ? Array.Empty<string>() 
                            : relativePath.Split('/', '\\');

                        // Load root .gitignore
                        var rootIgnore = Path.Combine(repositoryPath, ".gitignore");
                        rules.AddRange(ParseGitIgnoreFile(rootIgnore));

                        // Load .gitignore files in subdirectories
                        foreach (var part in pathParts)
                        {
                            currentPath = Path.Combine(currentPath, part);
                            var subIgnore = Path.Combine(currentPath, ".gitignore");
                            rules.AddRange(ParseGitIgnoreFile(subIgnore));
                        }

                        return rules;
                    }

                    private bool MatchesPattern(string path, string pattern)
                    {
                        // Convert gitignore pattern to regex
                        // Handle ** for any directory depth first
                        pattern = pattern.Replace("**/", "|||DOUBLESTAR|||");
                        
                        // Escape the pattern
                        var regexPattern = Regex.Escape(pattern);
                        
                        // Replace placeholders and escaped wildcards
                        regexPattern = regexPattern
                            .Replace("|||DOUBLESTAR|||", "(.*/)?")
                            .Replace("\\*", "[^/]*")
                            .Replace("\\?", "[^/]");
                        
                        regexPattern = "^" + regexPattern + "$";

                        return Regex.IsMatch(path, regexPattern, RegexOptions.IgnoreCase);
                    }
                }
                """
        });

        // Repository implementation
        project.Files.Add(new FileModel("ComparisonRequestRepository", repositoriesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Collections.Concurrent;
                using GitAnalysis.Core.Entities;
                using GitAnalysis.Core.Interfaces;

                namespace GitAnalysis.Infrastructure.Repositories;

                /// <summary>
                /// In-memory repository for Git comparison requests.
                /// </summary>
                public class ComparisonRequestRepository : IComparisonRequestRepository
                {
                    private readonly ConcurrentDictionary<Guid, GitComparisonRequest> requests = new();

                    public Task<GitComparisonRequest> CreateAsync(GitComparisonRequest request, CancellationToken cancellationToken = default)
                    {
                        request.RequestId = Guid.NewGuid();
                        request.RequestedAt = DateTime.UtcNow;
                        request.Status = GitComparisonStatus.Pending;

                        requests.TryAdd(request.RequestId, request);
                        return Task.FromResult(request);
                    }

                    public Task<GitComparisonRequest?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
                    {
                        requests.TryGetValue(requestId, out var request);
                        return Task.FromResult(request);
                    }

                    public Task<GitComparisonRequest> UpdateAsync(GitComparisonRequest request, CancellationToken cancellationToken = default)
                    {
                        requests[request.RequestId] = request;
                        return Task.FromResult(request);
                    }

                    public Task<IEnumerable<GitComparisonRequest>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
                    {
                        var userRequests = requests.Values
                            .Where(r => r.UserId == userId)
                            .OrderByDescending(r => r.RequestedAt)
                            .AsEnumerable();
                        
                        return Task.FromResult(userRequests);
                    }
                }
                """
        });

        // Background service for async processing
        project.Files.Add(new FileModel("ComparisonProcessorService", backgroundServicesDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using System.Threading.Channels;
                using Microsoft.Extensions.Hosting;
                using Microsoft.Extensions.Logging;
                using GitAnalysis.Core.Entities;
                using GitAnalysis.Core.Interfaces;

                namespace GitAnalysis.Infrastructure.BackgroundServices;

                /// <summary>
                /// Background service for asynchronous Git comparison processing.
                /// Prevents blocking the API while performing heavy Git operations.
                /// </summary>
                public class ComparisonProcessorService : BackgroundService
                {
                    private readonly ILogger<ComparisonProcessorService> logger;
                    private readonly IGitService gitService;
                    private readonly IComparisonRequestRepository repository;
                    private readonly Channel<Guid> requestQueue;

                    public ComparisonProcessorService(
                        ILogger<ComparisonProcessorService> logger,
                        IGitService gitService,
                        IComparisonRequestRepository repository)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
                        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
                        this.requestQueue = Channel.CreateUnbounded<Guid>();
                    }

                    public async Task EnqueueRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
                    {
                        await requestQueue.Writer.WriteAsync(requestId, cancellationToken);
                    }

                    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
                    {
                        logger.LogInformation("Comparison Processor Service starting");

                        await foreach (var requestId in requestQueue.Reader.ReadAllAsync(stoppingToken))
                        {
                            try
                            {
                                var request = await repository.GetByIdAsync(requestId, stoppingToken);
                                if (request == null)
                                {
                                    logger.LogWarning("Request {RequestId} not found", requestId);
                                    continue;
                                }

                                logger.LogInformation("Processing comparison request {RequestId}", requestId);
                                request.Status = GitComparisonStatus.Processing;
                                await repository.UpdateAsync(request, stoppingToken);

                                var result = await gitService.GenerateDiffAsync(
                                    request.RepositoryPath,
                                    request.SourceBranch,
                                    request.TargetBranch,
                                    stoppingToken);

                                request.Status = GitComparisonStatus.Completed;
                                request.CompletedAt = DateTime.UtcNow;
                                await repository.UpdateAsync(request, stoppingToken);

                                logger.LogInformation("Completed comparison request {RequestId}", requestId);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Error processing comparison request {RequestId}", requestId);
                                
                                var request = await repository.GetByIdAsync(requestId, stoppingToken);
                                if (request != null)
                                {
                                    request.Status = GitComparisonStatus.Failed;
                                    request.ErrorMessage = ex.Message;
                                    request.CompletedAt = DateTime.UtcNow;
                                    await repository.UpdateAsync(request, stoppingToken);
                                }
                            }
                        }

                        logger.LogInformation("Comparison Processor Service stopped");
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

                using Microsoft.Extensions.Configuration;
                using Microsoft.Extensions.DependencyInjection;
                using GitAnalysis.Core.Interfaces;
                using GitAnalysis.Infrastructure.Services;
                using GitAnalysis.Infrastructure.Repositories;
                using GitAnalysis.Infrastructure.BackgroundServices;

                namespace GitAnalysis.Infrastructure;

                public static class ConfigureServices
                {
                    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
                    {
                        services.AddSingleton<IGitService, GitService>();
                        services.AddSingleton<IGitIgnoreEngine, GitIgnoreEngine>();
                        services.AddSingleton<IComparisonRequestRepository, ComparisonRequestRepository>();
                        services.AddSingleton<ComparisonProcessorService>();
                        services.AddHostedService(sp => sp.GetRequiredService<ComparisonProcessorService>());

                        return services;
                    }
                }
                """
        });
    }

    public void AddApiFiles(ProjectModel project, string microserviceName)
    {
        logger.LogInformation("Adding GitAnalysis.Api files");

        var controllersDir = Path.Combine(project.Directory, "Controllers");

        // Comparison controller
        project.Files.Add(new FileModel("ComparisonController", controllersDir, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using Microsoft.AspNetCore.Mvc;
                using GitAnalysis.Core.DTOs;
                using GitAnalysis.Core.Entities;
                using GitAnalysis.Core.Interfaces;
                using GitAnalysis.Infrastructure.BackgroundServices;

                namespace GitAnalysis.Api.Controllers;

                [ApiController]
                [Route("api/[controller]")]
                public class ComparisonController : ControllerBase
                {
                    private readonly ILogger<ComparisonController> logger;
                    private readonly IComparisonRequestRepository repository;
                    private readonly ComparisonProcessorService processorService;
                    private readonly IGitService gitService;

                    public ComparisonController(
                        ILogger<ComparisonController> logger,
                        IComparisonRequestRepository repository,
                        ComparisonProcessorService processorService,
                        IGitService gitService)
                    {
                        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
                        this.processorService = processorService ?? throw new ArgumentNullException(nameof(processorService));
                        this.gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
                    }

                    /// <summary>
                    /// Request a new Git comparison. Returns immediately with Accepted status.
                    /// </summary>
                    [HttpPost]
                    public async Task<ActionResult<ComparisonResultDto>> RequestComparison(
                        [FromBody] ComparisonRequestDto dto,
                        CancellationToken cancellationToken)
                    {
                        logger.LogInformation("Received comparison request for {Repo} between {Source} and {Target}",
                            dto.RepositoryPath, dto.SourceBranch, dto.TargetBranch);

                        var request = new GitComparisonRequest
                        {
                            RepositoryPath = dto.RepositoryPath,
                            SourceBranch = dto.SourceBranch,
                            TargetBranch = dto.TargetBranch,
                            UserId = User?.Identity?.Name
                        };

                        request = await repository.CreateAsync(request, cancellationToken);
                        await processorService.EnqueueRequestAsync(request.RequestId, cancellationToken);

                        var result = new ComparisonResultDto
                        {
                            RequestId = request.RequestId,
                            Status = request.Status.ToString()
                        };

                        return Accepted($"/api/comparison/{request.RequestId}", result);
                    }

                    /// <summary>
                    /// Get the status and result of a comparison request.
                    /// </summary>
                    [HttpGet("{requestId}")]
                    public async Task<ActionResult<ComparisonResultDto>> GetComparison(
                        Guid requestId,
                        CancellationToken cancellationToken)
                    {
                        var request = await repository.GetByIdAsync(requestId, cancellationToken);
                        if (request == null)
                            return NotFound();

                        var result = new ComparisonResultDto
                        {
                            RequestId = request.RequestId,
                            Status = request.Status.ToString(),
                            CompletedAt = request.CompletedAt,
                            ErrorMessage = request.ErrorMessage
                        };

                        return Ok(result);
                    }

                    /// <summary>
                    /// Get all branches in a repository.
                    /// </summary>
                    [HttpGet("branches")]
                    public async Task<ActionResult<IEnumerable<string>>> GetBranches(
                        [FromQuery] string repositoryPath,
                        CancellationToken cancellationToken)
                    {
                        var branches = await gitService.GetBranchesAsync(repositoryPath, cancellationToken);
                        return Ok(branches);
                    }
                }
                """
        });

        // Program.cs
        project.Files.Add(new FileModel("Program", project.Directory, CSharp)
        {
            Body = """
                // Copyright (c) Quinntyne Brown. All Rights Reserved.
                // Licensed under the MIT License. See License.txt in the project root for license information.

                using GitAnalysis.Infrastructure;

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddInfrastructureServices(builder.Configuration);

                var app = builder.Build();

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

        // appsettings.json
        project.Files.Add(new FileModel("appsettings", project.Directory, ".json")
        {
            Body = """
                {
                  "Logging": {
                    "LogLevel": {
                      "Default": "Information",
                      "Microsoft.AspNetCore": "Warning"
                    }
                  },
                  "AllowedHosts": "*"
                }
                """
        });
    }
}
