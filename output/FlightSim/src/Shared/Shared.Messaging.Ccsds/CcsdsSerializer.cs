// Auto-generated code
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Messaging.Ccsds;

/// <summary>
/// Serializer for CCSDS space packets.
/// </summary>
public class CcsdsSerializer : IMessageSerializer
{
    private readonly PacketRegistry _registry;

    public CcsdsSerializer(PacketRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public byte[] Serialize<T>(T value)
    {
        if (value is not CcsdsPacket packet)
        {
            throw new ArgumentException($"Type {typeof(T).Name} is not a CcsdsPacket");
        }

        var packer = new BitPacker();

        // Pack data to get its length
        var dataPacker = new BitPacker();
        packet.PackData(dataPacker);
        var dataBytes = dataPacker.GetBytes();

        // Create and pack header
        var header = new CcsdsPrimaryHeader
        {
            Version = 0,
            PacketType = packet.PacketType,
            SecondaryHeaderFlag = packet.Timestamp.HasValue,
            Apid = packet.Apid,
            SequenceFlags = 3, // Unsegmented
            SequenceCount = packet.SequenceCount & 0x3FFF,
            PacketDataLength = dataBytes.Length - 1 + (packet.Timestamp.HasValue ? 6 : 0),
        };

        header.Pack(packer);

        // Pack secondary header if present (CUC time: 4 bytes coarse + 2 bytes fine)
        if (packet.Timestamp.HasValue)
        {
            var epoch = new DateTimeOffset(1958, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var totalSeconds = (packet.Timestamp.Value - epoch).TotalSeconds;
            var coarse = (uint)totalSeconds;
            var fine = (ushort)((totalSeconds - coarse) * 65536);

            packer.PackUInt32(coarse);
            packer.PackUInt16(fine);
        }

        // Pack data
        packer.PackBytes(dataBytes);

        return packer.GetBytes();
    }

    public T? Deserialize<T>(byte[] data)
    {
        var result = Deserialize(data, typeof(T));
        return result is T typed ? typed : default;
    }

    public object? Deserialize(byte[] data, Type type)
    {
        var unpacker = new BitUnpacker(data);

        // Unpack header
        var header = CcsdsPrimaryHeader.Unpack(unpacker);

        // Get packet type from registry
        var packetType = _registry.GetPacketType(header.Apid);
        if (packetType == null)
        {
            throw new InvalidOperationException($"No packet type registered for APID {header.Apid}");
        }

        if (!type.IsAssignableFrom(packetType))
        {
            throw new InvalidOperationException($"Registered type {packetType.Name} is not assignable to {type.Name}");
        }

        // Create packet instance
        var packet = (CcsdsPacket?)Activator.CreateInstance(packetType);
        if (packet == null)
        {
            throw new InvalidOperationException($"Could not create instance of {packetType.Name}");
        }

        packet.SequenceCount = header.SequenceCount;

        // Unpack secondary header if present
        if (header.SecondaryHeaderFlag)
        {
            var coarse = unpacker.UnpackUInt32();
            var fine = unpacker.UnpackUInt16();
            var epoch = new DateTimeOffset(1958, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var totalSeconds = coarse + (fine / 65536.0);
            packet.Timestamp = epoch.AddSeconds(totalSeconds);
        }

        // Unpack data
        packet.UnpackData(unpacker);

        return packet;
    }
}
