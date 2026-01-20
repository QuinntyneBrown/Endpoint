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

    /// <summary>
    /// Engine number (0-3)
    /// </summary>
    public byte EngineNumber { get; set; }

    public bool EngineRunning { get; set; }

    public bool StarterEngaged { get; set; }

    /// <summary>
    /// Engine RPM
    /// Unit: rpm
    /// </summary>
    public ushort Rpm { get; set; }

    /// <summary>
    /// Manifold pressure in 0.1 inHg
    /// Unit: inHg
    /// </summary>
    public ushort ManifoldPressure { get; set; }

    /// <summary>
    /// Fuel flow in 0.1 GPH
    /// Unit: gph
    /// </summary>
    public ushort FuelFlow { get; set; }

    /// <summary>
    /// Oil pressure in PSI
    /// Unit: psi
    /// </summary>
    public byte OilPressure { get; set; }

    /// <summary>
    /// Oil temperature in deg C
    /// Unit: degC
    /// </summary>
    public byte OilTemperature { get; set; }

    /// <summary>
    /// CHT in deg C
    /// Unit: degC
    /// </summary>
    public ushort CylinderHeadTemp { get; set; }

    /// <summary>
    /// EGT in deg C
    /// Unit: degC
    /// </summary>
    public ushort ExhaustGasTemp { get; set; }

    public override void PackData(BitPacker packer)
    {
        packer.PackUInt32(AircraftId);
        packer.PackByte(EngineNumber);
        packer.PackBool(EngineRunning);
        packer.PackBool(StarterEngaged);
        packer.PackSpare(6); // spare
        packer.PackUInt16(Rpm);
        packer.PackUInt16(ManifoldPressure);
        packer.PackUInt16(FuelFlow);
        packer.PackByte(OilPressure);
        packer.PackByte(OilTemperature);
        packer.PackUInt16(CylinderHeadTemp);
        packer.PackUInt16(ExhaustGasTemp);
    }

    public override void UnpackData(BitUnpacker unpacker)
    {
        AircraftId = unpacker.UnpackUInt32();
        EngineNumber = unpacker.UnpackByte();
        EngineRunning = unpacker.UnpackBool();
        StarterEngaged = unpacker.UnpackBool();
        unpacker.SkipBits(6); // spare
        Rpm = unpacker.UnpackUInt16();
        ManifoldPressure = unpacker.UnpackUInt16();
        FuelFlow = unpacker.UnpackUInt16();
        OilPressure = unpacker.UnpackByte();
        OilTemperature = unpacker.UnpackByte();
        CylinderHeadTemp = unpacker.UnpackUInt16();
        ExhaustGasTemp = unpacker.UnpackUInt16();
    }
}
