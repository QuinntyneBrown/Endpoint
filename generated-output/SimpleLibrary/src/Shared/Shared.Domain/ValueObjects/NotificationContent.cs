// Auto-generated code
namespace Simple.Shared.Domain.ValueObjects;

/// <summary>
/// Value object for NotificationContent.
/// </summary>
public sealed class NotificationContent : ValueObject
{
    public string Title { get; }
    public string Body { get; }
    public int Priority { get; }

    public NotificationContent(string title, string body, int priority)
    {
        Title = title;
        Body = body;
        Priority = priority;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return new object?[]
        {
            Title,
            Body,
            Priority
        };
    }
}
