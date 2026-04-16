using LoyaltySphere.RewardService.Application.DTOs;
using LoyaltySphere.RewardService.Domain.Entities;

namespace LoyaltySphere.RewardService.Application.Mappers;

/// <summary>
/// Mapper for Reward entity to DTOs.
/// Follows Single Responsibility Principle - only handles Reward mapping.
/// </summary>
public static class RewardMapper
{
    /// <summary>
    /// Maps Reward entity to RewardTransactionDto.
    /// </summary>
    public static RewardTransactionDto ToTransactionDto(Reward reward)
    {
        return new RewardTransactionDto
        {
            Id = reward.Id,
            Points = reward.PointsAwarded.Value,
            TransactionAmount = reward.TransactionAmount.Amount,
            RewardType = reward.RewardType.ToString(),
            Source = reward.Source,
            Description = reward.Description,
            TransactionId = reward.TransactionId,
            CampaignId = reward.CampaignId,
            ProcessedAt = reward.ProcessedAt
        };
    }
}
