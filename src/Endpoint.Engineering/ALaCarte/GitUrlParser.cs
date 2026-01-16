// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using System.Web;

namespace Endpoint.Engineering.ALaCarte;

/// <summary>
/// Utility class for parsing Git repository URLs to extract repository information.
/// Supports GitHub, GitLab, Bitbucket, Azure DevOps, Gitea, and private/self-hosted git hosts.
/// </summary>
public static class GitUrlParser
{
    /// <summary>
    /// Parses a Git repository URL to extract the repository URL, branch, and folder path.
    /// Supports formats:
    /// - GitHub: https://github.com/owner/repo/tree/branch/path/to/folder
    /// - GitHub Enterprise: https://github.company.com/owner/repo/tree/branch/path/to/folder
    /// - GitLab: https://gitlab.com/owner/repo/-/tree/branch/path/to/folder
    /// - Self-hosted GitLab: https://git.company.com/owner/repo/-/tree/branch/path/to/folder
    /// - Bitbucket: https://bitbucket.org/owner/repo/src/branch/path/to/folder
    /// - Self-hosted Bitbucket: https://bitbucket.company.com/projects/PROJECT/repos/repo/browse/path?at=refs/heads/branch
    /// - Azure DevOps: https://dev.azure.com/org/project/_git/repo?path=/path&amp;version=GBbranch
    /// - Gitea/Gogs: https://gitea.company.com/owner/repo/src/branch/branch-name/path/to/folder
    /// </summary>
    /// <param name="url">The full Git URL to a folder.</param>
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

