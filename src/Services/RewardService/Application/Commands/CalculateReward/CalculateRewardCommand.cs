using LoyaltySphere.RewardService.Domain.ValueObjects;
using MediatR;

namespace LoyaltySphere.RewardService.Application.Commands.CalculateReward;

/// <summary>
/// Command to calculate and award reward points for a transaction.
/// This is triggered when a POS transaction occurs.
/// </summary>
public record CalculateRewardCommand : IRequest<CalculateRewardResponse>
{
    public required string TenantId { get; init; }
    public required string CustomerId { get; init; }
    public required decimal TransactionAmount { get; init; }
    public string Currency { get; init; } = "EGP";
    public string? TransactionId { get; init; }
    public string? MerchantId { get; init; }
    public string? MerchantCategory { get; init; }
    public string? ProductCategory { get; init; }
    public DateTime TransactionDate { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Response from reward calculation.
/// </summary>
public record CalculateRewardResponse
{
    public required Guid RewardId { get; init; }
    public required string CustomerId { get; init; }
    public required decimal PointsAwarded { get; init; }
    public required decimal BasePoints { get; init; }
    public required decimal CampaignBonus { get; init; }
    public required decimal NewBalance { get; init; }
    public required string CustomerTier { get; init; }
    public string? AppliedRuleName { get; init; }
    public string? AppliedCampaignName { get; init; }
    public required decimal TierMultiplier { get; init; }
    public required DateTime ProcessedAt { get; init; }
}
