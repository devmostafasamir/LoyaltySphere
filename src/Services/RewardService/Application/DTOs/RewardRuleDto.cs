namespace LoyaltySphere.RewardService.Application.DTOs;

/// <summary>
/// Data Transfer Object for Reward Rule information.
/// Used for API responses.
/// </summary>
public record RewardRuleDto
{
    public Guid Id { get; init; }
    public required string RuleName { get; init; }
    public required string Description { get; init; }
    public required decimal PointsPerUnit { get; init; }
    public decimal? MinimumTransactionAmount { get; init; }
    public decimal? MaximumTransactionAmount { get; init; }
    public string? MerchantCategory { get; init; }
    public string? MerchantId { get; init; }
    public string? ProductCategory { get; init; }
    public required int Priority { get; init; }
    public required bool IsActive { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
    public required string RuleType { get; init; }
    public required DateTime CreatedAt { get; init; }
}