        // Try each parser in order of specificity
        return ParseGitHub(url)
            ?? ParseGitLab(url)
            ?? ParseBitbucketCloud(url)
            ?? ParseBitbucketServer(url)
            ?? ParseAzureDevOps(url)
            ?? ParseGiteaGogs(url)
            ?? ParseSelfHostedGitLab(url)
            ?? ParseSelfHostedGitHub(url);
    }

    /// <summary>
    /// Parses GitHub.com URLs.
    /// Pattern: https://github.com/owner/repo/tree/branch/path/to/folder
    /// </summary>
    private static (string RepositoryUrl, string Branch, string FolderPath)? ParseGitHub(string url)
    {
        var match = Regex.Match(url, @"^(https?://github\.com/[^/]+/[^/]+)/tree/(.+)$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var repoUrl = match.Groups[1].Value;
            var afterTree = match.Groups[2].Value;
            return ParseBranchAndPath(repoUrl, afterTree);
        }

        return null;
    }

    /// <summary>
    /// Parses GitLab.com URLs.
    /// Pattern: https://gitlab.com/owner/repo/-/tree/branch/path/to/folder
    /// Also supports: https://gitlab.com/owner/subowner/repo/-/tree/branch/path/to/folder
    /// </summary>
    private static (string RepositoryUrl, string Branch, string FolderPath)? ParseGitLab(string url)
    {
        // Match any number of path segments before /-/tree/
        var match = Regex.Match(url, @"^(https?://gitlab\.com/.+?)/-/tree/(.+)$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var repoUrl = match.Groups[1].Value;
            var afterTree = match.Groups[2].Value;
            return ParseBranchAndPath(repoUrl, afterTree);
        }

        return null;
    }

    /// <summary>
    /// Parses Bitbucket Cloud URLs.
    /// Pattern: https://bitbucket.org/owner/repo/src/branch/path/to/folder
    /// </summary>
    private static (string RepositoryUrl, string Branch, string FolderPath)? ParseBitbucketCloud(string url)
    {
        var match = Regex.Match(url, @"^(https?://bitbucket\.org/[^/]+/[^/]+)/src/(.+)$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var repoUrl = match.Groups[1].Value;
            var afterSrc = match.Groups[2].Value;
            return ParseBranchAndPath(repoUrl, afterSrc);
        }

        return null;
    }

    /// <summary>
    /// Parses Bitbucket Server (self-hosted) URLs.
    /// Pattern: https://bitbucket.company.com/projects/PROJECT/repos/repo/browse/path?at=refs/heads/branch
    /// Alternative: https://bitbucket.company.com/scm/PROJECT/repo.git (clone URL)
    /// </summary>
    private static (string RepositoryUrl, string Branch, string FolderPath)? ParseBitbucketServer(string url)
    {
        // Pattern for browse URLs with query string
        var match = Regex.Match(url, @"^(https?://[^/]+/projects/[^/]+/repos/[^/]+)/browse(?:/(.*))?(?:\?|$)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var repoUrl = match.Groups[1].Value;
            var folderPath = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;

            // Extract branch from query string if present
            var branch = "main";
            if (url.Contains('?'))
            {
                var queryString = url.Substring(url.IndexOf('?') + 1);
                var queryParams = HttpUtility.ParseQueryString(queryString);
                var atParam = queryParams["at"];
                if (!string.IsNullOrEmpty(atParam))
                {
                    // at=refs/heads/branch-name or at=branch-name
                    branch = atParam.Replace("refs/heads/", string.Empty);
                }
            }

            return (repoUrl, branch, folderPath);
        }

        return null;
    }

    /// <summary>
    /// Parses Azure DevOps URLs.
    /// Pattern: https://dev.azure.com/org/project/_git/repo?path=/path&amp;version=GBbranch
    /// Also supports: https://org.visualstudio.com/project/_git/repo?path=/path&amp;version=GBbranch
    /// </summary>
    private static (string RepositoryUrl, string Branch, string FolderPath)? ParseAzureDevOps(string url)
    {
        // Azure DevOps pattern: https://dev.azure.com/org/project/_git/repo
        var devAzureMatch = Regex.Match(url, @"^(https?://dev\.azure\.com/[^/]+/[^/]+/_git/[^/?]+)", RegexOptions.IgnoreCase);

        // Legacy VSTS pattern: https://org.visualstudio.com/project/_git/repo
        var vstsMatch = Regex.Match(url, @"^(https?://[^/]+\.visualstudio\.com/[^/]+/_git/[^/?]+)", RegexOptions.IgnoreCase);

        var match = devAzureMatch.Success ? devAzureMatch : (vstsMatch.Success ? vstsMatch : null);
        if (match == null)
        {
            return null;
        }

        var repoUrl = match.Groups[1].Value;
        var branch = "main";
        var folderPath = string.Empty;

        // Extract path and version from query string
        if (url.Contains('?'))
        {
            var queryString = url.Substring(url.IndexOf('?') + 1);
            var queryParams = HttpUtility.ParseQueryString(queryString);

            var pathParam = queryParams["path"];
            if (!string.IsNullOrEmpty(pathParam))
            {
                folderPath = pathParam.TrimStart('/');
            }

            var versionParam = queryParams["version"];
            if (!string.IsNullOrEmpty(versionParam))
            {
                // version=GBbranch-name (GB prefix for branch)
                if (versionParam.StartsWith("GB", StringComparison.OrdinalIgnoreCase))
                {
                    branch = versionParam.Substring(2);
                }
                else
                {
                    branch = versionParam;
                }
            }
        }

        return (repoUrl, branch, folderPath);
    }

    /// <summary>
    /// Parses Gitea and Gogs URLs.
    /// Pattern: https://gitea.company.com/owner/repo/src/branch/branch-name/path/to/folder
    /// </summary>
    private static (string RepositoryUrl, string Branch, string FolderPath)? ParseGiteaGogs(string url)
    {
        // Gitea/Gogs pattern: /src/branch/branch-name/path
        var match = Regex.Match(url, @"^(https?://[^/]+/[^/]+/[^/]+)/src/branch/(.+)$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var repoUrl = match.Groups[1].Value;
            var afterBranch = match.Groups[2].Value;

            // Don't match if it looks like GitHub, GitLab, or Bitbucket
            if (IsKnownGitHost(repoUrl))
            {
                return null;
            }

            return ParseBranchAndPath(repoUrl, afterBranch);
        }

        return null;
    }

    /// <summary>
    /// Parses self-hosted GitLab instances.
    /// Pattern: https://git.company.com/owner/repo/-/tree/branch/path
    /// Also supports: https://git.company.com/owner/subowner/repo/-/tree/branch/path
    /// The /-/tree/ pattern is unique to GitLab.
    /// </summary>
    private static (string RepositoryUrl, string Branch, string FolderPath)? ParseSelfHostedGitLab(string url)
    {
        // Match any number of path segments before /-/tree/
        var match = Regex.Match(url, @"^(https?://[^/]+/.+?)/-/tree/(.+)$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var repoUrl = match.Groups[1].Value;
            var afterTree = match.Groups[2].Value;

            // Don't match if it looks like public GitLab (already handled)
            if (repoUrl.Contains("gitlab.com", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return ParseBranchAndPath(repoUrl, afterTree);
        }

        return null;
    }

    /// <summary>
    /// Parses self-hosted GitHub Enterprise and generic git hosts using /tree/ pattern.
    /// Pattern: https://github.company.com/owner/repo/tree/branch/path
    /// </summary>
    private static (string RepositoryUrl, string Branch, string FolderPath)? ParseSelfHostedGitHub(string url)
    {
        var match = Regex.Match(url, @"^(https?://[^/]+/[^/]+/[^/]+)/tree/(.+)$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var repoUrl = match.Groups[1].Value;
            var afterTree = match.Groups[2].Value;

            // Don't match if it's public GitHub (already handled) or other known hosts
            if (IsKnownGitHost(repoUrl))
            {
                return null;
            }

            return ParseBranchAndPath(repoUrl, afterTree);
        }

        return null;
    }

    /// <summary>
    /// Checks if the URL is a known public git hosting service.
    /// </summary>
    private static bool IsKnownGitHost(string repoUrl)
    {
        var knownHosts = new[] { "github.com", "gitlab.com", "bitbucket.org", "dev.azure.com" };
        return knownHosts.Any(host => repoUrl.Contains(host, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Parses the branch name and folder path from the URL segment after /tree/ or /-/tree/.
    /// </summary>
    /// <param name="repoUrl">The repository URL.</param>
    /// <param name="afterTree">The URL segment after /tree/ or /-/tree/.</param>
    /// <returns>A tuple containing (repositoryUrl, branch, folderPath).</returns>
    private static (string RepositoryUrl, string Branch, string FolderPath) ParseBranchAndPath(string repoUrl, string afterTree)
    {
        // Strip query string if present (e.g., ?ref_type=heads from GitLab URLs)
        var queryIndex = afterTree.IndexOf('?');
        if (queryIndex >= 0)
        {
            afterTree = afterTree.Substring(0, queryIndex);
        }

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
    /// Checks if a URL is a valid Git folder URL.
    /// Supports GitHub, GitLab, Bitbucket, Azure DevOps, Gitea, and private/self-hosted git hosts.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <returns>True if the URL can be parsed, false otherwise.</returns>
    public static bool IsValidGitUrl(string url)
    {
        return Parse(url) != null;
    }
}
