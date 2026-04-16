namespace LoyaltySphere.RewardService.Api.Contracts.Campaigns;

/// <summary>
/// Request to create a new campaign.
/// </summary>
public record CreateCampaignRequest
{
    public required string CampaignName { get; init; }
    public required string Description { get; init; }
    public required string CampaignType { get; init; } // Bonus, Multiplier, Cashback
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public decimal? BonusPoints { get; init; }
    public decimal? PointsMultiplier { get; init; }
    public decimal? CashbackPercentage { get; init; }
    public string? TargetCustomerSegment { get; init; }
    public string? TargetMerchantCategory { get; init; }
    public decimal? MinimumTransactionAmount { get; init; }
    public int? MaxParticipations { get; init; }
    public string? Terms { get; init; }
}
