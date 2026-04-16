namespace LoyaltySphere.RewardService.Api.Contracts.Rewards;

/// <summary>
/// Request to calculate reward for a transaction.
/// </summary>
public record CalculateRewardRequest
{
    public required string CustomerId { get; init; }
    public required decimal TransactionAmount { get; init; }
    public string? Currency { get; init; }
    public string? TransactionId { get; init; }
    public string? MerchantId { get; init; }
    public string? MerchantCategory { get; init; }
    public string? ProductCategory { get; init; }
    public DateTime? TransactionDate { get; init; }
}
