using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Domain.Enums;

namespace LoyaltySphere.RewardService.Domain.Strategies;

/// <summary>
/// Strategy interface for campaign bonus calculations.
/// Follows Open/Closed Principle - new campaign types can be added without modifying existing code.
/// </summary>
public interface ICampaignStrategy
{
    /// <summary>
    /// Calculates bonus points based on campaign type.
    /// </summary>
    Points CalculateBonusPoints(Points basePoints, Money transactionAmount);

    /// <summary>
    /// Gets the campaign type this strategy handles.
    /// </summary>
    CampaignType CampaignType { get; }
}
