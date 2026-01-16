// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.ALaCarte.Models;

/// <summary>
/// Configuration for mapping a source folder to a destination folder.
/// </summary>
public class FolderConfiguration
{
    /// <summary>
    /// The source path within the repository to copy from.
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// The destination path to copy to (relative to the output directory).
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// The root path for Angular projects within the angular.json workspace configuration.
    /// When specified, this value is used as the "root" property in angular.json instead of
    /// deriving it from the destination path. This is useful when the Angular library root
    /// has multiple segments (e.g., "nike/utils" instead of just "utils").
    /// </summary>
    public string? Root { get; set; }
}
