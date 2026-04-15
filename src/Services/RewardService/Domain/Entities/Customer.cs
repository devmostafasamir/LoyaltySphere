using LoyaltySphere.Common.Domain;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Domain.Events;

namespace LoyaltySphere.RewardService.Domain.Entities;

/// <summary>
/// Represents a customer in the loyalty program.
/// Aggregate root for customer-related operations.
/// </summary>
public class Customer : Entity
{
    public string CustomerId { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public Points PointsBalance { get; private set; } = Points.Zero;
    public Points LifetimePoints { get; private set; } = Points.Zero;
    public DateTime EnrolledAt { get; private set; }
    public bool IsActive { get; private set; }
    public string Tier { get; private set; } = "Bronze"; // Bronze, Silver, Gold, Platinum

    // Navigation properties
    private readonly List<Reward> _rewards = new();
    public IReadOnlyCollection<Reward> Rewards => _rewards.AsReadOnly();

    private Customer() { } // EF Core

    private Customer(
        string tenantId,
        string customerId,
        string firstName,
        string lastName,
        string email,
        string? phoneNumber)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        PointsBalance = Points.Zero;
        LifetimePoints = Points.Zero;
        EnrolledAt = DateTime.UtcNow;
        IsActive = true;
        Tier = "Bronze";

        AddDomainEvent(new CustomerEnrolledEvent(Id, tenantId, customerId, email));
    }

    public static Customer Create(
        string tenantId,
        string customerId,
        string firstName,
        string lastName,
        string email,
        string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID is required", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID is required", nameof(customerId));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        return new Customer(tenantId, customerId, firstName, lastName, email, phoneNumber);
    }

    /// <summary>
    /// Awards points to the customer and updates tier if necessary
    /// </summary>
    public void AwardPoints(Points points, string reason, string? transactionId = null)
    {
        if (points.IsZero)
            throw new ArgumentException("Cannot award zero points", nameof(points));

        if (!IsActive)
            throw new InvalidOperationException("Cannot award points to inactive customer");

        var previousBalance = PointsBalance;
        PointsBalance = PointsBalance.Add(points);
        LifetimePoints = LifetimePoints.Add(points);

        MarkAsUpdated();

        // Check for tier upgrade
        UpdateTier();

        AddDomainEvent(new PointsAwardedEvent(
            Id,
            TenantId,
            CustomerId,
            points,
            previousBalance,
            PointsBalance,
            reason,
            transactionId));
    }

    /// <summary>
    /// Redeems points from the customer's balance
    /// </summary>
    public void RedeemPoints(Points points, string reason)
    {
        if (points.IsZero)
            throw new ArgumentException("Cannot redeem zero points", nameof(points));

        if (!IsActive)
            throw new InvalidOperationException("Cannot redeem points for inactive customer");

        if (PointsBalance.IsLessThan(points))
            throw new InvalidOperationException($"Insufficient points. Available: {PointsBalance}, Requested: {points}");

        var previousBalance = PointsBalance;
        PointsBalance = PointsBalance.Subtract(points);

        MarkAsUpdated();

        AddDomainEvent(new PointsRedeemedEvent(
            Id,
            TenantId,
            CustomerId,
            points,
            previousBalance,
            PointsBalance,
            reason));
    }

    /// <summary>
    /// Updates customer tier based on lifetime points
    /// </summary>
    private void UpdateTier()
    {
        var previousTier = Tier;
        var lifetimeValue = LifetimePoints.Value;

        Tier = lifetimeValue switch
        {
            >= 100000 => "Platinum",
            >= 50000 => "Gold",
            >= 10000 => "Silver",
            _ => "Bronze"
        };

        if (Tier != previousTier)
        {
            AddDomainEvent(new CustomerTierUpgradedEvent(
                Id,
                TenantId,
                CustomerId,
                previousTier,
                Tier,
                LifetimePoints));
        }
    }

    /// <summary>
    /// Deactivates the customer account
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Customer is already inactive");

        IsActive = false;
        MarkAsUpdated();

        AddDomainEvent(new CustomerDeactivatedEvent(Id, TenantId, CustomerId));
    }

    /// <summary>
    /// Reactivates the customer account
    /// </summary>
    public void Reactivate()
    {
        if (IsActive)
            throw new InvalidOperationException("Customer is already active");

        IsActive = true;
        MarkAsUpdated();

        AddDomainEvent(new CustomerReactivatedEvent(Id, TenantId, CustomerId));
    }

    /// <summary>
    /// Updates customer information
    /// </summary>
    public void UpdateInformation(string firstName, string lastName, string email, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;

        MarkAsUpdated();
    }

    public string FullName => $"{FirstName} {LastName}";
}
