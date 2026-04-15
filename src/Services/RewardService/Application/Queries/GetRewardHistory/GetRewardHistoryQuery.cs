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

/// <summary>
/// DTO for a single reward transaction.
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
