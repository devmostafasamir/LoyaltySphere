using LoyaltySphere.Common.Domain;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Events;

/// <summary>
/// Domain event raised when a reward is calculated for a transaction.
/// This event triggers real-time notifications and updates.
/// </summary>
public sealed record RewardCalculatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public Guid RewardId { get; }
    public string TenantId { get; }
    public Guid CustomerId { get; }
    public string CustomerExternalId { get; }
    public Points PointsAwarded { get; }
    public Money TransactionAmount { get; }
    public string Source { get; }
    public string? TransactionId { get; }

    public RewardCalculatedEvent(
        Guid rewardId,
        string tenantId,
        Guid customerId,
        string customerExternalId,
        Points pointsAwarded,
        Money transactionAmount,
        string source,
        string? transactionId)
    {
        RewardId = rewardId;
        TenantId = tenantId;
        CustomerId = customerId;
        CustomerExternalId = customerExternalId;
        PointsAwarded = pointsAwarded;
        TransactionAmount = transactionAmount;
        Source = source;
        TransactionId = transactionId;
    }
}
