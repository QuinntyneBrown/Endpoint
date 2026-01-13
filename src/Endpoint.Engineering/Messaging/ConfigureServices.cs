// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Engineering.Messaging.Artifacts;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring Messaging project generator services.
/// </summary>
public static class MessagingConfigureServices
{
    /// <summary>
    /// Adds Messaging project generator services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMessagingProjectGeneratorServices(this IServiceCollection services)
    {
        services.AddSingleton<IMessagingArtifactFactory, MessagingArtifactFactory>();
        services.AddArifactGenerator(typeof(MessagingProjectModel).Assembly);

        return services;
    }
}
