// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.CodeReview.Cli.Services;

public class GitService : IGitService
{
    private static readonly string[] BranchPrefixes = { "feature", "bugfix", "hotfix", "release", "copilot", "dependabot" };

    private readonly ILogger<GitService> _logger;

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetDiffAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }

        var (repositoryUrl, branchName) = ParseGitUrl(url);

        _logger.LogInformation("Getting diff for repository: {RepositoryUrl}, branch: {BranchName}", repositoryUrl, branchName);

        var tempDirectory = Path.Combine(Path.GetTempPath(), $"code-review-{Guid.NewGuid()}");

        try
        {
            Directory.CreateDirectory(tempDirectory);

            _logger.LogInformation("Cloning repository to: {TempDirectory}", tempDirectory);

            // Clone the repository (fetch all branches)
            await RunGitCommandAsync(tempDirectory, $"clone --no-checkout {repositoryUrl} .", cancellationToken);

            // Fetch all remote branches
            await RunGitCommandAsync(tempDirectory, "fetch --all", cancellationToken);

            // Get the default branch name from remote HEAD
            var defaultBranch = await GetDefaultBranchAsync(tempDirectory, cancellationToken);
            _logger.LogInformation("Repository cloned. Default branch: {DefaultBranch}", defaultBranch);

            // Verify the target branch exists
            var branchExists = await BranchExistsAsync(tempDirectory, branchName, cancellationToken);
            if (!branchExists)
            {
                throw new InvalidOperationException($"Branch '{branchName}' not found in repository.");
            }

            _logger.LogInformation("Comparing {TargetBranch} with {DefaultBranch}", branchName, defaultBranch);

            // Get the diff between the merge base and the target branch
            var diff = await RunGitCommandAsync(
                tempDirectory,
                $"diff origin/{defaultBranch}...origin/{branchName}",
                cancellationToken);

            return diff;
        }
        finally
        {
            // Clean up the temporary directory
            if (Directory.Exists(tempDirectory))
            {
                try
                {
                    // On Windows, git files can be read-only, so we need to clear attributes first
                    ClearReadOnlyAttributes(tempDirectory);
                    Directory.Delete(tempDirectory, recursive: true);
                    _logger.LogInformation("Cleaned up temporary directory: {TempDirectory}", tempDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clean up temporary directory: {TempDirectory}", tempDirectory);
                }
            }
        }
    }

    private async Task<string> GetDefaultBranchAsync(string workingDirectory, CancellationToken cancellationToken)
    {
        try
        {
            // Try to get the default branch from remote HEAD
            var result = await RunGitCommandAsync(workingDirectory, "symbolic-ref refs/remotes/origin/HEAD", cancellationToken);
            var defaultBranch = result.Trim().Replace("refs/remotes/origin/", "");
            if (!string.IsNullOrWhiteSpace(defaultBranch))
            {
                return defaultBranch;
            }
        }
        catch
        {
            // If symbolic-ref fails, fall back to common default branch names
        }

        // Check for common default branch names
        if (await BranchExistsAsync(workingDirectory, "main", cancellationToken))
        {
            return "main";
        }

        if (await BranchExistsAsync(workingDirectory, "master", cancellationToken))
        {
            return "master";
        }

        return "main";
    }

    private async Task<bool> BranchExistsAsync(string workingDirectory, string branchName, CancellationToken cancellationToken)
    {
        try
        {
            await RunGitCommandAsync(workingDirectory, $"rev-parse --verify origin/{branchName}", cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> RunGitCommandAsync(string workingDirectory, string arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
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

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var errorMessage = error.ToString().Trim();
            _logger.LogWarning("Git command failed: git {Arguments}. Error: {Error}", arguments, errorMessage);
            throw new InvalidOperationException($"Git command failed: git {arguments}. Error: {errorMessage}");
        }

        return output.ToString();
    }

    private static void ClearReadOnlyAttributes(string directory)
    {
        foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
        {
            var attributes = File.GetAttributes(file);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);
            }
        }
    }

    private static (string RepositoryUrl, string BranchName) ParseGitUrl(string url)
    {
        var uri = new Uri(url);
        var path = uri.AbsolutePath.TrimEnd('/');

        // GitLab/Self-hosted pattern: /-/tree/{branch}[/{path}]
        var gitlabIndex = path.IndexOf("/-/tree/", StringComparison.Ordinal);
        if (gitlabIndex >= 0)
        {
            var repoPath = path[..gitlabIndex];
            var afterTree = path[(gitlabIndex + 8)..]; // 8 = "/-/tree/".Length

            // For GitLab, branch is first segment (GitLab URL-encodes slashes in branch names)
            var slashIndex = afterTree.IndexOf('/');
            var branchName = slashIndex >= 0 ? afterTree[..slashIndex] : afterTree;

            return ($"{uri.Scheme}://{uri.Host}{repoPath}", branchName);
        }

        // GitHub pattern: /{owner}/{repo}/tree/{branch}[/{path}]
        var treeIndex = path.IndexOf("/tree/", StringComparison.Ordinal);
        if (treeIndex >= 0)
        {
            var repoPath = path[..treeIndex];
            var afterTree = path[(treeIndex + 6)..]; // 6 = "/tree/".Length

            // For GitHub, we need to handle branches with slashes
            // Heuristic: check for common branch prefixes that indicate multi-segment branch names
            var segments = afterTree.Split('/', StringSplitOptions.RemoveEmptyEntries);

            string branchName;
            if (segments.Length == 1)
            {
                branchName = segments[0];
            }
            else if (BranchPrefixes.Contains(segments[0], StringComparer.OrdinalIgnoreCase))
            {
                // First segment is a branch prefix, take first two segments as branch name
                branchName = $"{segments[0]}/{segments[1]}";
            }
            else
            {
                // Assume single-segment branch with path
                branchName = segments[0];
            }

            return ($"{uri.Scheme}://{uri.Host}{repoPath}", branchName);
        }

        // No tree pattern - assume just repository URL, default branch
        return (url.TrimEnd('/'), "main");
    }
}
