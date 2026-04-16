namespace LoyaltySphere.RewardService.Api.Contracts.RewardRules;

/// <summary>
/// Request to create a new reward rule.
/// </summary>
public record CreateRewardRuleRequest
{
    public required string RuleName { get; init; }
    public required string Description { get; init; }
    public required decimal PointsPerUnit { get; init; }
    public string? RuleType { get; init; }
    public int Priority { get; init; }
    public decimal? MinimumTransactionAmount { get; init; }
    public decimal? MaximumTransactionAmount { get; init; }
    public string? MerchantCategory { get; init; }
    public string? MerchantId { get; init; }
    public string? ProductCategory { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
}
