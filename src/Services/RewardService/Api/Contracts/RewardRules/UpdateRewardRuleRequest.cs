namespace LoyaltySphere.RewardService.Api.Contracts.RewardRules;

/// <summary>
/// Request to update an existing reward rule.
/// </summary>
public record UpdateRewardRuleRequest
{
    public required string RuleName { get; init; }
    public required string Description { get; init; }
    public required decimal PointsPerUnit { get; init; }
    public int Priority { get; init; }
    public decimal? MinimumTransactionAmount { get; init; }
    public decimal? MaximumTransactionAmount { get; init; }
}
