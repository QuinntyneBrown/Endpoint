// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.RedisPubSub.Models;

namespace Endpoint.Engineering.RedisPubSub.Artifacts;

/// <summary>
/// Factory interface for creating messaging artifacts.
/// </summary>
public interface IArtifactFactory
{
    /// <summary>
    /// Creates a messaging project model.
    /// </summary>
    /// <param name="model">The messaging model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation and contains the messaging project model.</returns>
    Task<MessagingProjectModel> CreateMessagingProjectAsync(MessagingModel model, CancellationToken cancellationToken = default);
}
