using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Services;

/// <summary>
/// Implementation of campaign eligibility checking logic.
/// Follows Single Responsibility Principle - only handles eligibility checks.
/// </summary>
public class CampaignEligibilityChecker : ICampaignEligibilityChecker
{
    public bool IsCustomerEligible(
        Campaign campaign,
        Customer customer,
        Money transactionAmount)
    {
        return campaign.IsCustomerEligible(
            customer.Tier,
            transactionAmount,
            null); // merchantCategory handled separately
    }

    public IEnumerable<Campaign> GetEligibleCampaigns(
        IEnumerable<Campaign> campaigns,
        Customer customer,
        Money transactionAmount,
        string? merchantCategory = null)
    {
        return campaigns
            .Where(c => c.IsCustomerEligible(customer.Tier, transactionAmount, merchantCategory))
            .OrderByDescending(c => c.Priority);
    }
}
