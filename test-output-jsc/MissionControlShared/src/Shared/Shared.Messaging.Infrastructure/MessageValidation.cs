// Auto-generated code
namespace MCC.Shared.Shared.Messaging.Infrastructure;

/// <summary>
/// Result of message validation.
/// </summary>
public class MessageValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();

    public static MessageValidationResult Success() => new() { IsValid = true };

    public static MessageValidationResult Failure(params string[] errors) =>
        new() { IsValid = false, Errors = errors.ToList() };
}

/// <summary>
/// Interface for message validators.
/// </summary>
public interface IMessageValidator<T>
{
    MessageValidationResult Validate(T message);
}

/// <summary>
/// Base class for message validators with fluent rule building.
/// </summary>
public abstract class MessageValidatorBase<T> : IMessageValidator<T>
{
    private readonly List<Func<T, (bool isValid, string? error)>> _rules = new();

    protected void AddRule(Func<T, bool> predicate, string errorMessage)
    {
        _rules.Add(msg => (predicate(msg), predicate(msg) ? null : errorMessage));
    }

    protected void RequireNotNull<TValue>(Func<T, TValue?> selector, string propertyName)
        where TValue : class
    {
        AddRule(msg => selector(msg) != null, $"{propertyName} is required");
    }

    protected void RequireNotEmpty(Func<T, string?> selector, string propertyName)
    {
        AddRule(msg => !string.IsNullOrWhiteSpace(selector(msg)), $"{propertyName} cannot be empty");
    }

    protected void RequireInRange(Func<T, int> selector, int min, int max, string propertyName)
    {
        AddRule(msg =>
        {
            var value = selector(msg);
            return value >= min && value <= max;
        }, $"{propertyName} must be between {min} and {max}");
    }

    public MessageValidationResult Validate(T message)
    {
        var errors = new List<string>();

        foreach (var rule in _rules)
        {
            var (isValid, error) = rule(message);
            if (!isValid && error != null)
            {
                errors.Add(error);
            }
        }

        return errors.Count > 0
            ? MessageValidationResult.Failure(errors.ToArray())
            : MessageValidationResult.Success();
    }
}
