using LoyaltySphere.Common.Domain;

namespace LoyaltySphere.RewardService.Domain.ValueObjects;

/// <summary>
/// Value object representing loyalty points.
/// Ensures points are always non-negative and provides arithmetic operations.
/// </summary>
public sealed class Points : ValueObject
{
    public decimal Value { get; }

    private Points(decimal value)
    {
        Value = value;
    }

    public static Points Create(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Points cannot be negative", nameof(value));

        return new Points(value);
    }

    public static Points Zero => new(0);

    public Points Add(Points other)
    {
        return new Points(Value + other.Value);
    }

    public Points Subtract(Points other)
    {
        var result = Value - other.Value;
        if (result < 0)
            throw new InvalidOperationException("Cannot subtract more points than available");

        return new Points(result);
    }

    public Points Multiply(decimal multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentException("Multiplier cannot be negative", nameof(multiplier));

        return new Points(Value * multiplier);
    }

    public bool IsGreaterThan(Points other) => Value > other.Value;
    public bool IsGreaterThanOrEqual(Points other) => Value >= other.Value;
    public bool IsLessThan(Points other) => Value < other.Value;
    public bool IsZero => Value == 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value:0} points";

    public static implicit operator decimal(Points points) => points.Value;
    public static explicit operator Points(decimal value) => Create(value);
}
