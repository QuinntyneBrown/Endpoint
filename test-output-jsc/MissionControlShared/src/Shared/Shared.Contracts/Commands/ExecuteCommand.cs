// Auto-generated code
using MessagePack;

namespace MCC.Shared.Shared.Contracts.Commands;

[MessagePackObject]
public sealed class ExecuteCommand
{
    [Key(0)]
    public string CommandType { get; init; } = string.Empty;

    [Key(1)]
    public Dictionary<string, string> Parameters { get; init; } = new();

}
