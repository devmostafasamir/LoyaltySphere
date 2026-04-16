using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Domain.Enums;

namespace LoyaltySphere.RewardService.Domain.Strategies;

/// <summary>
/// Strategy for fixed bonus points campaigns.
/// Example: "Earn 500 bonus points on any purchase"
/// </summary>
public class BonusCampaignStrategy : ICampaignStrategy
{
    public CampaignType CampaignType => CampaignType.Bonus;

    public Points CalculateBonusPoints(Points basePoints, Money transactionAmount)
    {
        // Fixed bonus - typically configured in campaign
        // For now, return a percentage of base points
        return basePoints.Multiply(0.20m); // 20% bonus
    }
}
