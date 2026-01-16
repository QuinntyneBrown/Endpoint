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
    public static (string RepositoryUrl, string Branch, string FolderPath)? Parse(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        // Normalize URL
        url = url.Trim();

        // GitHub pattern: https://github.com/owner/repo/tree/branch/path/to/folder
        // The branch can contain slashes, so we need to be more careful with parsing
        var githubMatch = Regex.Match(url, @"^(https?://github\.com/[^/]+/[^/]+)/tree/(.+)$", RegexOptions.IgnoreCase);
        if (githubMatch.Success)
        {
            var repoUrl = githubMatch.Groups[1].Value;
            var afterTree = githubMatch.Groups[2].Value;
            
            // Split the afterTree part - the first segment is the branch, rest is folder path
            // However, we need to handle cases where branch contains slashes
            // We'll try to parse conservatively: if there's a slash after the branch name, the rest is folder
            // Since we can't definitively know where branch ends and path begins without querying the repo,
            // we'll use a heuristic: look for common branch patterns
            
            // For simplicity, we'll assume the branch name continues until we find a path segment
            // that looks like a folder (common patterns: src, lib, packages, etc.)
            // Or if there's only one segment, it's the branch
            
            var segments = afterTree.Split('/');
            if (segments.Length == 1)
            {
                // Only branch, no folder path
                return (repoUrl, segments[0], string.Empty);
            }
            
            // Try to determine where branch ends and path begins
            // Common branch prefixes: feature/, bugfix/, hotfix/, release/
            // For now, we'll use a simple heuristic: if first segment is a branch prefix, take first 2 segments as branch
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

        // GitLab pattern: https://gitlab.com/owner/repo/-/tree/branch/path/to/folder
        var gitlabMatch = Regex.Match(url, @"^(https?://gitlab\.com/[^/]+/[^/]+)/-/tree/(.+)$", RegexOptions.IgnoreCase);
        if (gitlabMatch.Success)
        {
            var repoUrl = gitlabMatch.Groups[1].Value;
            var afterTree = gitlabMatch.Groups[2].Value;
            
            var segments = afterTree.Split('/');
            if (segments.Length == 1)
            {
                return (repoUrl, segments[0], string.Empty);
            }
            
            // Apply same heuristic as GitHub
            var branchPrefixes = new[] { "feature", "bugfix", "hotfix", "release" };
            if (branchPrefixes.Contains(segments[0], StringComparer.OrdinalIgnoreCase) && segments.Length >= 2)
            {
                var branch = $"{segments[0]}/{segments[1]}";
                var folderPath = string.Join("/", segments.Skip(2));
                return (repoUrl, branch, folderPath);
            }
            
            var defaultBranch = segments[0];
            var defaultPath = string.Join("/", segments.Skip(1));
            return (repoUrl, defaultBranch, defaultPath);
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
                
                var segments = afterTree.Split('/');
                if (segments.Length == 1)
                {
                    return (repoUrl, segments[0], string.Empty);
                }
                
                // Apply same heuristic
                var branchPrefixes = new[] { "feature", "bugfix", "hotfix", "release" };
                if (branchPrefixes.Contains(segments[0], StringComparer.OrdinalIgnoreCase) && segments.Length >= 2)
                {
                    var branch = $"{segments[0]}/{segments[1]}";
                    var folderPath = string.Join("/", segments.Skip(2));
                    return (repoUrl, branch, folderPath);
                }
                
                var defaultBranch = segments[0];
                var defaultPath = string.Join("/", segments.Skip(1));
                return (repoUrl, defaultBranch, defaultPath);
            }
        }

        // If no pattern matches, return null
        return null;
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
