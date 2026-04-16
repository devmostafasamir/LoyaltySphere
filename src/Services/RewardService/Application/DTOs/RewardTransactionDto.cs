namespace LoyaltySphere.RewardService.Application.DTOs;

/// <summary>
/// Data Transfer Object for a single reward transaction.
/// Used in reward history responses.
/// </summary>
public record RewardTransactionDto
{
    public required Guid Id { get; init; }
    public required decimal Points { get; init; }
    public required decimal TransactionAmount { get; init; }
    public required string RewardType { get; init; }
    public required string Source { get; init; }
    public required string Description { get; init; }
    public string? TransactionId { get; init; }
    public string? CampaignId { get; init; }
    public required DateTime ProcessedAt { get; init; }
}
