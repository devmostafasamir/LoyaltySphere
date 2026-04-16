using LoyaltySphere.Common.Domain;

namespace LoyaltySphere.RewardService.Domain.ValueObjects;

/// <summary>
/// Value object representing monetary amounts.
/// Includes currency code and ensures proper decimal handling.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "EGP")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = "EGP") => new(0, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Cannot subtract more than available amount");

        return new Money(result, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentException("Multiplier cannot be negative", nameof(multiplier));

        return new Money(Amount * multiplier, Currency);
    }

    public Money ApplyPercentage(decimal percentage)
    {
        return new Money(Amount * (percentage / 100), Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    public bool IsZero => Amount == 0;

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot operate on different currencies: {Currency} and {other.Currency}");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";

    public static implicit operator decimal(Money money) => money.Amount;
}
