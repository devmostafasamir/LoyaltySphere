using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Services;

/// <summary>
/// Domain service for applying points caps and limits.
/// Prevents abuse by limiting maximum points per transaction.
/// </summary>
public interface IPointsCapService
{
    /// <summary>
    /// Applies maximum points cap to prevent abuse.
    /// </summary>
    Points ApplyPointsCap(Points points, Money transactionAmount);

    /// <summary>
    /// Gets the maximum allowed points for a transaction amount.
    /// </summary>
    Points GetMaximumPoints(Money transactionAmount);
}
