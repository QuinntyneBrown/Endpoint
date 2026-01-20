// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Infrastructure;

/// <summary>
/// Attribute to mark message version.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MessageVersionAttribute : Attribute
{
    public int Version { get; }
    public string? Description { get; set; }

    public MessageVersionAttribute(int version)
    {
        Version = version;
    }
}

/// <summary>
/// Interface for versioned messages.
/// </summary>
public interface IVersionedMessage
{
    /// <summary>Gets the message schema version.</summary>
    int Version { get; }
}

/// <summary>
/// Helper for message version operations.
/// </summary>
public static class MessageVersioning
{
    /// <summary>
    /// Gets the version of a message type.
    /// </summary>
    public static int GetVersion<T>() where T : class
    {
        return GetVersion(typeof(T));
    }

    /// <summary>
    /// Gets the version of a message type.
    /// </summary>
    public static int GetVersion(Type messageType)
    {
        var attr = messageType.GetCustomAttributes(typeof(MessageVersionAttribute), false)
            .FirstOrDefault() as MessageVersionAttribute;

        return attr?.Version ?? 1;
    }

    /// <summary>
    /// Checks if message types are compatible (same version).
    /// </summary>
    public static bool AreCompatible(Type type1, Type type2)
    {
        return GetVersion(type1) == GetVersion(type2);
    }
}
