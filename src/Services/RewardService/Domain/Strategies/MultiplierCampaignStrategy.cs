using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Domain.Enums;

namespace LoyaltySphere.RewardService.Domain.Strategies;

/// <summary>
/// Strategy for multiplier campaigns.
/// Example: "2x points on all purchases this weekend"
/// </summary>
public class MultiplierCampaignStrategy : ICampaignStrategy
{
    public CampaignType CampaignType => CampaignType.Multiplier;

    public Points CalculateBonusPoints(Points basePoints, Money transactionAmount)
    {
        // Double the base points (2x multiplier)
        return basePoints; // Returns same as base, effectively doubling total
    }
}
