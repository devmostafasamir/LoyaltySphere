using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Services;

/// <summary>
/// Domain service for checking campaign eligibility.
/// Encapsulates campaign eligibility rules.
/// </summary>
public interface ICampaignEligibilityChecker
{
    /// <summary>
    /// Checks if a customer is eligible for a specific campaign.
    /// </summary>
    bool IsCustomerEligible(
        Campaign campaign,
        Customer customer,
        Money transactionAmount);

    /// <summary>
    /// Gets all eligible campaigns for a customer and transaction.
    /// </summary>
    IEnumerable<Campaign> GetEligibleCampaigns(
        IEnumerable<Campaign> campaigns,
        Customer customer,
        Money transactionAmount,
        string? merchantCategory = null);
}
