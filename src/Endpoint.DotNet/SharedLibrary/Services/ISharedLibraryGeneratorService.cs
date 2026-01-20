// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.SharedLibrary.Configuration;

namespace Endpoint.DotNet.SharedLibrary.Services;

/// <summary>
/// Service for generating shared library projects from configuration.
/// </summary>
public interface ISharedLibraryGeneratorService
{
    /// <summary>
    /// Generates all shared library projects based on the configuration.
    /// </summary>
    /// <param name="config">The shared library configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task GenerateAsync(SharedLibraryConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Previews what would be generated without actually creating files.
    /// </summary>
    /// <param name="config">The shared library configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Preview result with list of projects and files.</returns>
    Task<GenerationPreview> PreviewAsync(SharedLibraryConfig config, CancellationToken cancellationToken = default);
}

/// <summary>
/// Preview result showing what would be generated.
/// </summary>
public class GenerationPreview
{
    /// <summary>
    /// Gets or sets the list of projects that would be created.
    /// </summary>
    public List<string> Projects { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of files that would be created.
    /// </summary>
    public List<string> Files { get; set; } = new();
}
