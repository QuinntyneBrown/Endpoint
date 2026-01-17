// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.ALaCarte.Core.Models;

/// <summary>
/// Request model for the Take operation.
/// Takes a folder from a git/gitlab repository and copies it to a target directory.
/// </summary>
public class ALaCarteTakeRequest
{
    /// <summary>
    /// The URL of the git/gitlab repository.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The branch to clone from.
    /// </summary>
    public string Branch { get; set; } = "main";

    /// <summary>
    /// The path within the repository to the folder to copy.
    /// </summary>
    public string FromPath { get; set; } = string.Empty;

    /// <summary>
    /// The target directory where the folder will be copied to.
    /// Defaults to the current directory if not specified.
    /// </summary>
    public string Directory { get; set; } = Environment.CurrentDirectory;

    /// <summary>
    /// The name of the .NET solution to create/update (optional).
    /// If not specified and a .csproj is found, defaults to the folder name.
    /// </summary>
    public string? SolutionName { get; set; }

    /// <summary>
    /// The root path for Angular projects within the angular.json workspace configuration.
    /// When specified, this value is used as the "root" property in angular.json instead of
    /// deriving it from the destination path. This is useful when the Angular library root
    /// has multiple segments (e.g., "nike/utils" instead of just "utils").
    /// </summary>
    public string? Root { get; set; }

    /// <summary>
    /// The local directory path to copy from instead of cloning from a Git repository.
    /// When specified, the FromPath is treated as a subdirectory within this local directory.
    /// If not specified, the command will clone from the Git repository URL.
    /// </summary>
    public string? FromDirectory { get; set; }
}
