// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.SharedLibrary.Configuration;

/// <summary>
/// Interface for loading shared library configuration from files.
/// </summary>
public interface ISharedLibraryConfigLoader
{
    /// <summary>
    /// Loads configuration from a YAML file.
    /// </summary>
    /// <param name="filePath">Path to the YAML configuration file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded configuration.</returns>
    Task<SharedLibraryConfig> LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads configuration from a YAML string.
    /// </summary>
    /// <param name="yamlContent">YAML content string.</param>
    /// <returns>The loaded configuration.</returns>
    SharedLibraryConfig LoadFromString(string yamlContent);

    /// <summary>
    /// Validates a configuration.
    /// </summary>
    /// <param name="config">Configuration to validate.</param>
    /// <returns>List of validation errors, empty if valid.</returns>
    IReadOnlyList<string> Validate(SharedLibraryConfig config);
}
