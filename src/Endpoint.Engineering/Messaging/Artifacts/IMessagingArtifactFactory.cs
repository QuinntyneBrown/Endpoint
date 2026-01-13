// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.Messaging.Models;

namespace Endpoint.Engineering.Messaging.Artifacts;

/// <summary>
/// Factory interface for creating messaging project artifacts.
/// </summary>
public interface IMessagingArtifactFactory
{
    /// <summary>
    /// Creates a messaging project model from a project definition.
    /// </summary>
    /// <param name="definition">The messaging project definition.</param>
    /// <param name="outputDirectory">The output directory for the project.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation and contains the messaging project model.</returns>
    Task<MessagingProjectModel> CreateMessagingProjectAsync(
        MessagingProjectDefinition definition,
        string outputDirectory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a messaging project model from one or more definition files.
    /// </summary>
    /// <param name="definitionFilePaths">The paths to JSON definition files.</param>
    /// <param name="outputDirectory">The output directory for the project.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation and contains the messaging project model.</returns>
    Task<MessagingProjectModel> CreateMessagingProjectFromFilesAsync(
        IEnumerable<string> definitionFilePaths,
        string outputDirectory,
        CancellationToken cancellationToken = default);
}
