// Auto-generated code
using MCC.Shared.Shared.Messaging.Abstractions;

namespace MCC.Shared.Shared.Messaging.Jsc;

/// <summary>
/// JSC message serializer implementing IMessageSerializer.
/// </summary>
public class JscMessageSerializer : IMessageSerializer
{
    /// <inheritdoc />
    public byte[] Serialize<T>(T value)
    {
        if (value is JscMessage message)
        {
            return message.Serialize();
        }

        throw new ArgumentException($"Type {typeof(T).Name} is not a JscMessage");
    }

    /// <inheritdoc />
    public T? Deserialize<T>(byte[] data)
    {
        if (typeof(T) == typeof(JscMessage))
        {
            return (T)(object)JscMessage.Deserialize(data);
        }

        throw new ArgumentException($"Type {typeof(T).Name} is not supported");
    }

    /// <inheritdoc />
    public object? Deserialize(byte[] data, Type type)
    {
        if (type == typeof(JscMessage))
        {
            return JscMessage.Deserialize(data);
        }

        throw new ArgumentException($"Type {type.Name} is not supported");
    }
}
