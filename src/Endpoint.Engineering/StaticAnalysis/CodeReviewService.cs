// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Text;
using Endpoint.Engineering.StaticAnalysis.Models;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.StaticAnalysis;

/// <summary>
/// Service for performing code reviews by comparing git branches and running static analysis.
/// </summary>
public class CodeReviewService : ICodeReviewService
{
    private readonly ILogger<CodeReviewService> _logger;
    private readonly IStaticAnalysisService _staticAnalysisService;

    public CodeReviewService(
        ILogger<CodeReviewService> logger,
        IStaticAnalysisService staticAnalysisService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _staticAnalysisService = staticAnalysisService ?? throw new ArgumentNullException(nameof(staticAnalysisService));
    }

    /// <inheritdoc/>
    public async Task<CodeReviewResult> ReviewAsync(
        string directory,
        string targetBranch = "main",
        bool runStaticAnalysis = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            directory = Environment.CurrentDirectory;
        }

        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        var repositoryRoot = FindGitRepositoryRoot(directory);
        if (repositoryRoot == null)
        {
            throw new InvalidOperationException($"No git repository found at or above: {directory}");
        }

        _logger.LogInformation("Performing code review for repository at: {RepositoryRoot}", repositoryRoot);

        var result = new CodeReviewResult
        {
            RepositoryRoot = repositoryRoot
        };

        // Get git diff
        result.GitDiff = await GetDiffAsync(repositoryRoot, targetBranch, cancellationToken);

        _logger.LogInformation(
            "Comparing {CurrentBranch} against {TargetBranch}. Changed files: {FileCount}",
            result.GitDiff.CurrentBranch,
            result.GitDiff.TargetBranch,
            result.GitDiff.ChangedFiles.Count);

        // Run static analysis on changed files if requested
        if (runStaticAnalysis && result.GitDiff.HasChanges)
        {
            _logger.LogInformation("Running static analysis on changed files...");
            result.AnalysisResult = await _staticAnalysisService.AnalyzeAsync(repositoryRoot, cancellationToken);
        }

        return result;
    }

    /// <inheritdoc/>
    public string? FindGitRepositoryRoot(string directory)
    {
        var current = new DirectoryInfo(directory);

        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, ".git")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<string> GetCurrentBranchAsync(string repositoryPath)
    {
        var result = await RunGitCommandAsync(repositoryPath, "rev-parse --abbrev-ref HEAD");
        return result.Trim();
    }

    /// <inheritdoc/>
    public async Task<GitDiffResult> GetDiffAsync(
        string repositoryPath,
        string targetBranch,
        CancellationToken cancellationToken = default)
    {
        var result = new GitDiffResult
        {
            TargetBranch = targetBranch
        };

        // Get current branch
        result.CurrentBranch = await GetCurrentBranchAsync(repositoryPath);

        // Check if target branch exists
        var branchExists = await CheckBranchExistsAsync(repositoryPath, targetBranch);
        if (!branchExists)
        {
            _logger.LogWarning("Target branch '{TargetBranch}' does not exist in repository", targetBranch);
            return result;
        }

        // Get list of changed files
        var changedFilesOutput = await RunGitCommandAsync(
            repositoryPath,
            $"diff --name-only {targetBranch}...HEAD");

        if (!string.IsNullOrWhiteSpace(changedFilesOutput))
        {
            result.ChangedFiles = changedFilesOutput
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .ToList();
        }

        // Get full diff
        result.RawDiff = await RunGitCommandAsync(
            repositoryPath,
            $"diff {targetBranch}...HEAD");

        return result;
    }

    private async Task<bool> CheckBranchExistsAsync(string repositoryPath, string branchName)
    {
        try
        {
            var output = await RunGitCommandAsync(repositoryPath, $"rev-parse --verify {branchName}");
            return !string.IsNullOrWhiteSpace(output);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogDebug("Branch '{BranchName}' does not exist: {Message}", branchName, ex.Message);
            return false;
        }
    }

    private async Task<string> RunGitCommandAsync(string repositoryPath, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = repositoryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                error.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var errorMessage = error.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                _logger.LogWarning(
                    "Git command failed: git {Arguments} in {WorkingDirectory}. Error: {Error}",
                    arguments,
                    repositoryPath,
                    errorMessage);
            }

            throw new InvalidOperationException(
                $"Git command failed: git {arguments}. Error: {errorMessage}. " +
                $"Working directory: {repositoryPath}. " +
                "Ensure git is installed and the directory is a valid git repository.");
        }

        return output.ToString();
    }
}
