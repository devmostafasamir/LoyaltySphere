using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.Services;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LoyaltySphere.RewardService.Application.Services;

/// <summary>
/// Core service for calculating rewards based on transactions.
/// Orchestrates domain services following Clean Architecture.
/// Application layer coordinates, domain services contain business logic.
/// </summary>
public class RewardCalculationService : IRewardCalculationService
{
    private readonly ILogger<RewardCalculationService> _logger;
    private readonly ITierCalculationService _tierCalculationService;
    private readonly IRewardRuleSelector _rewardRuleSelector;
    private readonly ICampaignEligibilityChecker _campaignEligibilityChecker;
    private readonly IPointsCapService _pointsCapService;

    public RewardCalculationService(
        ILogger<RewardCalculationService> logger,
        ITierCalculationService tierCalculationService,
        IRewardRuleSelector rewardRuleSelector,
        ICampaignEligibilityChecker campaignEligibilityChecker,
        IPointsCapService pointsCapService)
    {
        _logger = logger;
        _tierCalculationService = tierCalculationService;
        _rewardRuleSelector = rewardRuleSelector;
        _campaignEligibilityChecker = campaignEligibilityChecker;
        _pointsCapService = pointsCapService;
    }

    /// <summary>
    /// Calculates total reward points for a transaction.
    /// Applies base rules, campaigns, and tier multipliers.
    /// </summary>
    public Task<RewardCalculationResult> CalculateRewardAsync(
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

        // Step 1: Find the best matching reward rule using domain service
        var selectedRule = _rewardRuleSelector.SelectBestRule(
            applicableRules, 
            transactionAmount, 
            merchantId, 
            merchantCategory);
        
        if (selectedRule == null)
        {
            _logger.LogWarning(
                "No applicable reward rule found for transaction. Amount: {Amount}, Merchant: {MerchantId}",
                transactionAmount,
                merchantId);

            return Task.FromResult(RewardCalculationResult.NoReward("No applicable reward rule found"));
        }

        // Step 2: Calculate base points from the rule
        var basePoints = selectedRule.CalculatePoints(transactionAmount);
        _logger.LogDebug("Base points calculated: {Points} using rule {RuleName}", basePoints, selectedRule.RuleName);

        // Step 3: Apply tier multiplier using domain service
        var tierMultiplier = _tierCalculationService.GetTierMultiplier(customer.Tier);
        var pointsWithTierBonus = basePoints.Multiply(tierMultiplier);
        _logger.LogDebug("Points after tier bonus ({Tier}): {Points}", customer.Tier, pointsWithTierBonus);

        // Step 4: Apply campaign bonuses using domain service
        var campaignBonus = Points.Zero;
        Campaign? appliedCampaign = null;

        var eligibleCampaigns = _campaignEligibilityChecker.GetEligibleCampaigns(
            activeCampaigns,
            customer,
            transactionAmount,
            merchantCategory);

        foreach (var campaign in eligibleCampaigns)
        {
            var bonus = campaign.CalculateBonusPoints(pointsWithTierBonus, transactionAmount);
            if (bonus.IsGreaterThan(campaignBonus))
            {
                campaignBonus = bonus;
                appliedCampaign = campaign;
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

        // Step 6: Apply any caps or limits using domain service
        var finalPoints = _pointsCapService.ApplyPointsCap(totalPoints, transactionAmount);

        _logger.LogInformation(
            "Reward calculation complete. Base: {Base}, Tier Bonus: {TierBonus}, Campaign: {Campaign}, Total: {Total}",
            basePoints,
            pointsWithTierBonus,
            campaignBonus,
            finalPoints);

        return Task.FromResult(RewardCalculationResult.Success(
            finalPoints,
            basePoints,
            campaignBonus,
            selectedRule,
            appliedCampaign,
            tierMultiplier));
    }

    /// <summary>
    /// Calculates instant cashback for a transaction.
    /// Used by banks for immediate POS discounts.
    /// </summary>
    public Task<CashbackCalculationResult> CalculateInstantCashbackAsync(
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

        // Apply tier bonus using domain service
        var tierMultiplier = _tierCalculationService.GetTierMultiplier(customer.Tier);
        var finalPoints = cashbackPoints.Multiply(tierMultiplier);

        _logger.LogInformation(
            "Instant cashback calculated: {Amount} ({Points} points)",
            cashbackAmount,
            finalPoints);

        return Task.FromResult(new CashbackCalculationResult(
            finalPoints,
            cashbackAmount,
            cashbackPercentage,
            tierMultiplier));
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
