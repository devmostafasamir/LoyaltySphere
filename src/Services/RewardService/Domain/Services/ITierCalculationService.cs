using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Domain.Enums;

namespace LoyaltySphere.RewardService.Domain.Services;

/// <summary>
/// Domain service for tier-related calculations.
/// Handles tier multipliers and tier upgrade logic.
/// </summary>
public interface ITierCalculationService
{
    /// <summary>
    /// Gets the points multiplier for a given tier.
    /// Higher tiers earn more points per transaction.
    /// </summary>
    decimal GetTierMultiplier(CustomerTier tier);

    /// <summary>
    /// Calculates the new tier based on total accumulated points.
    /// </summary>
    CustomerTier CalculateNewTier(Points totalPoints);

    /// <summary>
    /// Gets the point thresholds for each tier upgrade.
    /// </summary>
    Dictionary<CustomerTier, decimal> GetTierThresholds();
}
