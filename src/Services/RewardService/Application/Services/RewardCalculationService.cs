using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LoyaltySphere.RewardService.Application.Services;

/// <summary>
/// Core service for calculating rewards based on transactions.
/// Applies reward rules, campaigns, and tier bonuses.
/// This is the heart of the loyalty program logic.
/// </summary>
public class RewardCalculationService : IRewardCalculationService
{
    private readonly ILogger<RewardCalculationService> _logger;

    public RewardCalculationService(ILogger<RewardCalculationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculates total reward points for a transaction.
    /// Applies base rules, campaigns, and tier multipliers.
    /// </summary>
    public async Task<RewardCalculationResult> CalculateRewardAsync(
        Customer customer,
        Money transactionAmount,
        IEnumerable<RewardRule> applicableRules,
        IEnumerable<Campaign> activeCampaigns,
        string? merchantId = null,
        string? merchantCategory = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Calculating reward for customer {CustomerId}, amount {Amount}",
            customer.CustomerId,
            transactionAmount);

        // Step 1: Find the best matching reward rule
        var selectedRule = SelectBestRule(applicableRules, transactionAmount, merchantId, merchantCategory);
        
        if (selectedRule == null)
        {
            _logger.LogWarning(
                "No applicable reward rule found for transaction. Amount: {Amount}, Merchant: {MerchantId}",
                transactionAmount,
                merchantId);

            return RewardCalculationResult.NoReward("No applicable reward rule found");
        }

        // Step 2: Calculate base points from the rule
        var basePoints = selectedRule.CalculatePoints(transactionAmount);
        _logger.LogDebug("Base points calculated: {Points} using rule {RuleName}", basePoints, selectedRule.RuleName);

        // Step 3: Apply tier multiplier
        var tierMultiplier = GetTierMultiplier(customer.Tier);
        var pointsWithTierBonus = basePoints.Multiply(tierMultiplier);
        _logger.LogDebug("Points after tier bonus ({Tier}): {Points}", customer.Tier, pointsWithTierBonus);

        // Step 4: Apply campaign bonuses
        var campaignBonus = Points.Zero;
        Campaign? appliedCampaign = null;

        foreach (var campaign in activeCampaigns.OrderByDescending(c => c.Priority))
        {
            if (campaign.IsCustomerEligible(customer.Tier, transactionAmount, merchantCategory))
            {
                var bonus = campaign.CalculateBonusPoints(pointsWithTierBonus, transactionAmount);
                if (bonus.IsGreaterThan(campaignBonus))
                {
                    campaignBonus = bonus;
                    appliedCampaign = campaign;
                }
            }
        }

        if (appliedCampaign != null)
        {
            _logger.LogInformation(
                "Campaign bonus applied: {Bonus} points from campaign {CampaignName}",
                campaignBonus,
                appliedCampaign.CampaignName);
        }

        // Step 5: Calculate total points
        var totalPoints = pointsWithTierBonus.Add(campaignBonus);

        // Step 6: Apply any caps or limits
        var finalPoints = ApplyPointsCap(totalPoints, transactionAmount);

        _logger.LogInformation(
            "Reward calculation complete. Base: {Base}, Tier Bonus: {TierBonus}, Campaign: {Campaign}, Total: {Total}",
            basePoints,
            pointsWithTierBonus,
            campaignBonus,
            finalPoints);

        return RewardCalculationResult.Success(
            finalPoints,
            basePoints,
            campaignBonus,
            selectedRule,
            appliedCampaign,
            tierMultiplier);
    }

    /// <summary>
    /// Calculates instant cashback for a transaction.
    /// Used by banks for immediate POS discounts.
    /// </summary>
    public async Task<CashbackCalculationResult> CalculateInstantCashbackAsync(
        Customer customer,
        Money transactionAmount,
        decimal cashbackPercentage,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Calculating instant cashback for customer {CustomerId}, amount {Amount}, rate {Rate}%",
            customer.CustomerId,
            transactionAmount,
            cashbackPercentage);

        if (cashbackPercentage <= 0 || cashbackPercentage > 100)
        {
            throw new ArgumentException("Cashback percentage must be between 0 and 100", nameof(cashbackPercentage));
        }

        // Calculate cashback amount
        var cashbackAmount = transactionAmount.ApplyPercentage(cashbackPercentage);

        // Convert to points (1 EGP = 1 point for simplicity)
        var cashbackPoints = Points.Create(cashbackAmount.Amount);

        // Apply tier bonus
        var tierMultiplier = GetTierMultiplier(customer.Tier);
        var finalPoints = cashbackPoints.Multiply(tierMultiplier);

        _logger.LogInformation(
            "Instant cashback calculated: {Amount} ({Points} points)",
            cashbackAmount,
            finalPoints);

