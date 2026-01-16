// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Models;

namespace Endpoint.Engineering.ALaCarte;

/// <summary>
/// Service for cloning git repositories, extracting select folders,
/// and creating a new folder structure based on mapping configuration.
/// </summary>
public interface IALaCarteService
{
    /// <summary>
    /// Processes the ALaCarte request by cloning repositories, extracting folders,
    /// and optionally creating .NET solutions and Angular workspaces.
    /// </summary>
    /// <param name="request">The ALaCarte request containing repository and folder configurations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the ALaCarte operation.</returns>
    Task<ALaCarteResult> ProcessAsync(ALaCarteRequest request, CancellationToken cancellationToken = default);
}
