// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Jsc.Messages;

/// <summary>
/// Command acknowledgment
/// </summary>
public class CommandAck
{
    /// <summary>Message type code.</summary>
    public const byte TypeCode = 0x20;

    /// <summary>ID of command being acknowledged</summary>
    public uint CommandId { get; set; }

    /// <summary>Acknowledgment status</summary>
    public byte Status { get; set; }

}
