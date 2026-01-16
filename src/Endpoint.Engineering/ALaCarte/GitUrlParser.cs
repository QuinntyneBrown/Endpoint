// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;

namespace Endpoint.Engineering.ALaCarte;

/// <summary>
/// Utility class for parsing GitHub and GitLab URLs to extract repository information.
/// </summary>
public static class GitUrlParser
{
    /// <summary>
    /// Parses a GitHub or GitLab URL to extract the repository URL, branch, and folder path.
    /// Supports formats:
    /// - GitHub: https://github.com/owner/repo/tree/branch/path/to/folder
    /// - GitLab: https://gitlab.com/owner/repo/-/tree/branch/path/to/folder
    /// </summary>
    /// <param name="url">The full GitHub or GitLab URL to a folder.</param>
    /// <returns>A tuple containing (repositoryUrl, branch, folderPath), or null if parsing fails.</returns>
    /// <remarks>
    /// Note: Branch names with slashes (e.g., feature/branch-name) are supported using a heuristic approach.
    /// The parser recognizes common branch prefixes (feature, bugfix, hotfix, release) followed by a slash.
    /// For other branch naming patterns, the first path segment after /tree/ is assumed to be the branch name,
    /// and subsequent segments are treated as the folder path.
    /// </remarks>
    public static (string RepositoryUrl, string Branch, string FolderPath)? Parse(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        // Normalize URL
        url = url.Trim();

        // GitHub pattern: https://github.com/owner/repo/tree/branch/path/to/folder
        var githubMatch = Regex.Match(url, @"^(https?://github\.com/[^/]+/[^/]+)/tree/(.+)$", RegexOptions.IgnoreCase);
        if (githubMatch.Success)
        {
            var repoUrl = githubMatch.Groups[1].Value;
            var afterTree = githubMatch.Groups[2].Value;
            return ParseBranchAndPath(repoUrl, afterTree);
        }

        // GitLab pattern: https://gitlab.com/owner/repo/-/tree/branch/path/to/folder
        var gitlabMatch = Regex.Match(url, @"^(https?://gitlab\.com/[^/]+/[^/]+)/-/tree/(.+)$", RegexOptions.IgnoreCase);
        if (gitlabMatch.Success)
        {
            var repoUrl = gitlabMatch.Groups[1].Value;
            var afterTree = gitlabMatch.Groups[2].Value;
            return ParseBranchAndPath(repoUrl, afterTree);
        }

        // Support for self-hosted GitLab instances: https://gitlab.example.com/owner/repo/-/tree/branch/path
        var selfHostedGitlabMatch = Regex.Match(url, @"^(https?://[^/]+/[^/]+/[^/]+)/-/tree/(.+)$", RegexOptions.IgnoreCase);
        if (selfHostedGitlabMatch.Success)
        {
            var urlPart = selfHostedGitlabMatch.Groups[1].Value;
            // Only accept if it looks like a GitLab URL (has gitlab in the domain or uses /-/tree/ pattern)
            if (urlPart.Contains("gitlab", StringComparison.OrdinalIgnoreCase))
            {
                var repoUrl = urlPart;
                var afterTree = selfHostedGitlabMatch.Groups[2].Value;
                return ParseBranchAndPath(repoUrl, afterTree);
            }
        }

        // If no pattern matches, return null
        return null;
    }

    /// <summary>
    /// Parses the branch name and folder path from the URL segment after /tree/ or /-/tree/.
    /// </summary>
    /// <param name="repoUrl">The repository URL.</param>
    /// <param name="afterTree">The URL segment after /tree/ or /-/tree/.</param>
    /// <returns>A tuple containing (repositoryUrl, branch, folderPath).</returns>
    private static (string RepositoryUrl, string Branch, string FolderPath) ParseBranchAndPath(string repoUrl, string afterTree)
    {
        var segments = afterTree.Split('/');
        
        if (segments.Length == 1)
        {
            // Only branch, no folder path
            return (repoUrl, segments[0], string.Empty);
        }
        
        // Try to determine where branch ends and path begins
        // Common branch prefixes: feature/, bugfix/, hotfix/, release/
        var branchPrefixes = new[] { "feature", "bugfix", "hotfix", "release" };
        if (branchPrefixes.Contains(segments[0], StringComparer.OrdinalIgnoreCase) && segments.Length >= 2)
        {
            var branch = $"{segments[0]}/{segments[1]}";
            var folderPath = string.Join("/", segments.Skip(2));
            return (repoUrl, branch, folderPath);
        }
        
        // Otherwise, assume first segment is branch, rest is path
        var defaultBranch = segments[0];
        var defaultPath = string.Join("/", segments.Skip(1));
        return (repoUrl, defaultBranch, defaultPath);
    }

    /// <summary>
    /// Checks if a URL is a valid GitHub or GitLab folder URL.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns>True if the URL can be parsed, false otherwise.</returns>
    public static bool IsValidGitUrl(string url)
    {
        return Parse(url) != null;
    }
}
