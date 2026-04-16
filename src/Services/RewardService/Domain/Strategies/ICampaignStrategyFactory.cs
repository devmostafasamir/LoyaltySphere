using LoyaltySphere.RewardService.Domain.Enums;

namespace LoyaltySphere.RewardService.Domain.Strategies;

/// <summary>
/// Factory for creating campaign strategies.
/// Follows Factory Pattern for strategy instantiation.
/// </summary>
public interface ICampaignStrategyFactory
{
    /// <summary>
    /// Gets the appropriate strategy for a campaign type.
    /// </summary>
    ICampaignStrategy GetStrategy(CampaignType campaignType);
}
