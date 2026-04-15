using LoyaltySphere.Common.Domain;

namespace LoyaltySphere.RewardService.Domain.ValueObjects;

/// <summary>
/// Value object representing a tenant identifier.
/// Ensures tenant IDs are always valid and normalized.
/// </summary>
public sealed class TenantId : ValueObject
{
    public string Value { get; }

    private TenantId(string value)
    {
        Value = value;
    }

    public static TenantId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(value));

        // Normalize to lowercase and trim
        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length < 3)
            throw new ArgumentException("Tenant ID must be at least 3 characters", nameof(value));

        if (normalized.Length > 100)
            throw new ArgumentException("Tenant ID cannot exceed 100 characters", nameof(value));

        return new TenantId(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(TenantId tenantId) => tenantId.Value;
    public static explicit operator TenantId(string value) => Create(value);
}
