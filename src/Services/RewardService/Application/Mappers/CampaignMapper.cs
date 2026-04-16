using LoyaltySphere.RewardService.Application.DTOs;
using LoyaltySphere.RewardService.Domain.Entities;

namespace LoyaltySphere.RewardService.Application.Mappers;

/// <summary>
/// Mapper for Campaign entity to DTOs.
/// Follows Single Responsibility Principle - only handles Campaign mapping.
/// </summary>
public static class CampaignMapper
{
    /// <summary>
    /// Maps Campaign entity to CampaignDto.
    /// </summary>
    public static CampaignDto ToDto(Campaign campaign)
    {
        return new CampaignDto
        {
            Id = campaign.Id,
            CampaignName = campaign.CampaignName,
            Description = campaign.Description,
            CampaignType = campaign.CampaignType.ToString(),
            BonusPoints = campaign.BonusPoints,
            PointsMultiplier = campaign.PointsMultiplier,
            CashbackPercentage = campaign.CashbackPercentage,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            IsActive = campaign.IsActive,
            TargetCustomerSegment = campaign.TargetCustomerSegment,
            TargetMerchantCategory = campaign.TargetMerchantCategory,
            MinimumTransactionAmount = campaign.MinimumTransactionAmount,
            MaxParticipations = campaign.MaxParticipations,
            CurrentParticipations = campaign.CurrentParticipations,
            Terms = campaign.Terms,
            CreatedAt = campaign.CreatedAt
        };
    }
}
