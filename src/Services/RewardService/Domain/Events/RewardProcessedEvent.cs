using LoyaltySphere.Common.Domain;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Events;

/// <summary>
/// Domain event raised when a reward has been successfully processed.
/// </summary>
public sealed record RewardProcessedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public Guid RewardId { get; }
    public string TenantId { get; }
    public Guid CustomerId { get; }
    public string CustomerExternalId { get; }
    public Points PointsAwarded { get; }
    public string RewardType { get; }

    public RewardProcessedEvent(
        Guid rewardId,
        string tenantId,
        Guid customerId,
        string customerExternalId,
        Points pointsAwarded,
        string rewardType)
    {
        RewardId = rewardId;
        TenantId = tenantId;
        CustomerId = customerId;
        CustomerExternalId = customerExternalId;
        PointsAwarded = pointsAwarded;
        RewardType = rewardType;
    }
}
