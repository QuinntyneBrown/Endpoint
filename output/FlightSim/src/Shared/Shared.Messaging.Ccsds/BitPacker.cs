// Auto-generated code
namespace FlightSim.Shared.Messaging.Ccsds;

/// <summary>
/// Packs values at bit-level precision into a byte array.
/// Follows big-endian byte order (network order) as per CCSDS standard.
/// </summary>
public class BitPacker
{
    private readonly List<byte> _buffer = new();
    private int _currentByte;
    private int _bitPosition; // 0-7, MSB first

    /// <summary>
    /// Gets the packed data.
    /// </summary>
    public byte[] GetBytes()
    {
        // Flush any partial byte
        if (_bitPosition > 0)
        {
            _buffer.Add((byte)_currentByte);
        }

        return _buffer.ToArray();
    }

    /// <summary>
    /// Packs an unsigned value with the specified bit width.
    /// </summary>
    public void PackUnsigned(ulong value, int bitWidth)
    {
        for (int i = bitWidth - 1; i >= 0; i--)
        {
            int bit = (int)((value >> i) & 1);
            _currentByte = (_currentByte << 1) | bit;
            _bitPosition++;

            if (_bitPosition == 8)
            {
                _buffer.Add((byte)_currentByte);
                _currentByte = 0;
                _bitPosition = 0;
            }
        }
    }

    /// <summary>
    /// Packs a signed value with the specified bit width (two's complement).
    /// </summary>
    public void PackSigned(long value, int bitWidth)
    {
        // Convert to unsigned representation for packing
        ulong unsigned = (ulong)value & ((1UL << bitWidth) - 1);
        PackUnsigned(unsigned, bitWidth);
    }

    /// <summary>
    /// Packs a boolean as a single bit.
    /// </summary>
    public void PackBool(bool value) => PackUnsigned(value ? 1UL : 0UL, 1);

    /// <summary>
    /// Packs a byte (8 bits).
    /// </summary>
    public void PackByte(byte value) => PackUnsigned(value, 8);

    /// <summary>
    /// Packs a 16-bit unsigned integer.
    /// </summary>
    public void PackUInt16(ushort value) => PackUnsigned(value, 16);

    /// <summary>
    /// Packs a 32-bit unsigned integer.
    /// </summary>
    public void PackUInt32(uint value) => PackUnsigned(value, 32);

    /// <summary>
    /// Packs a 16-bit signed integer.
    /// </summary>
    public void PackInt16(short value) => PackSigned(value, 16);

    /// <summary>
    /// Packs a 32-bit signed integer.
    /// </summary>
    public void PackInt32(int value) => PackSigned(value, 32);

    /// <summary>
    /// Packs a 32-bit float.
    /// </summary>
    public void PackFloat32(float value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        foreach (var b in bytes)
        {
            PackByte(b);
        }
    }

    /// <summary>
    /// Packs a 64-bit float.
    /// </summary>
    public void PackFloat64(double value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        foreach (var b in bytes)
        {
            PackByte(b);
        }
    }

    /// <summary>
    /// Packs raw bytes.
    /// </summary>
    public void PackBytes(byte[] data)
    {
        foreach (var b in data)
        {
            PackByte(b);
        }
    }

    /// <summary>
    /// Packs spare/reserved bits.
    /// </summary>
    public void PackSpare(int bitWidth) => PackUnsigned(0, bitWidth);

    /// <summary>
    /// Aligns to the next byte boundary.
    /// </summary>
    public void AlignToByte()
    {
        if (_bitPosition > 0)
        {
            PackSpare(8 - _bitPosition);
        }
    }
}
