namespace LoyaltySphere.RewardService.Application.DTOs;

/// <summary>
/// Data Transfer Object for Campaign information.
/// Used for API responses.
/// </summary>
public record CampaignDto
{
    public Guid Id { get; init; }
    public required string CampaignName { get; init; }
    public required string Description { get; init; }
    public required string CampaignType { get; init; }
    public decimal BonusPoints { get; init; }
    public decimal? PointsMultiplier { get; init; }
    public decimal? CashbackPercentage { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required bool IsActive { get; init; }
    public string? TargetCustomerSegment { get; init; }
    public string? TargetMerchantCategory { get; init; }
    public decimal? MinimumTransactionAmount { get; init; }
    public int? MaxParticipations { get; init; }
    public int CurrentParticipations { get; init; }
    public string? Terms { get; init; }
    public required DateTime CreatedAt { get; init; }
}
