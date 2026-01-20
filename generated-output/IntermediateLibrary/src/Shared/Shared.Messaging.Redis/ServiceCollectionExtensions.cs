// Auto-generated code
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Shared.Messaging.Abstractions;

namespace ECommerce.Shared.Messaging.Redis;

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Redis event bus to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRedisEventBus(
        this IServiceCollection services,
        Action<RedisEventBusOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        services.AddSingleton<IMessageSerializer, MessagePackMessageSerializer>();
        services.AddSingleton<IEventBus, RedisEventBus>();

        return services;
    }
}
