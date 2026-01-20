// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.SharedLibrary.Configuration;

namespace Endpoint.DotNet.SharedLibrary.Services.Generators;

/// <summary>
/// Interface for project generators that create specific shared library projects.
/// </summary>
public interface IProjectGenerator
{
    /// <summary>
    /// Gets the order in which this generator should run.
    /// Lower values run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Determines if this generator should run based on the configuration.
    /// </summary>
    /// <param name="config">The shared library configuration.</param>
    /// <returns>True if the generator should run.</returns>
    bool ShouldGenerate(SharedLibraryConfig config);

    /// <summary>
    /// Generates the project files.
    /// </summary>
    /// <param name="context">The generator context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task GenerateAsync(GeneratorContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Previews what would be generated without creating files.
    /// </summary>
    /// <param name="context">The generator context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Preview result.</returns>
    Task<GenerationPreview> PreviewAsync(GeneratorContext context, CancellationToken cancellationToken = default);
}
