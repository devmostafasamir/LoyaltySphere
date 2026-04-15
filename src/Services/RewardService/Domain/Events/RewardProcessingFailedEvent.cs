using LoyaltySphere.Common.Domain;

namespace LoyaltySphere.RewardService.Domain.Events;

/// <summary>
/// Domain event raised when reward processing fails.
/// </summary>
public sealed record RewardProcessingFailedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public Guid RewardId { get; }
    public string TenantId { get; }
    public Guid CustomerId { get; }
    public string CustomerExternalId { get; }
    public string Error { get; }

    public RewardProcessingFailedEvent(
        Guid rewardId,
        string tenantId,
        Guid customerId,
        string customerExternalId,
        string error)
    {
        RewardId = rewardId;
        TenantId = tenantId;
        CustomerId = customerId;
        CustomerExternalId = customerExternalId;
        Error = error;
    }
}
