// Auto-generated code
using System.Collections.Concurrent;

namespace FlightSim.Shared.Messaging.Ccsds;

/// <summary>
/// Registry for mapping APIDs to packet types.
/// </summary>
public class PacketRegistry
{
    private readonly ConcurrentDictionary<int, Type> _packetTypes = new();

    /// <summary>
    /// Registers a packet type by its APID.
    /// </summary>
    /// <typeparam name="T">The packet type.</typeparam>
    public void Register<T>() where T : CcsdsPacket, new()
    {
        var instance = new T();
        _packetTypes[instance.Apid] = typeof(T);
    }

    /// <summary>
    /// Registers a packet type by APID.
    /// </summary>
    public void Register(int apid, Type packetType)
    {
        if (!typeof(CcsdsPacket).IsAssignableFrom(packetType))
        {
            throw new ArgumentException($"Type {packetType.Name} must derive from CcsdsPacket");
        }

        _packetTypes[apid] = packetType;
    }

    /// <summary>
    /// Gets the packet type for an APID.
    /// </summary>
    public Type? GetPacketType(int apid)
    {
        return _packetTypes.TryGetValue(apid, out var type) ? type : null;
    }

    /// <summary>
    /// Gets all registered APIDs.
    /// </summary>
    public IEnumerable<int> GetRegisteredApids() => _packetTypes.Keys;
}
