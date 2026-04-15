using LoyaltySphere.Common.Domain;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Entities;

/// <summary>
/// Represents a marketing campaign that offers bonus points or special rewards.
/// Campaigns can be time-limited and target specific customer segments.
/// </summary>
public class Campaign : Entity
{
    public string CampaignName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string CampaignType { get; private set; } = "Bonus"; // Bonus, Multiplier, Cashback, Referral
    public int Priority { get; private set; } = 0; // Higher priority campaigns are evaluated first
    public decimal BonusPoints { get; private set; }
    public decimal? PointsMultiplier { get; private set; }
    public decimal? CashbackPercentage { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? TargetCustomerSegment { get; private set; } // Bronze, Silver, Gold, Platinum, All
    public string? TargetMerchantCategory { get; private set; }
    public decimal? MinimumTransactionAmount { get; private set; }
    public int? MaxParticipations { get; private set; }
    public int CurrentParticipations { get; private set; }
    public string? Terms { get; private set; }

    private Campaign() { } // EF Core

    private Campaign(
        string tenantId,
        string campaignName,
        string description,
        string campaignType,
        DateTime startDate,
        DateTime endDate,
        int priority = 0,
        decimal bonusPoints = 0,
        decimal? pointsMultiplier = null,
        decimal? cashbackPercentage = null,
        string? targetCustomerSegment = null,
        string? targetMerchantCategory = null,
        decimal? minimumTransactionAmount = null,
        int? maxParticipations = null,
        string? terms = null)
    {
        TenantId = tenantId;
        CampaignName = campaignName;
        Description = description;
        CampaignType = campaignType;
        Priority = priority;
        StartDate = startDate;
        EndDate = endDate;
        BonusPoints = bonusPoints;
        PointsMultiplier = pointsMultiplier;
        CashbackPercentage = cashbackPercentage;
        TargetCustomerSegment = targetCustomerSegment;
        TargetMerchantCategory = targetMerchantCategory;
        MinimumTransactionAmount = minimumTransactionAmount;
        MaxParticipations = maxParticipations;
        CurrentParticipations = 0;
        Terms = terms;
        IsActive = true;
    }

    public static Campaign CreateBonusCampaign(
        string tenantId,
        string campaignName,
        string description,
        decimal bonusPoints,
        DateTime startDate,
        DateTime endDate,
        string? targetCustomerSegment = null,
        decimal? minimumTransactionAmount = null,
        int? maxParticipations = null,
        string? terms = null)
    {
        ValidateCommonParameters(tenantId, campaignName, startDate, endDate);

        if (bonusPoints <= 0)
            throw new ArgumentException("Bonus points must be positive", nameof(bonusPoints));

        return new Campaign(
            tenantId,
            campaignName,
            description,
            "Bonus",
            startDate,
            endDate,
            bonusPoints: bonusPoints,
            targetCustomerSegment: targetCustomerSegment,
            minimumTransactionAmount: minimumTransactionAmount,
            maxParticipations: maxParticipations,
            terms: terms);
    }

    public static Campaign CreateMultiplierCampaign(
        string tenantId,
        string campaignName,
        string description,
        decimal pointsMultiplier,
        DateTime startDate,
        DateTime endDate,
        string? targetMerchantCategory = null,
        decimal? minimumTransactionAmount = null,
        string? terms = null)
    {
        ValidateCommonParameters(tenantId, campaignName, startDate, endDate);

        if (pointsMultiplier <= 1)
            throw new ArgumentException("Points multiplier must be greater than 1", nameof(pointsMultiplier));

        return new Campaign(
            tenantId,
            campaignName,
            description,
            "Multiplier",
            startDate,
            endDate,
            pointsMultiplier: pointsMultiplier,
            targetMerchantCategory: targetMerchantCategory,
            minimumTransactionAmount: minimumTransactionAmount,
            terms: terms);
    }

    public static Campaign CreateCashbackCampaign(
        string tenantId,
        string campaignName,
        string description,
        decimal cashbackPercentage,
        DateTime startDate,
        DateTime endDate,
        string? targetMerchantCategory = null,
        decimal? minimumTransactionAmount = null,
        string? terms = null)
    {
        ValidateCommonParameters(tenantId, campaignName, startDate, endDate);

        if (cashbackPercentage <= 0 || cashbackPercentage > 100)
            throw new ArgumentException("Cashback percentage must be between 0 and 100", nameof(cashbackPercentage));

        return new Campaign(
            tenantId,
            campaignName,
            description,
            "Cashback",
            startDate,
            endDate,
            cashbackPercentage: cashbackPercentage,
            targetMerchantCategory: targetMerchantCategory,
            minimumTransactionAmount: minimumTransactionAmount,
            terms: terms);
    }

    private static void ValidateCommonParameters(
        string tenantId,
        string campaignName,
        DateTime startDate,
        DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID is required", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(campaignName))
            throw new ArgumentException("Campaign name is required", nameof(campaignName));

        if (startDate >= endDate)
            throw new ArgumentException("Start date must be before end date");
    }

    /// <summary>
    /// Checks if the campaign is currently active and valid
    /// </summary>
    public bool IsCurrentlyActive()
    {
        if (!IsActive)
            return false;

        var now = DateTime.UtcNow;
        if (now < StartDate || now > EndDate)
            return false;

        if (MaxParticipations.HasValue && CurrentParticipations >= MaxParticipations.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if a customer is eligible for this campaign
    /// </summary>
    public bool IsCustomerEligible(string customerTier, Money transactionAmount, string? merchantCategory = null)
    {
        if (!IsCurrentlyActive())
            return false;

        // Check customer segment
        if (!string.IsNullOrEmpty(TargetCustomerSegment) && 
            TargetCustomerSegment != "All" && 
            TargetCustomerSegment != customerTier)
            return false;

        // Check minimum transaction amount
        if (MinimumTransactionAmount.HasValue && transactionAmount.Amount < MinimumTransactionAmount.Value)
            return false;

        // Check merchant category
        if (!string.IsNullOrEmpty(TargetMerchantCategory) && TargetMerchantCategory != merchantCategory)
            return false;

        return true;
    }

    /// <summary>
    /// Calculates bonus points for this campaign
    /// </summary>
    public Points CalculateBonusPoints(Points basePoints, Money transactionAmount)
    {
        if (!IsCurrentlyActive())
            throw new InvalidOperationException("Campaign is not active");

        return CampaignType switch
        {
            "Bonus" => Points.Create(BonusPoints),
            "Multiplier" => basePoints.Multiply(PointsMultiplier ?? 1),
            "Cashback" => Points.Create(transactionAmount.Amount * (CashbackPercentage ?? 0) / 100),
            _ => Points.Zero
        };
    }

    /// <summary>
    /// Records a participation in the campaign
    /// </summary>
    public void RecordParticipation()
    {
        if (!IsCurrentlyActive())
            throw new InvalidOperationException("Cannot record participation for inactive campaign");

        if (MaxParticipations.HasValue && CurrentParticipations >= MaxParticipations.Value)
            throw new InvalidOperationException("Campaign has reached maximum participations");

        CurrentParticipations++;
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the campaign
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Campaign is already active");

        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the campaign
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Campaign is already inactive");

        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Extends the campaign end date
    /// </summary>
    public void ExtendEndDate(DateTime newEndDate)
    {
        if (newEndDate <= EndDate)
            throw new ArgumentException("New end date must be after current end date", nameof(newEndDate));

        EndDate = newEndDate;
        MarkAsUpdated();
    }
}
