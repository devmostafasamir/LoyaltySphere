using LoyaltySphere.Common.Domain;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Events;

/// <summary>
/// Domain event raised when a customer's tier is upgraded.
/// Triggers celebration animations and notifications.
/// </summary>
public sealed record CustomerTierUpgradedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public Guid CustomerId { get; }
    public string TenantId { get; }
    public string CustomerExternalId { get; }
    public string PreviousTier { get; }
    public string NewTier { get; }
    public Points LifetimePoints { get; }

    public CustomerTierUpgradedEvent(
        Guid customerId,
        string tenantId,
        string customerExternalId,
        string previousTier,
        string newTier,
        Points lifetimePoints)
    {
        CustomerId = customerId;
        TenantId = tenantId;
        CustomerExternalId = customerExternalId;
        PreviousTier = previousTier;
        NewTier = newTier;
        LifetimePoints = lifetimePoints;
    }
}
