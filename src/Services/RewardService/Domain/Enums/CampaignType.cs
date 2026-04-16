namespace LoyaltySphere.RewardService.Domain.Enums;

/// <summary>
/// Defines the types of loyalty campaigns.
/// Replaces magic strings for type safety and extensibility.
/// </summary>
public enum CampaignType
{
    /// <summary>
    /// Fixed bonus points added to earned points
    /// </summary>
    Bonus = 1,

    /// <summary>
    /// Multiplier applied to earned points (e.g., 2x, 3x)
    /// </summary>
    Multiplier = 2,

    /// <summary>
    /// Percentage cashback on transaction amount
    /// </summary>
    Cashback = 3,

    /// <summary>
    /// Tiered rewards based on spending thresholds
    /// </summary>
    Tiered = 4,

    /// <summary>
    /// Special event or seasonal campaign
    /// </summary>
    Seasonal = 5
}
