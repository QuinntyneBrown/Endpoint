// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
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

            // Clone the repository
            var cloneOptions = new CloneOptions
            {
                BranchName = null, // Clone all branches
                Checkout = false
            };

            Repository.Clone(repositoryUrl, tempDirectory, cloneOptions);

            using var repo = new Repository(tempDirectory);

            _logger.LogInformation("Repository cloned. Default branch: {DefaultBranch}", repo.Head.FriendlyName);

            // Find the default branch
            var defaultBranch = repo.Head;

            // Find the specified branch
            var targetBranch = repo.Branches.FirstOrDefault(b => 
                b.FriendlyName == branchName || 
                b.FriendlyName == $"origin/{branchName}");

            if (targetBranch == null)
            {
                throw new InvalidOperationException($"Branch '{branchName}' not found in repository.");
            }

            _logger.LogInformation("Comparing {TargetBranch} with {DefaultBranch}", targetBranch.FriendlyName, defaultBranch.FriendlyName);

            // Get the diff between the two branches
            var defaultCommit = defaultBranch.Tip;
            var targetCommit = targetBranch.Tip;

            // Find the merge base
            var mergeBase = repo.ObjectDatabase.FindMergeBase(defaultCommit, targetCommit);

            if (mergeBase == null)
            {
                _logger.LogWarning("No merge base found. Using direct comparison.");
                mergeBase = defaultCommit;
            }

            // Create the diff
            var mergeBaseTree = mergeBase.Tree;
            var targetTree = targetCommit.Tree;

            var diff = repo.Diff.Compare<Patch>(mergeBaseTree, targetTree);

            return diff.Content;
        }
        finally
        {
            // Clean up the temporary directory
            if (Directory.Exists(tempDirectory))
            {
                try
                {
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
