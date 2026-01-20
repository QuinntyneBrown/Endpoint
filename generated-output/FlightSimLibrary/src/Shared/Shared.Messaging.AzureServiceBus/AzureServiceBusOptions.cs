// Auto-generated code
namespace FlightSim.Shared.Messaging.AzureServiceBus;

/// <summary>
/// Options for Azure Service Bus event bus.
/// </summary>
public class AzureServiceBusOptions
{
    /// <summary>
    /// Gets or sets the Azure Service Bus connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default topic name.
    /// </summary>
    public string TopicName { get; set; } = "flightsim-events";

    /// <summary>
    /// Gets or sets the subscription name for this consumer.
    /// </summary>
    public string SubscriptionName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to use sessions.
    /// </summary>
    public bool UseSessions { get; set; } = false;
}
