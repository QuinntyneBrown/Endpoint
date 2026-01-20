// Auto-generated code
namespace Simple.Shared.Domain.Ids;

/// <summary>
/// Strongly-typed ID for NotificationId.
/// </summary>
public sealed class NotificationId : StronglyTypedId<NotificationId>
{
    public NotificationId(Guid value) : base(value)
    {
    }

    public static NotificationId New() => new(Guid.NewGuid());

    public static NotificationId From(Guid value) => new(value);

    public static NotificationId Parse(string value) => new(Guid.Parse(value));

    public static bool TryParse(string value, out NotificationId? result)
    {
        if (Guid.TryParse(value, out var guid))
        {
            result = new NotificationId(guid);
            return true;
        }

        result = null;
        return false;
    }
}
