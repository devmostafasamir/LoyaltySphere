using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Domain.Enums;

namespace LoyaltySphere.RewardService.Domain.Strategies;

/// <summary>
/// Strategy for cashback campaigns.
/// Example: "5% cashback on all purchases"
/// </summary>
public class CashbackCampaignStrategy : ICampaignStrategy
{
    public CampaignType CampaignType => CampaignType.Cashback;

    public Points CalculateBonusPoints(Points basePoints, Money transactionAmount)
    {
        // 5% of transaction amount as cashback points
        var cashbackPercentage = 0.05m;
        var cashbackAmount = transactionAmount.Amount * cashbackPercentage;
        return Points.Create(cashbackAmount);
    }
}
