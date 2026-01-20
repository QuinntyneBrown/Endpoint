// Auto-generated code
namespace FlightSim.Shared.Messaging.Ccsds;

/// <summary>
/// Unpacks values at bit-level precision from a byte array.
/// Follows big-endian byte order (network order) as per CCSDS standard.
/// </summary>
public class BitUnpacker
{
    private readonly byte[] _data;
    private int _bytePosition;
    private int _bitPosition; // 0-7, MSB first

    public BitUnpacker(byte[] data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    /// <summary>
    /// Gets or sets the current bit position.
    /// </summary>
    public int Position
    {
        get => _bytePosition * 8 + _bitPosition;
        set
        {
            _bytePosition = value / 8;
            _bitPosition = value % 8;
        }
    }

    /// <summary>
    /// Gets the total number of bits available.
    /// </summary>
    public int TotalBits => _data.Length * 8;

    /// <summary>
    /// Unpacks an unsigned value with the specified bit width.
    /// </summary>
    public ulong UnpackUnsigned(int bitWidth)
    {
        ulong value = 0;
        for (int i = 0; i < bitWidth; i++)
        {
            if (_bytePosition >= _data.Length)
            {
                throw new InvalidOperationException("Attempted to read beyond end of data");
            }

            int bit = (_data[_bytePosition] >> (7 - _bitPosition)) & 1;
            value = (value << 1) | (ulong)bit;

            _bitPosition++;
            if (_bitPosition == 8)
            {
                _bitPosition = 0;
                _bytePosition++;
            }
        }

        return value;
    }

    /// <summary>
    /// Unpacks a signed value with the specified bit width (two's complement).
    /// </summary>
    public long UnpackSigned(int bitWidth)
    {
        ulong unsigned = UnpackUnsigned(bitWidth);

        // Sign extend if necessary
        if ((unsigned & (1UL << (bitWidth - 1))) != 0)
        {
            // Negative number - extend sign
            unsigned |= ~((1UL << bitWidth) - 1);
        }

        return (long)unsigned;
    }

    /// <summary>
    /// Unpacks a boolean from a single bit.
    /// </summary>
    public bool UnpackBool() => UnpackUnsigned(1) != 0;

    /// <summary>
    /// Unpacks a byte (8 bits).
    /// </summary>
    public byte UnpackByte() => (byte)UnpackUnsigned(8);

    /// <summary>
    /// Unpacks a 16-bit unsigned integer.
    /// </summary>
    public ushort UnpackUInt16() => (ushort)UnpackUnsigned(16);

    /// <summary>
    /// Unpacks a 32-bit unsigned integer.
    /// </summary>
    public uint UnpackUInt32() => (uint)UnpackUnsigned(32);

    /// <summary>
    /// Unpacks a 16-bit signed integer.
    /// </summary>
    public short UnpackInt16() => (short)UnpackSigned(16);

    /// <summary>
    /// Unpacks a 32-bit signed integer.
    /// </summary>
    public int UnpackInt32() => (int)UnpackSigned(32);

    /// <summary>
    /// Unpacks a 32-bit float.
    /// </summary>
    public float UnpackFloat32()
    {
        var bytes = new byte[4];
        for (int i = 0; i < 4; i++)
        {
            bytes[i] = UnpackByte();
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return BitConverter.ToSingle(bytes, 0);
    }

    /// <summary>
    /// Unpacks a 64-bit float.
    /// </summary>
    public double UnpackFloat64()
    {
        var bytes = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            bytes[i] = UnpackByte();
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return BitConverter.ToDouble(bytes, 0);
    }

    /// <summary>
    /// Unpacks raw bytes.
    /// </summary>
    public byte[] UnpackBytes(int count)
    {
        var bytes = new byte[count];
        for (int i = 0; i < count; i++)
        {
            bytes[i] = UnpackByte();
        }

        return bytes;
    }

    /// <summary>
    /// Skips spare/reserved bits.
    /// </summary>
    public void SkipBits(int bitWidth) => UnpackUnsigned(bitWidth);

    /// <summary>
    /// Aligns to the next byte boundary.
    /// </summary>
    public void AlignToByte()
    {
        if (_bitPosition > 0)
        {
            SkipBits(8 - _bitPosition);
        }
    }
}
