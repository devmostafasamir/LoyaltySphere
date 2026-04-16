using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Services;

/// <summary>
/// Domain service for selecting the best reward rule.
/// Encapsulates rule selection logic.
/// </summary>
public interface IRewardRuleSelector
{
    /// <summary>
    /// Selects the best matching reward rule based on priority and applicability.
    /// </summary>
    RewardRule? SelectBestRule(
        IEnumerable<RewardRule> rules,
        Money transactionAmount,
        string? merchantId = null,
        string? merchantCategory = null);

    /// <summary>
    /// Gets all applicable rules for a transaction.
    /// </summary>
    IEnumerable<RewardRule> GetApplicableRules(
        IEnumerable<RewardRule> rules,
        Money transactionAmount,
        string? merchantId = null,
        string? merchantCategory = null);
}
