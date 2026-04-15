using LoyaltySphere.Common.Domain;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Events;

/// <summary>
/// Domain event raised when points are redeemed by a customer.
/// </summary>
public sealed record PointsRedeemedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public Guid CustomerId { get; }
    public string TenantId { get; }
    public string CustomerExternalId { get; }
    public Points PointsRedeemed { get; }
    public Points PreviousBalance { get; }
    public Points NewBalance { get; }
    public string Reason { get; }

    public PointsRedeemedEvent(
        Guid customerId,
        string tenantId,
        string customerExternalId,
        Points pointsRedeemed,
        Points previousBalance,
        Points newBalance,
        string reason)
    {
        CustomerId = customerId;
        TenantId = tenantId;
        CustomerExternalId = customerExternalId;
        PointsRedeemed = pointsRedeemed;
        PreviousBalance = previousBalance;
        NewBalance = newBalance;
        Reason = reason;
    }
}
