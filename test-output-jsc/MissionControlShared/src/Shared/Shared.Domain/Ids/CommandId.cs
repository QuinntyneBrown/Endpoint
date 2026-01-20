// Auto-generated code
namespace MCC.Shared.Shared.Domain.Ids;

/// <summary>
/// Strongly-typed ID for CommandId.
/// </summary>
public sealed class CommandId : StronglyTypedId<CommandId>
{
    public CommandId(Guid value) : base(value)
    {
    }

    public static CommandId New() => new(Guid.NewGuid());

    public static CommandId From(Guid value) => new(value);

    public static CommandId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out CommandId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new CommandId(guid);
            return true;
        }

        result = null;
        return false;
    }
}
