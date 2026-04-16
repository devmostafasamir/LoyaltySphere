namespace LoyaltySphere.RewardService.Api.Contracts.Rewards;

/// <summary>
/// Request to redeem loyalty points.
/// </summary>
public record RedeemPointsRequest
{
    public required string CustomerId { get; init; }
    public required decimal PointsToRedeem { get; init; }
    public required string RedemptionType { get; init; }
    public string? RedemptionDetails { get; init; }
}
