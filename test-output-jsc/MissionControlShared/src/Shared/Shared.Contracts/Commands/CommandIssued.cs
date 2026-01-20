// Auto-generated code
using MessagePack;
using MCC.Shared.Shared.Messaging.Abstractions;

namespace MCC.Shared.Shared.Contracts.Commands;

[MessagePackObject]
public sealed class CommandIssued : EventBase
{
    [Key(0)]
    public Guid CommandId { get; init; }

    [Key(1)]
    public string CommandType { get; init; } = string.Empty;

}
