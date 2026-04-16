using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Domain.Enums;

namespace LoyaltySphere.RewardService.Domain.Services;

/// <summary>
/// Implementation of tier calculation logic.
/// Encapsulates all tier-related business rules.
/// </summary>
public class TierCalculationService : ITierCalculationService
{
    private static readonly Dictionary<CustomerTier, decimal> TierMultipliers = new()
    {
        { CustomerTier.Bronze, 1.0m },
        { CustomerTier.Silver, 1.15m },
        { CustomerTier.Gold, 1.3m },
        { CustomerTier.Platinum, 1.5m },
        { CustomerTier.Diamond, 2.0m }
    };

    private static readonly Dictionary<CustomerTier, decimal> TierThresholds = new()
    {
        { CustomerTier.Bronze, 0m },
        { CustomerTier.Silver, 1000m },
        { CustomerTier.Gold, 5000m },
        { CustomerTier.Platinum, 15000m },
        { CustomerTier.Diamond, 50000m }
    };

    public decimal GetTierMultiplier(CustomerTier tier)
    {
        return TierMultipliers.TryGetValue(tier, out var multiplier) 
            ? multiplier 
            : 1.0m;
    }

    public CustomerTier CalculateNewTier(Points totalPoints)
    {
        var pointsValue = totalPoints.Value;

        if (pointsValue >= TierThresholds[CustomerTier.Diamond])
            return CustomerTier.Diamond;
        if (pointsValue >= TierThresholds[CustomerTier.Platinum])
            return CustomerTier.Platinum;
        if (pointsValue >= TierThresholds[CustomerTier.Gold])
            return CustomerTier.Gold;
        if (pointsValue >= TierThresholds[CustomerTier.Silver])
            return CustomerTier.Silver;

        return CustomerTier.Bronze;
    }

    public Dictionary<CustomerTier, decimal> GetTierThresholds()
    {
        return new Dictionary<CustomerTier, decimal>(TierThresholds);
    }
}
