// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.CyclicRandomizr;

/// <summary>
/// Service for generating cyclic randomizer classes for .NET types.
/// </summary>
public interface ICyclicRandomizrService
{
    /// <summary>
    /// Generates a randomizer class for the specified type that produces random instances
    /// cyclically with configurable frequency and property exclusions.
    /// </summary>
    /// <param name="fullyQualifiedTypeName">The fully qualified name of the .NET type (e.g., "MyNamespace.MyClass").</param>
    /// <param name="directory">The output directory for the generated file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task GenerateRandomizerAsync(string fullyQualifiedTypeName, string directory, CancellationToken cancellationToken = default);
}
