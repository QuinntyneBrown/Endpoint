// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.ALaCarte.Models;

/// <summary>
/// Configuration for a git repository to clone and extract folders from.
/// </summary>
public class RepositoryConfiguration
{
    /// <summary>
    /// The URL of the git repository (Git or GitLab).
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The branch to clone from.
    /// </summary>
    public string Branch { get; set; } = "main";

    /// <summary>
    /// The folder configurations for mapping source to destination paths.
    /// </summary>
    public List<FolderConfiguration> Folders { get; set; } = new();
}
