// Auto-generated code
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MCC.Shared.Shared.Messaging.Infrastructure;

/// <summary>
/// JSON serialization helper with configured options.
/// </summary>
public static class JsonSerializationHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>Serializes an object to JSON string.</summary>
    public static string Serialize<T>(T value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options ?? DefaultOptions);
    }

    /// <summary>Serializes an object to UTF-8 bytes.</summary>
    public static byte[] SerializeToBytes<T>(T value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, options ?? DefaultOptions);
    }

    /// <summary>Deserializes a JSON string to an object.</summary>
    public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
    }

    /// <summary>Deserializes UTF-8 bytes to an object.</summary>
    public static T? DeserializeFromBytes<T>(byte[] data, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(data, options ?? DefaultOptions);
    }

    /// <summary>Attempts to deserialize with error handling.</summary>
    public static bool TryDeserialize<T>(string json, out T? result, JsonSerializerOptions? options = null)
    {
        try
        {
            result = Deserialize<T>(json, options);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>Creates a deep clone via JSON serialization.</summary>
    public static T? Clone<T>(T value, JsonSerializerOptions? options = null)
    {
        var json = Serialize(value, options);
        return Deserialize<T>(json, options);
    }
}

/// <summary>
/// Binary serialization helper for big-endian operations.
/// </summary>
public static class BinarySerializationHelper
{
    /// <summary>Writes a boolean value.</summary>
    public static void WriteBool(byte[] buffer, int offset, bool value)
    {
        buffer[offset] = value ? (byte)1 : (byte)0;
    }

    /// <summary>Reads a boolean value.</summary>
    public static bool ReadBool(byte[] buffer, int offset)
    {
        return buffer[offset] != 0;
    }

    /// <summary>Writes a 16-bit unsigned integer in big-endian order.</summary>
    public static void WriteUInt16BE(byte[] buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)(value >> 8);
        buffer[offset + 1] = (byte)value;
    }

    /// <summary>Reads a 16-bit unsigned integer in big-endian order.</summary>
    public static ushort ReadUInt16BE(byte[] buffer, int offset)
    {
        return (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
    }

    /// <summary>Writes a 32-bit unsigned integer in big-endian order.</summary>
    public static void WriteUInt32BE(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    /// <summary>Reads a 32-bit unsigned integer in big-endian order.</summary>
    public static uint ReadUInt32BE(byte[] buffer, int offset)
    {
        return (uint)((buffer[offset] << 24) | (buffer[offset + 1] << 16) |
                      (buffer[offset + 2] << 8) | buffer[offset + 3]);
    }

    /// <summary>Writes a length-prefixed string (2-byte length + UTF-8 data).</summary>
    public static int WriteLengthPrefixedString(byte[] buffer, int offset, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteUInt16BE(buffer, offset, (ushort)bytes.Length);
        Array.Copy(bytes, 0, buffer, offset + 2, bytes.Length);
        return 2 + bytes.Length;
    }

    /// <summary>Reads a length-prefixed string.</summary>
    public static string ReadLengthPrefixedString(byte[] buffer, int offset, out int bytesRead)
    {
        var length = ReadUInt16BE(buffer, offset);
        var str = Encoding.UTF8.GetString(buffer, offset + 2, length);
        bytesRead = 2 + length;
        return str;
    }
}
