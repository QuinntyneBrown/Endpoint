// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Models;

namespace Endpoint.Engineering.StaticAnalysis;

/// <summary>
/// Service for performing code reviews by comparing git branches and running static analysis.
/// </summary>
public interface ICodeReviewService
{
    /// <summary>
    /// Performs a code review by comparing the current branch against the main branch.
    /// </summary>
    /// <param name="directory">Directory containing a git repository.</param>
    /// <param name="targetBranch">The target branch to compare against (default: "main").</param>
    /// <param name="runStaticAnalysis">Whether to run static analysis on changed files.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The code review result.</returns>
    Task<CodeReviewResult> ReviewAsync(
        string directory,
        string targetBranch = "main",
        bool runStaticAnalysis = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the git repository root from the given directory.
    /// </summary>
    /// <param name="directory">Starting directory.</param>
    /// <returns>The git repository root path, or null if not found.</returns>
    string? FindGitRepositoryRoot(string directory);

    /// <summary>
    /// Gets the current git branch name.
    /// </summary>
    /// <param name="repositoryPath">Path to git repository.</param>
    /// <returns>The current branch name.</returns>
    Task<string> GetCurrentBranchAsync(string repositoryPath);

    /// <summary>
    /// Gets the git diff between the current branch and target branch.
    /// </summary>
    /// <param name="repositoryPath">Path to git repository.</param>
    /// <param name="targetBranch">Target branch to compare against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The git diff result.</returns>
    Task<GitDiffResult> GetDiffAsync(
        string repositoryPath,
        string targetBranch,
        CancellationToken cancellationToken = default);
}
