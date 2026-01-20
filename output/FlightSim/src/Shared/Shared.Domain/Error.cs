// Auto-generated code
namespace FlightSim.Shared.Domain;

/// <summary>
/// Represents an error.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error NotFound(string entityName, object id) =>
        new($"{entityName}.NotFound", $"{entityName} with id '{id}' was not found.");

    public static Error Validation(string propertyName, string message) =>
        new($"Validation.{propertyName}", message);
}
