using LoyaltySphere.RewardService.Application.DTOs;
using MediatR;

namespace LoyaltySphere.RewardService.Application.Queries.GetRewardHistory;

/// <summary>
/// Query to get customer's reward transaction history.
/// </summary>
public record GetRewardHistoryQuery : IRequest<RewardHistoryResponse>
{
    public required string TenantId { get; init; }
    public required string CustomerId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? RewardType { get; init; } // Filter by type: Earned, Redeemed, Bonus, Cashback
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

/// <summary>
/// Response containing paginated reward history.
/// </summary>
public record RewardHistoryResponse
{
    public required string CustomerId { get; init; }
    public required List<RewardTransactionDto> Transactions { get; init; }
    public required int TotalCount { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required int TotalPages { get; init; }
}
