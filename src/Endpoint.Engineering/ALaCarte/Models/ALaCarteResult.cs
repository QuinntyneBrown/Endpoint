// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.ALaCarte.Models;

/// <summary>
/// Result of the ALaCarte operation.
/// </summary>
public class ALaCarteResult
{
    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The output directory where files were copied to.
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// The path to the created solution file (if applicable).
    /// </summary>
    public string? SolutionPath { get; set; }

    /// <summary>
    /// List of .csproj files that were discovered and processed.
    /// </summary>
    public List<string> CsprojFiles { get; set; } = new();

    /// <summary>
    /// List of Angular workspaces created for orphan projects.
    /// </summary>
    public List<string> AngularWorkspacesCreated { get; set; } = new();

    /// <summary>
    /// List of error messages if any operations failed.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// List of warning messages.
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}
