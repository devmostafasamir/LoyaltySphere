namespace LoyaltySphere.RewardService.Application.DTOs;

/// <summary>
/// Data Transfer Object for dashboard analytics.
/// </summary>
public record DashboardAnalyticsDto
{
    public required int TotalCustomers { get; init; }
    public required int ActiveCustomers { get; init; }
    public required decimal TotalPointsAwarded { get; init; }
    public required decimal TotalPointsRedeemed { get; init; }
    public required int ActiveCampaigns { get; init; }
    public required List<TierDistributionDto> TierDistribution { get; init; }
    public required List<DailyTransactionDto> RecentTransactions { get; init; }
    public required DateTime FromDate { get; init; }
    public required DateTime ToDate { get; init; }
}

/// <summary>
/// Customer tier distribution data.
/// </summary>
public record TierDistributionDto
{
    public required string Tier { get; init; }
    public required int Count { get; init; }
    public required decimal TotalPoints { get; init; }
}

/// <summary>
/// Daily transaction summary data.
/// </summary>
public record DailyTransactionDto
{
    public required DateTime Date { get; init; }
    public required int Count { get; init; }
    public required decimal TotalPoints { get; init; }
}
