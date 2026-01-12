// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Engineering.RedisPubSub.Artifacts;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring RedisPubSub services.
/// </summary>
public static class RedisPubSubConfigureServices
{
    /// <summary>
    /// Adds RedisPubSub messaging services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisPubSubServices(this IServiceCollection services)
    {
        services.AddSingleton<IArtifactFactory, ArtifactFactory>();
        services.AddArifactGenerator(typeof(MessagingProjectModel).Assembly);

        return services;
    }
}
