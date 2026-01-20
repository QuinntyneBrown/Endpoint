// Auto-generated code
using System.Net;

namespace MCC.Shared.Shared.Messaging.UdpMulticast;

/// <summary>
/// Options for UDP multicast event bus.
/// </summary>
public class UdpMulticastOptions
{
    /// <summary>
    /// Gets or sets the multicast group address.
    /// </summary>
    public string MulticastGroup { get; set; } = "239.0.0.1";

    /// <summary>
    /// Gets or sets the multicast port.
    /// </summary>
    public int Port { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the time-to-live for multicast packets.
    /// </summary>
    public int Ttl { get; set; } = 32;

    /// <summary>
    /// Gets or sets the local interface to bind to.
    /// </summary>
    public string? LocalInterface { get; set; }

    /// <summary>
    /// Gets the multicast group as IPAddress.
    /// </summary>
    public IPAddress MulticastGroupAddress => IPAddress.Parse(MulticastGroup);
}
