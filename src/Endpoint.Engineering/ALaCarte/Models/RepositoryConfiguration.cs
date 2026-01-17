// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.ALaCarte.Models;

/// <summary>
/// Configuration for a git repository to clone and extract folders from,
/// or a local directory to copy folders from.
/// </summary>
public class RepositoryConfiguration
{
    /// <summary>
    /// The URL of the git repository (Git or GitLab).
    /// Not required if LocalDirectory is specified.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The branch to clone from.
    /// Only used when Url is specified.
    /// </summary>
    public string Branch { get; set; } = "main";

    /// <summary>
    /// The local directory path to copy from instead of cloning from a Git repository.
    /// When specified, folders will be copied from this local directory instead of cloning from Git.
    /// Either Url or LocalDirectory must be specified, but not both.
    /// </summary>
    public string? LocalDirectory { get; set; }

    /// <summary>
    /// The folder configurations for mapping source to destination paths.
    /// </summary>
    public List<FolderConfiguration> Folders { get; set; } = new();
}
