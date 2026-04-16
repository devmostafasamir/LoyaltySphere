using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Domain.Enums;

namespace LoyaltySphere.RewardService.Domain.Strategies;

/// <summary>
/// Strategy for tiered campaigns.
/// Example: "Bronze: 10% bonus, Silver: 20% bonus, Gold: 30% bonus"
/// </summary>
public class TieredCampaignStrategy : ICampaignStrategy
{
    public CampaignType CampaignType => CampaignType.Tiered;

    public Points CalculateBonusPoints(Points basePoints, Money transactionAmount)
    {
        // Tiered bonus based on transaction amount
        if (transactionAmount.Amount >= 1000m)
            return basePoints.Multiply(0.30m); // 30% bonus for large transactions
        if (transactionAmount.Amount >= 500m)
            return basePoints.Multiply(0.20m); // 20% bonus for medium transactions
        if (transactionAmount.Amount >= 100m)
            return basePoints.Multiply(0.10m); // 10% bonus for small transactions

        return Points.Zero;
    }
}
