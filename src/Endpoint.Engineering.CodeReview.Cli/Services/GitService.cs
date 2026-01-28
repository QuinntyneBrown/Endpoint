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
    private readonly ILogger<GitService> _logger;

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetDiffAsync(string repositoryUrl, string branchName, CancellationToken cancellationToken = default)
    {
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

            return await Task.FromResult(diff.Content);
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
}
