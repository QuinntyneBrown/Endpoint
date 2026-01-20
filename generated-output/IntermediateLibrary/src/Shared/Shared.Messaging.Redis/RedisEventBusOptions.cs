// Auto-generated code
namespace ECommerce.Shared.Messaging.Redis;

/// <summary>
/// Options for Redis event bus.
/// </summary>
public class RedisEventBusOptions
{
    /// <summary>
    /// Gets or sets the Redis connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Gets or sets the channel prefix.
    /// </summary>
    public string ChannelPrefix { get; set; } = "ecommerce";
}
