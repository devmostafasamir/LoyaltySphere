using LoyaltySphere.Common.Domain;

namespace LoyaltySphere.RewardService.Domain.Events;

/// <summary>
/// Domain event raised when a new customer is enrolled in the loyalty program.
/// </summary>
public sealed record CustomerEnrolledEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public Guid CustomerId { get; }
    public string TenantId { get; }
    public string CustomerExternalId { get; }
    public string Email { get; }

    public CustomerEnrolledEvent(
        Guid customerId,
        string tenantId,
        string customerExternalId,
        string email)
    {
        CustomerId = customerId;
        TenantId = tenantId;
        CustomerExternalId = customerExternalId;
        Email = email;
    }
}
