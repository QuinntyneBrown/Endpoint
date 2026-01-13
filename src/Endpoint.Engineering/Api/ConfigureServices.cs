// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Engineering.Api;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring API Gateway services.
/// </summary>
public static class ApiConfigureServices
{
    /// <summary>
    /// Adds API Gateway services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApiGatewayServices(this IServiceCollection services)
    {
        services.AddSingleton<IApiArtifactFactory, ApiArtifactFactory>();
        services.AddArifactGenerator(typeof(ApiGatewayModel).Assembly);

        return services;
    }
}
