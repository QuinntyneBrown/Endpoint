// Auto-generated code
using MessagePack;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Messaging.UdpMulticast;

/// <summary>
/// MessagePack-based message serializer.
/// </summary>
public class MessagePackMessageSerializer : IMessageSerializer
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

    public byte[] Serialize<T>(T value) =>
        MessagePackSerializer.Serialize(value, Options);

    public T? Deserialize<T>(byte[] data) =>
        MessagePackSerializer.Deserialize<T>(data, Options);

    public object? Deserialize(byte[] data, Type type) =>
        MessagePackSerializer.Deserialize(type, data, Options);
}
