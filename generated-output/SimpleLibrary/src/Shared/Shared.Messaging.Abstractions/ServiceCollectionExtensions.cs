// Auto-generated code
using Microsoft.Extensions.DependencyInjection;

namespace Simple.Shared.Messaging.Abstractions;

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds messaging abstractions to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMessagingAbstractions(this IServiceCollection services)
    {
        // Base abstractions don't register anything by default
        // This is here for consistency and future extensibility
        return services;
    }
}
