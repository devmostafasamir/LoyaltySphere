namespace LoyaltySphere.RewardService.Domain.Enums;

/// <summary>
/// Defines customer loyalty tiers.
/// Replaces magic strings for type safety and tier management.
/// </summary>
public enum CustomerTier
{
    /// <summary>
    /// Entry level tier - 1.0x multiplier
    /// </summary>
    Bronze = 1,

    /// <summary>
    /// Mid-level tier - 1.25x multiplier
    /// </summary>
    Silver = 2,

    /// <summary>
    /// Premium tier - 1.5x multiplier
    /// </summary>
    Gold = 3,

    /// <summary>
    /// Elite tier - 2.0x multiplier
    /// </summary>
    Platinum = 4,

    /// <summary>
    /// VIP tier - 2.5x multiplier
    /// </summary>
    Diamond = 5
}