        return new CashbackCalculationResult(
            finalPoints,
            cashbackAmount,
            cashbackPercentage,
            tierMultiplier);
    }

    /// <summary>
    /// Validates if a customer can redeem the requested points.
    /// </summary>
    public Task<RedemptionValidationResult> ValidateRedemptionAsync(
        Customer customer,
        Points pointsToRedeem,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Validating redemption for customer {CustomerId}, points {Points}",
            customer.CustomerId,
            pointsToRedeem);

        if (!customer.IsActive)
        {
            return Task.FromResult(RedemptionValidationResult.Failed("Customer account is inactive"));
        }

        if (pointsToRedeem.IsZero)
        {
            return Task.FromResult(RedemptionValidationResult.Failed("Cannot redeem zero points"));
        }

        if (customer.PointsBalance.IsLessThan(pointsToRedeem))
        {
            return Task.FromResult(RedemptionValidationResult.Failed(
                $"Insufficient points. Available: {customer.PointsBalance}, Requested: {pointsToRedeem}"));
        }

        // Minimum redemption threshold (e.g., 100 points)
        var minimumRedemption = Points.Create(100);
        if (pointsToRedeem.IsLessThan(minimumRedemption))
        {
            return Task.FromResult(RedemptionValidationResult.Failed(
                $"Minimum redemption is {minimumRedemption} points"));
        }

        _logger.LogInformation("Redemption validation passed");
        return Task.FromResult(RedemptionValidationResult.Success());
    }

    /// <summary>
    /// Selects the best reward rule based on priority and applicability.
    /// </summary>
    private RewardRule? SelectBestRule(
        IEnumerable<RewardRule> rules,
        Money transactionAmount,
        string? merchantId,
        string? merchantCategory)
    {
        return rules
            .Where(r => r.AppliesTo(transactionAmount, merchantId, merchantCategory))
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.PointsPerUnit)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the points multiplier based on customer tier.
    /// Higher tiers earn more points per transaction.
    /// </summary>
    private decimal GetTierMultiplier(string tier)
    {
        return tier switch
        {
            "Platinum" => 1.5m,  // 50% bonus
            "Gold" => 1.3m,      // 30% bonus
            "Silver" => 1.15m,   // 15% bonus
            "Bronze" => 1.0m,    // No bonus
            _ => 1.0m
        };
    }

    /// <summary>
    /// Applies maximum points cap to prevent abuse.
    /// Example: Maximum 10% of transaction amount as points.
    /// </summary>
    private Points ApplyPointsCap(Points points, Money transactionAmount)
    {
        // Cap at 10% of transaction amount
        var maxPoints = Points.Create(transactionAmount.Amount * 0.10m);

        if (points.IsGreaterThan(maxPoints))
        {
            _logger.LogWarning(
                "Points capped from {Original} to {Capped} (10% of transaction)",
                points,
                maxPoints);
            return maxPoints;
        }

        return points;
    }
}

/// <summary>
/// Interface for reward calculation service.
/// </summary>
public interface IRewardCalculationService
{
    Task<RewardCalculationResult> CalculateRewardAsync(
        Customer customer,
        Money transactionAmount,
        IEnumerable<RewardRule> applicableRules,
        IEnumerable<Campaign> activeCampaigns,
        string? merchantId = null,
        string? merchantCategory = null,
        CancellationToken cancellationToken = default);

    Task<CashbackCalculationResult> CalculateInstantCashbackAsync(
        Customer customer,
        Money transactionAmount,
        decimal cashbackPercentage,
        CancellationToken cancellationToken = default);

    Task<RedemptionValidationResult> ValidateRedemptionAsync(
        Customer customer,
        Points pointsToRedeem,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of reward calculation.
/// </summary>
public record RewardCalculationResult
{
    public bool IsSuccess { get; init; }
    public Points TotalPoints { get; init; } = Points.Zero;
    public Points BasePoints { get; init; } = Points.Zero;
    public Points CampaignBonus { get; init; } = Points.Zero;
    public RewardRule? AppliedRule { get; init; }
    public Campaign? AppliedCampaign { get; init; }
    public decimal TierMultiplier { get; init; }
    public string? ErrorMessage { get; init; }

    public static RewardCalculationResult Success(
        Points totalPoints,
        Points basePoints,
        Points campaignBonus,
        RewardRule appliedRule,
        Campaign? appliedCampaign,
        decimal tierMultiplier)
    {
        return new RewardCalculationResult
        {
            IsSuccess = true,
            TotalPoints = totalPoints,
            BasePoints = basePoints,
            CampaignBonus = campaignBonus,
            AppliedRule = appliedRule,
            AppliedCampaign = appliedCampaign,
            TierMultiplier = tierMultiplier
        };
    }

    public static RewardCalculationResult NoReward(string reason)
    {
        return new RewardCalculationResult
        {
            IsSuccess = false,
            ErrorMessage = reason
        };
    }
}

/// <summary>
/// Result of cashback calculation.
/// </summary>
public record CashbackCalculationResult(
    Points CashbackPoints,
    Money CashbackAmount,
    decimal CashbackPercentage,
    decimal TierMultiplier);

/// <summary>
/// Result of redemption validation.
/// </summary>
public record RedemptionValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }

    public static RedemptionValidationResult Success()
    {
        return new RedemptionValidationResult { IsValid = true };
    }

    public static RedemptionValidationResult Failed(string errorMessage)
    {
        return new RedemptionValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}
