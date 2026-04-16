namespace LoyaltySphere.RewardService.Domain.Enums;

/// <summary>
/// Defines the types of rewards that can be earned or redeemed.
/// Replaces magic strings for type safety and maintainability.
/// </summary>
public enum RewardType
{
    /// <summary>
    /// Points earned from transactions
    /// </summary>
    Earned = 1,

    /// <summary>
    /// Points redeemed for rewards
    /// </summary>
    Redeemed = 2,

    /// <summary>
    /// Bonus points from campaigns or promotions
    /// </summary>
    Bonus = 3,

    /// <summary>
    /// Instant cashback rewards
    /// </summary>
    Cashback = 4,

    /// <summary>
    /// Points expired due to inactivity
    /// </summary>
    Expired = 5,

    /// <summary>
    /// Points adjusted by admin (positive or negative)
    /// </summary>
    Adjustment = 6
}
