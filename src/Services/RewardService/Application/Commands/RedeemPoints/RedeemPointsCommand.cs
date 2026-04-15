using MediatR;

namespace LoyaltySphere.RewardService.Application.Commands.RedeemPoints;

/// <summary>
/// Command to redeem loyalty points.
/// </summary>
public record RedeemPointsCommand : IRequest<RedeemPointsResponse>
{
    public required string TenantId { get; init; }
    public required string CustomerId { get; init; }
    public required decimal PointsToRedeem { get; init; }
    public required string RedemptionType { get; init; } // Voucher, Cashback, Product, Discount
    public string? RedemptionDetails { get; init; }
}

/// <summary>
/// Response from points redemption.
/// </summary>
public record RedeemPointsResponse
{
    public required Guid RedemptionId { get; init; }
    public required string CustomerId { get; init; }
    public required decimal PointsRedeemed { get; init; }
    public required decimal RemainingBalance { get; init; }
    public required string RedemptionType { get; init; }
    public required DateTime RedeemedAt { get; init; }
}
