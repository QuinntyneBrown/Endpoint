// Auto-generated code
namespace ECommerce.Shared.Domain;

/// <summary>
/// Base class for strongly-typed IDs with Guid underlying type.
/// </summary>
public abstract class StronglyTypedId<TSelf> : IEquatable<TSelf>
    where TSelf : StronglyTypedId<TSelf>
{
    public Guid Value { get; }

    protected StronglyTypedId(Guid value)
    {
        Value = value;
    }

    public bool Equals(TSelf? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is TSelf other && Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public static bool operator ==(StronglyTypedId<TSelf>? left, StronglyTypedId<TSelf>? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(StronglyTypedId<TSelf>? left, StronglyTypedId<TSelf>? right)
    {
        return !(left == right);
    }
}
