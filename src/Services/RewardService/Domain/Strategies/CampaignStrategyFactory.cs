using LoyaltySphere.RewardService.Domain.Enums;

namespace LoyaltySphere.RewardService.Domain.Strategies;

/// <summary>
/// Factory implementation for creating campaign strategies.
/// Centralizes strategy creation logic.
/// </summary>
public class CampaignStrategyFactory : ICampaignStrategyFactory
{
    private readonly Dictionary<CampaignType, ICampaignStrategy> _strategies;

    public CampaignStrategyFactory()
    {
        _strategies = new Dictionary<CampaignType, ICampaignStrategy>
        {
            { CampaignType.Bonus, new BonusCampaignStrategy() },
            { CampaignType.Multiplier, new MultiplierCampaignStrategy() },
            { CampaignType.Cashback, new CashbackCampaignStrategy() },
            { CampaignType.Tiered, new TieredCampaignStrategy() }
        };
    }

    public ICampaignStrategy GetStrategy(CampaignType campaignType)
    {
        if (_strategies.TryGetValue(campaignType, out var strategy))
        {
            return strategy;
        }

        // Default to bonus strategy if type not found
        return _strategies[CampaignType.Bonus];
    }
}
