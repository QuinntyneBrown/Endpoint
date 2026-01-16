// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.SolutionPruning;

/// <summary>
/// Service for pruning a .NET solution to only include a specified type and its dependencies/references.
/// </summary>
public interface ISolutionPruneService
{
    /// <summary>
    /// Prunes a .NET solution to only include the specified type and its transitive dependencies and references.
    /// </summary>
    /// <param name="solutionPath">Path to the .NET solution file (.sln).</param>
    /// <param name="fullyQualifiedTypeName">The fully qualified name of the type to prune around (e.g., "MyNamespace.MyClass").</param>
    /// <param name="outputDirectory">Optional output directory for the pruned solution. If not specified, creates a subfolder in the solution directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the pruning operation.</returns>
    Task<SolutionPruneResult> PruneAsync(
        string solutionPath,
        string fullyQualifiedTypeName,
        string? outputDirectory = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a solution pruning operation.
/// </summary>
public class SolutionPruneResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the path to the pruned solution file.
    /// </summary>
    public string? PrunedSolutionPath { get; set; }

    /// <summary>
    /// Gets or sets the fully qualified name of the target type.
    /// </summary>
    public string? TargetTypeName { get; set; }

    /// <summary>
    /// Gets or sets the list of types included in the pruned solution.
    /// </summary>
    public List<string> IncludedTypes { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of files included in the pruned solution.
    /// </summary>
    public List<string> IncludedFiles { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of projects included in the pruned solution.
    /// </summary>
    public List<string> IncludedProjects { get; set; } = new();

    /// <summary>
    /// Gets or sets the count of types that depend on the target type.
    /// </summary>
    public int DependentTypeCount { get; set; }

    /// <summary>
    /// Gets or sets the count of types that the target type depends on.
    /// </summary>
    public int DependencyTypeCount { get; set; }
}
