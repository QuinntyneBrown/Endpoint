// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.SharedLibrary.Configuration;

namespace Endpoint.DotNet.SharedLibrary.Services;

/// <summary>
/// Context passed to project generators.
/// </summary>
public class GeneratorContext
{
    /// <summary>
    /// Gets or sets the shared library configuration.
    /// </summary>
    public SharedLibraryConfig Config { get; set; } = new();

    /// <summary>
    /// Gets or sets the solution directory path.
    /// </summary>
    public string SolutionDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the src directory path.
    /// </summary>
    public string SrcDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the shared directory path.
    /// </summary>
    public string SharedDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the solution name.
    /// </summary>
    public string SolutionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the root namespace.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target framework.
    /// </summary>
    public string TargetFramework { get; set; } = "net9.0";

    /// <summary>
    /// Gets or sets whether this is a preview run (no files created).
    /// </summary>
    public bool IsPreview { get; set; }

    /// <summary>
    /// Gets or sets the library name prefix for generated projects.
    /// Defaults to "Shared" (e.g., Shared.Domain, Shared.Contracts).
    /// </summary>
    public string LibraryName { get; set; } = "Shared";
}
