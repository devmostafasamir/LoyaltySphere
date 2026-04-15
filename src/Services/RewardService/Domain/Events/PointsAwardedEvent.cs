using LoyaltySphere.Common.Domain;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Events;

/// <summary>
/// Domain event raised when points are awarded to a customer.
/// Triggers real-time UI updates via SignalR.
/// </summary>
public sealed record PointsAwardedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    public Guid CustomerId { get; }
    public string TenantId { get; }
    public string CustomerExternalId { get; }
    public Points PointsAwarded { get; }
    public Points PreviousBalance { get; }
    public Points NewBalance { get; }
    public string Reason { get; }
    public string? TransactionId { get; }

    public PointsAwardedEvent(
        Guid customerId,
        string tenantId,
        string customerExternalId,
        Points pointsAwarded,
        Points previousBalance,
        Points newBalance,
        string reason,
        string? transactionId)
    {
        CustomerId = customerId;
        TenantId = tenantId;
        CustomerExternalId = customerExternalId;
        PointsAwarded = pointsAwarded;
        PreviousBalance = previousBalance;
        NewBalance = newBalance;
        Reason = reason;
        TransactionId = transactionId;
    }
}
