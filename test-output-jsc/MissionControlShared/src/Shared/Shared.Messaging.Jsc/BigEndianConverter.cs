// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Jsc;

/// <summary>
/// Provides methods for reading and writing values in big-endian (network) byte order.
/// </summary>
public static class BigEndianConverter
{
    /// <summary>
    /// Reads a 16-bit unsigned integer from a byte array in big-endian order.
    /// </summary>
    public static ushort ReadUInt16(byte[] buffer, int offset)
    {
        return (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
    }

    /// <summary>
    /// Reads a 32-bit unsigned integer from a byte array in big-endian order.
    /// </summary>
    public static uint ReadUInt32(byte[] buffer, int offset)
    {
        return (uint)((buffer[offset] << 24) |
                      (buffer[offset + 1] << 16) |
                      (buffer[offset + 2] << 8) |
                      buffer[offset + 3]);
    }

    /// <summary>
    /// Reads a 64-bit unsigned integer from a byte array in big-endian order.
    /// </summary>
    public static ulong ReadUInt64(byte[] buffer, int offset)
    {
        return ((ulong)buffer[offset] << 56) |
               ((ulong)buffer[offset + 1] << 48) |
               ((ulong)buffer[offset + 2] << 40) |
               ((ulong)buffer[offset + 3] << 32) |
               ((ulong)buffer[offset + 4] << 24) |
               ((ulong)buffer[offset + 5] << 16) |
               ((ulong)buffer[offset + 6] << 8) |
               buffer[offset + 7];
    }

    /// <summary>
    /// Reads a 16-bit signed integer from a byte array in big-endian order.
    /// </summary>
    public static short ReadInt16(byte[] buffer, int offset)
    {
        return (short)ReadUInt16(buffer, offset);
    }

    /// <summary>
    /// Reads a 32-bit signed integer from a byte array in big-endian order.
    /// </summary>
    public static int ReadInt32(byte[] buffer, int offset)
    {
        return (int)ReadUInt32(buffer, offset);
    }

    /// <summary>
    /// Reads a 64-bit signed integer from a byte array in big-endian order.
    /// </summary>
    public static long ReadInt64(byte[] buffer, int offset)
    {
        return (long)ReadUInt64(buffer, offset);
    }

    /// <summary>
    /// Writes a 16-bit unsigned integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteUInt16(byte[] buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)(value >> 8);
        buffer[offset + 1] = (byte)value;
    }

    /// <summary>
    /// Writes a 32-bit unsigned integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteUInt32(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    /// <summary>
    /// Writes a 64-bit unsigned integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteUInt64(byte[] buffer, int offset, ulong value)
    {
        buffer[offset] = (byte)(value >> 56);
        buffer[offset + 1] = (byte)(value >> 48);
        buffer[offset + 2] = (byte)(value >> 40);
        buffer[offset + 3] = (byte)(value >> 32);
        buffer[offset + 4] = (byte)(value >> 24);
        buffer[offset + 5] = (byte)(value >> 16);
        buffer[offset + 6] = (byte)(value >> 8);
        buffer[offset + 7] = (byte)value;
    }

    /// <summary>
    /// Writes a 16-bit signed integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteInt16(byte[] buffer, int offset, short value)
    {
        WriteUInt16(buffer, offset, (ushort)value);
    }

    /// <summary>
    /// Writes a 32-bit signed integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteInt32(byte[] buffer, int offset, int value)
    {
        WriteUInt32(buffer, offset, (uint)value);
    }

    /// <summary>
    /// Writes a 64-bit signed integer to a byte array in big-endian order.
    /// </summary>
    public static void WriteInt64(byte[] buffer, int offset, long value)
    {
        WriteUInt64(buffer, offset, (ulong)value);
    }

    /// <summary>
    /// Reads a 32-bit float from a byte array in big-endian order.
    /// </summary>
    public static float ReadFloat(byte[] buffer, int offset)
    {
        var bytes = new byte[4];
        Array.Copy(buffer, offset, bytes, 0, 4);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToSingle(bytes, 0);
    }

    /// <summary>
    /// Reads a 64-bit double from a byte array in big-endian order.
    /// </summary>
    public static double ReadDouble(byte[] buffer, int offset)
    {
        var bytes = new byte[8];
        Array.Copy(buffer, offset, bytes, 0, 8);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToDouble(bytes, 0);
    }

    /// <summary>
    /// Writes a 32-bit float to a byte array in big-endian order.
    /// </summary>
    public static void WriteFloat(byte[] buffer, int offset, float value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        Array.Copy(bytes, 0, buffer, offset, 4);
    }

    /// <summary>
    /// Writes a 64-bit double to a byte array in big-endian order.
    /// </summary>
    public static void WriteDouble(byte[] buffer, int offset, double value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        Array.Copy(bytes, 0, buffer, offset, 8);
    }
}
