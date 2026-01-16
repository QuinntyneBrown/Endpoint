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
}
