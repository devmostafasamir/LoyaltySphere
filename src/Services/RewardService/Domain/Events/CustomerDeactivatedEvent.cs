using LoyaltySphere.Common.Domain;

namespace LoyaltySphere.RewardService.Domain.Events;

/// <summary>
/// Domain event raised when a customer account is deactivated.
/// </summary>
public sealed record CustomerDeactivatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public Guid CustomerId { get; }
    public string TenantId { get; }
    public string CustomerExternalId { get; }

    public CustomerDeactivatedEvent(
        Guid customerId,
        string tenantId,
        string customerExternalId)
    {
        CustomerId = customerId;
        TenantId = tenantId;
        CustomerExternalId = customerExternalId;
    }
}
