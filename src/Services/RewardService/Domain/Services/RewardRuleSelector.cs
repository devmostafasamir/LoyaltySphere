using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Services;

/// <summary>
/// Implementation of reward rule selection logic.
/// Follows Single Responsibility Principle - only handles rule selection.
/// </summary>
public class RewardRuleSelector : IRewardRuleSelector
{
    public RewardRule? SelectBestRule(
        IEnumerable<RewardRule> rules,
        Money transactionAmount,
        string? merchantId = null,
        string? merchantCategory = null)
    {
        return GetApplicableRules(rules, transactionAmount, merchantId, merchantCategory)
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.PointsPerUnit)
            .FirstOrDefault();
    }

    public IEnumerable<RewardRule> GetApplicableRules(
        IEnumerable<RewardRule> rules,
        Money transactionAmount,
        string? merchantId = null,
        string? merchantCategory = null)
    {
        return rules.Where(r => r.AppliesTo(transactionAmount, merchantId, merchantCategory));
    }
}
