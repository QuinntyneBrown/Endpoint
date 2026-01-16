// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.ALaCarte.Models;

/// <summary>
/// Result of the Take operation.
/// </summary>
public class ALaCarteTakeResult
{
    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The output directory where the folder was copied to.
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// The path to the folder that was copied.
    /// </summary>
    public string CopiedFolderPath { get; set; } = string.Empty;

    /// <summary>
    /// The path to the created/updated solution file (if applicable).
    /// </summary>
    public string? SolutionPath { get; set; }

    /// <summary>
    /// The path to the created/updated Angular workspace (if applicable).
    /// </summary>
    public string? AngularWorkspacePath { get; set; }

    /// <summary>
    /// List of .csproj files that were discovered in the folder.
    /// </summary>
    public List<string> CsprojFiles { get; set; } = new();

    /// <summary>
    /// Indicates whether the folder is a .NET project (contains .csproj).
    /// </summary>
    public bool IsDotNetProject { get; set; }

    /// <summary>
    /// Indicates whether the folder is an Angular project (contains angular.json or ng-package.json).
    /// </summary>
    public bool IsAngularProject { get; set; }

    /// <summary>
    /// List of error messages if any operations failed.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// List of warning messages.
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}
