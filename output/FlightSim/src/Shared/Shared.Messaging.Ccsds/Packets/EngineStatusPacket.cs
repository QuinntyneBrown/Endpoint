// Auto-generated code
namespace FlightSim.Shared.Messaging.Ccsds.Packets;

/// <summary>
/// Engine telemetry packet
/// </summary>
public class EngineStatusPacket : CcsdsPacket
{
    public override int Apid => 101;

    public override int PacketType => 0;

    public uint AircraftId { get; set; }

    public byte EngineNumber { get; set; }

    public bool EngineRunning { get; set; }

    /// <summary>
    /// Engine RPM
    /// </summary>
    public ushort Rpm { get; set; }

    /// <summary>
    /// Fuel flow in 0.1 GPH
    /// </summary>
    public ushort FuelFlow { get; set; }

    /// <summary>
    /// Oil pressure in PSI
    /// </summary>
    public byte OilPressure { get; set; }

    /// <summary>
    /// Oil temperature in deg C
    /// </summary>
    public byte OilTemp { get; set; }

    public override void PackData(BitPacker packer)
    {
        packer.PackUInt32(AircraftId);
        packer.PackByte(EngineNumber);
        packer.PackBool(EngineRunning);
        packer.PackSpare(7); // spare
        packer.PackUInt16(Rpm);
        packer.PackUInt16(FuelFlow);
        packer.PackByte(OilPressure);
        packer.PackByte(OilTemp);
    }

    public override void UnpackData(BitUnpacker unpacker)
    {
        AircraftId = unpacker.UnpackUInt32();
        EngineNumber = unpacker.UnpackByte();
        EngineRunning = unpacker.UnpackBool();
        unpacker.SkipBits(7); // spare
        Rpm = unpacker.UnpackUInt16();
        FuelFlow = unpacker.UnpackUInt16();
        OilPressure = unpacker.UnpackByte();
        OilTemp = unpacker.UnpackByte();
    }
}
