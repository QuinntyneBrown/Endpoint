// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Jsc;

/// <summary>
/// CRC-32 checksum calculator using IEEE 802.3 polynomial.
/// </summary>
public static class Crc32
{
    private static readonly uint[] Table = GenerateTable();

    private static uint[] GenerateTable()
    {
        const uint polynomial = 0xEDB88320;
        var table = new uint[256];

        for (uint i = 0; i < 256; i++)
        {
            var crc = i;
            for (var j = 0; j < 8; j++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ polynomial : crc >> 1;
            }
            table[i] = crc;
        }

        return table;
    }

    /// <summary>
    /// Calculates the CRC-32 checksum for the given data.
    /// </summary>
    public static uint Calculate(byte[] data)
    {
        return Calculate(data, 0, data.Length);
    }

    /// <summary>
    /// Calculates the CRC-32 checksum for a portion of the given data.
    /// </summary>
    public static uint Calculate(byte[] data, int offset, int length)
    {
        var crc = 0xFFFFFFFF;

        for (var i = offset; i < offset + length; i++)
        {
            crc = (crc >> 8) ^ Table[(crc ^ data[i]) & 0xFF];
        }

        return crc ^ 0xFFFFFFFF;
    }

    /// <summary>
    /// Updates a running CRC-32 checksum with additional data.
    /// </summary>
    public static uint Update(uint crc, byte[] data, int offset, int length)
    {
        crc ^= 0xFFFFFFFF;

        for (var i = offset; i < offset + length; i++)
        {
            crc = (crc >> 8) ^ Table[(crc ^ data[i]) & 0xFF];
        }

        return crc ^ 0xFFFFFFFF;
    }

    /// <summary>
    /// Verifies that the checksum matches the expected value.
    /// </summary>
    public static bool Verify(byte[] data, uint expectedChecksum)
    {
        return Calculate(data) == expectedChecksum;
    }

    /// <summary>
    /// Verifies that the checksum matches the expected value.
    /// </summary>
    public static bool Verify(byte[] data, int offset, int length, uint expectedChecksum)
    {
        return Calculate(data, offset, length) == expectedChecksum;
    }
}
