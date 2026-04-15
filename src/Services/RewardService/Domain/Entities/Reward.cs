using LoyaltySphere.Common.Domain;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Domain.Events;

namespace LoyaltySphere.RewardService.Domain.Entities;

/// <summary>
/// Represents a reward transaction in the loyalty program.
/// Records points awarded or redeemed for a specific customer.
/// </summary>
public class Reward : Entity
{
    public Guid CustomerId { get; private set; }
    public string CustomerExternalId { get; private set; } = string.Empty;
    public Points PointsAwarded { get; private set; } = Points.Zero;
    public Money TransactionAmount { get; private set; } = Money.Zero();
    public string RewardType { get; private set; } = string.Empty; // Earned, Redeemed, Bonus, Cashback
    public string Source { get; private set; } = string.Empty; // POS, Online, Campaign, Manual
    public string? TransactionId { get; private set; }
    public string? CampaignId { get; private set; }
    public string? MerchantId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime ProcessedAt { get; private set; }
    public bool IsProcessed { get; private set; }
    public string? ProcessingError { get; private set; }

    // Navigation properties
    public Customer? Customer { get; private set; }

    private Reward() { } // EF Core

    private Reward(
        string tenantId,
        Guid customerId,
        string customerExternalId,
        Points pointsAwarded,
        Money transactionAmount,
        string rewardType,
        string source,
        string description,
        string? transactionId = null,
        string? campaignId = null,
        string? merchantId = null)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        CustomerExternalId = customerExternalId;
        PointsAwarded = pointsAwarded;
        TransactionAmount = transactionAmount;
        RewardType = rewardType;
        Source = source;
        Description = description;
        TransactionId = transactionId;
        CampaignId = campaignId;
        MerchantId = merchantId;
        ProcessedAt = DateTime.UtcNow;
        IsProcessed = false;
    }

    public static Reward CreateEarned(
        string tenantId,
        Guid customerId,
        string customerExternalId,
        Points pointsAwarded,
        Money transactionAmount,
        string source,
        string description,
        string? transactionId = null,
        string? merchantId = null)
    {
        ValidateCreationParameters(tenantId, customerId, customerExternalId, pointsAwarded, source, description);

        var reward = new Reward(
            tenantId,
            customerId,
            customerExternalId,
            pointsAwarded,
            transactionAmount,
            "Earned",
            source,
            description,
            transactionId,
            null,
            merchantId);

        reward.AddDomainEvent(new RewardCalculatedEvent(
            reward.Id,
            tenantId,
            customerId,
            customerExternalId,
            pointsAwarded,
            transactionAmount,
            source,
            transactionId));

        return reward;
    }

    public static Reward CreateRedeemed(
        string tenantId,
        Guid customerId,
        string customerExternalId,
        Points pointsRedeemed,
        string description)
    {
        ValidateCreationParameters(tenantId, customerId, customerExternalId, pointsRedeemed, "Redemption", description);

        var reward = new Reward(
            tenantId,
            customerId,
            customerExternalId,
            pointsRedeemed.Multiply(-1), // Negative for redemption
            Money.Zero(),
            "Redeemed",
            "Redemption",
            description);

        return reward;
    }

    public static Reward CreateBonus(
        string tenantId,
        Guid customerId,
        string customerExternalId,
        Points bonusPoints,
        string description,
        string? campaignId = null)
    {
        ValidateCreationParameters(tenantId, customerId, customerExternalId, bonusPoints, "Campaign", description);

        var reward = new Reward(
            tenantId,
            customerId,
            customerExternalId,
            bonusPoints,
            Money.Zero(),
            "Bonus",
            "Campaign",
            description,
            null,
            campaignId);

        return reward;
    }

    public static Reward CreateCashback(
        string tenantId,
        Guid customerId,
        string customerExternalId,
        Points cashbackPoints,
        Money transactionAmount,
        string description,
        string? transactionId = null)
    {
        ValidateCreationParameters(tenantId, customerId, customerExternalId, cashbackPoints, "Cashback", description);

        var reward = new Reward(
            tenantId,
            customerId,
            customerExternalId,
            cashbackPoints,
            transactionAmount,
            "Cashback",
            "POS",
            description,
            transactionId);

        return reward;
    }

    private static void ValidateCreationParameters(
        string tenantId,
        Guid customerId,
        string customerExternalId,
        Points points,
        string source,
        string description)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID is required", nameof(tenantId));

        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        if (string.IsNullOrWhiteSpace(customerExternalId))
            throw new ArgumentException("Customer external ID is required", nameof(customerExternalId));

        if (points.IsZero)
            throw new ArgumentException("Points cannot be zero", nameof(points));

        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source is required", nameof(source));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));
    }

    /// <summary>
    /// Marks the reward as successfully processed
    /// </summary>
    public void MarkAsProcessed()
    {
        if (IsProcessed)
            throw new InvalidOperationException("Reward has already been processed");

        IsProcessed = true;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();

        AddDomainEvent(new RewardProcessedEvent(
            Id,
            TenantId,
            CustomerId,
            CustomerExternalId,
            PointsAwarded,
            RewardType));
    }

    /// <summary>
    /// Marks the reward as failed with an error message
    /// </summary>
    public void MarkAsFailed(string error)
    {
        if (IsProcessed)
            throw new InvalidOperationException("Cannot mark processed reward as failed");

        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error message is required", nameof(error));

        ProcessingError = error;
        MarkAsUpdated();

        AddDomainEvent(new RewardProcessingFailedEvent(
            Id,
            TenantId,
            CustomerId,
            CustomerExternalId,
            error));
    }

    public bool IsEarned => RewardType == "Earned";
    public bool IsRedeemed => RewardType == "Redeemed";
    public bool IsBonus => RewardType == "Bonus";
    public bool IsCashback => RewardType == "Cashback";
}
