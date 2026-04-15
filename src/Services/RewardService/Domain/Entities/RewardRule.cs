using LoyaltySphere.Common.Domain;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Entities;

/// <summary>
/// Represents a rule for calculating rewards based on transaction criteria.
/// Defines how many points are awarded for specific transaction types.
/// </summary>
public class RewardRule : Entity
{
    public string RuleName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal PointsPerUnit { get; private set; } // Points per currency unit
    public decimal? MinimumTransactionAmount { get; private set; }
    public decimal? MaximumTransactionAmount { get; private set; }
    public string? MerchantCategory { get; private set; }
    public string? MerchantId { get; private set; }
    public string? ProductCategory { get; private set; }
    public int Priority { get; private set; } // Higher priority rules are evaluated first
    public bool IsActive { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public string RuleType { get; private set; } = "Standard"; // Standard, Bonus, Cashback, Campaign

    private RewardRule() { } // EF Core

    private RewardRule(
        string tenantId,
        string ruleName,
        string description,
        decimal pointsPerUnit,
        string ruleType,
        int priority,
        decimal? minimumTransactionAmount = null,
        decimal? maximumTransactionAmount = null,
        string? merchantCategory = null,
        string? merchantId = null,
        string? productCategory = null,
        DateTime? validFrom = null,
        DateTime? validUntil = null)
    {
        TenantId = tenantId;
        RuleName = ruleName;
        Description = description;
        PointsPerUnit = pointsPerUnit;
        RuleType = ruleType;
        Priority = priority;
        MinimumTransactionAmount = minimumTransactionAmount;
        MaximumTransactionAmount = maximumTransactionAmount;
        MerchantCategory = merchantCategory;
        MerchantId = merchantId;
        ProductCategory = productCategory;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        IsActive = true;
    }

    public static RewardRule Create(
        string tenantId,
        string ruleName,
        string description,
        decimal pointsPerUnit,
        string ruleType = "Standard",
        int priority = 0,
        decimal? minimumTransactionAmount = null,
        decimal? maximumTransactionAmount = null,
        string? merchantCategory = null,
        string? merchantId = null,
        string? productCategory = null,
        DateTime? validFrom = null,
        DateTime? validUntil = null)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID is required", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("Rule name is required", nameof(ruleName));

        if (pointsPerUnit <= 0)
            throw new ArgumentException("Points per unit must be positive", nameof(pointsPerUnit));

        if (minimumTransactionAmount.HasValue && maximumTransactionAmount.HasValue)
        {
            if (minimumTransactionAmount.Value > maximumTransactionAmount.Value)
                throw new ArgumentException("Minimum amount cannot exceed maximum amount");
        }

        if (validFrom.HasValue && validUntil.HasValue)
        {
            if (validFrom.Value > validUntil.Value)
                throw new ArgumentException("Valid from date cannot be after valid until date");
        }

        return new RewardRule(
            tenantId,
            ruleName,
            description,
            pointsPerUnit,
            ruleType,
            priority,
            minimumTransactionAmount,
            maximumTransactionAmount,
            merchantCategory,
            merchantId,
            productCategory,
            validFrom,
            validUntil);
    }

    /// <summary>
    /// Checks if this rule applies to a given transaction
    /// </summary>
    public bool AppliesTo(
        Money transactionAmount,
        string? merchantId = null,
        string? merchantCategory = null,
        string? productCategory = null,
        DateTime? transactionDate = null)
    {
        if (!IsActive)
            return false;

        // Check date validity
        var checkDate = transactionDate ?? DateTime.UtcNow;
        if (ValidFrom.HasValue && checkDate < ValidFrom.Value)
            return false;

        if (ValidUntil.HasValue && checkDate > ValidUntil.Value)
            return false;

        // Check amount range
        if (MinimumTransactionAmount.HasValue && transactionAmount.Amount < MinimumTransactionAmount.Value)
            return false;

        if (MaximumTransactionAmount.HasValue && transactionAmount.Amount > MaximumTransactionAmount.Value)
            return false;

        // Check merchant filters
        if (!string.IsNullOrEmpty(MerchantId) && MerchantId != merchantId)
            return false;

        if (!string.IsNullOrEmpty(MerchantCategory) && MerchantCategory != merchantCategory)
            return false;

        // Check product category
        if (!string.IsNullOrEmpty(ProductCategory) && ProductCategory != productCategory)
            return false;

        return true;
    }

    /// <summary>
    /// Calculates points for a given transaction amount
    /// </summary>
    public Points CalculatePoints(Money transactionAmount)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot calculate points with inactive rule");

        var points = transactionAmount.Amount * PointsPerUnit;
        return Points.Create(points);
    }

    /// <summary>
    /// Activates the rule
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Rule is already active");

        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the rule
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Rule is already inactive");

        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the rule configuration
    /// </summary>
    public void Update(
        string ruleName,
        string description,
        decimal pointsPerUnit,
        int priority,
        decimal? minimumTransactionAmount = null,
        decimal? maximumTransactionAmount = null)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("Rule name is required", nameof(ruleName));

        if (pointsPerUnit <= 0)
            throw new ArgumentException("Points per unit must be positive", nameof(pointsPerUnit));

        RuleName = ruleName;
        Description = description;
        PointsPerUnit = pointsPerUnit;
        Priority = priority;
        MinimumTransactionAmount = minimumTransactionAmount;
        MaximumTransactionAmount = maximumTransactionAmount;

        MarkAsUpdated();
    }
}
