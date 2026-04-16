using LoyaltySphere.RewardService.Application.DTOs;
using LoyaltySphere.RewardService.Domain.Entities;

namespace LoyaltySphere.RewardService.Application.Mappers;

/// <summary>
/// Mapper for RewardRule entity to DTOs.
/// Follows Single Responsibility Principle - only handles RewardRule mapping.
/// </summary>
public static class RewardRuleMapper
{
    /// <summary>
    /// Maps RewardRule entity to RewardRuleDto.
    /// </summary>
    public static RewardRuleDto ToDto(RewardRule rule)
    {
        return new RewardRuleDto
        {
            Id = rule.Id,
            RuleName = rule.RuleName,
            Description = rule.Description,
            PointsPerUnit = rule.PointsPerUnit,
            MinimumTransactionAmount = rule.MinimumTransactionAmount,
            MaximumTransactionAmount = rule.MaximumTransactionAmount,
            MerchantCategory = rule.MerchantCategory,
            MerchantId = rule.MerchantId,
            ProductCategory = rule.ProductCategory,
            Priority = rule.Priority,
            IsActive = rule.IsActive,
            ValidFrom = rule.ValidFrom,
            ValidUntil = rule.ValidUntil,
            RuleType = rule.RuleType,
            CreatedAt = rule.CreatedAt
        };
    }
}
