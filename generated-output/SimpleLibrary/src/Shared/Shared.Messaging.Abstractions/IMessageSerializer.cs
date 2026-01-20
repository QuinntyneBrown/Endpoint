// Auto-generated code
namespace Simple.Shared.Messaging.Abstractions;

/// <summary>
/// Interface for message serialization.
/// </summary>
public interface IMessageSerializer
{
    /// <summary>
    /// Serializes an object to bytes.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <returns>Serialized bytes.</returns>
    byte[] Serialize<T>(T value);

    /// <summary>
    /// Deserializes bytes to an object.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="data">The data to deserialize.</param>
    /// <returns>Deserialized object.</returns>
    T? Deserialize<T>(byte[] data);

    /// <summary>
    /// Deserializes bytes to an object of a specific type.
    /// </summary>
    /// <param name="data">The data to deserialize.</param>
    /// <param name="type">The type to deserialize to.</param>
    /// <returns>Deserialized object.</returns>
    object? Deserialize(byte[] data, Type type);
}
