// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.Api.Models;

namespace Endpoint.Engineering.Api;

/// <summary>
/// Factory interface for creating API Gateway artifacts.
/// </summary>
public interface IApiArtifactFactory
{
    /// <summary>
    /// Creates an API Gateway project model.
    /// </summary>
    /// <param name="model">The API Gateway input model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation and contains the API Gateway project model.</returns>
    Task<ApiGatewayModel> CreateApiGatewayProjectAsync(ApiGatewayInputModel model, CancellationToken cancellationToken = default);
}

