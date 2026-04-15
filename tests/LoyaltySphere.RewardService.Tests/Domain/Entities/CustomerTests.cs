using FluentAssertions;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Domain.Events;
using Xunit;

namespace LoyaltySphere.RewardService.Tests.Domain.Entities;

/// <summary>
/// Unit tests for Customer entity.
/// Tests business logic, domain events, and invariants.
/// </summary>
public class CustomerTests
{
    private const string TenantId = "test-tenant";

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var customer = Customer.Create(
            TenantId,
            "cust-001",
            "Ahmed",
            "Hassan",
            "ahmed@example.com",
            "+20123456789"
        );

        // Assert
        customer.Should().NotBeNull();
        customer.TenantId.Should().Be(TenantId);
        customer.CustomerId.Should().Be("cust-001");
        customer.FirstName.Should().Be("Ahmed");
        customer.LastName.Should().Be("Hassan");
        customer.Email.Should().Be("ahmed@example.com");
        customer.PhoneNumber.Should().Be("+20123456789");
        customer.PointsBalance.Value.Should().Be(0);
        customer.LifetimePoints.Value.Should().Be(0);
        customer.IsActive.Should().BeTrue();
        customer.Tier.Should().Be("Bronze");
    }

    [Fact]
    public void Create_WithoutPhoneNumber_ShouldSucceed()
    {
        // Arrange & Act
        var customer = Customer.Create(
            TenantId,
            "cust-001",
            "Ahmed",
            "Hassan",
            "ahmed@example.com"
        );

        // Assert
        customer.PhoneNumber.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => Customer.Create(
            "",
            "cust-001",
            "Ahmed",
            "Hassan",
            "ahmed@example.com"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Tenant ID is required*");
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => Customer.Create(
            TenantId,
            "",
            "Ahmed",
            "Hassan",
            "ahmed@example.com"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Customer ID is required*");
    }

    [Fact]
    public void Create_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => Customer.Create(
            TenantId,
            "cust-001",
            "",
            "Hassan",
            "ahmed@example.com"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*First name is required*");
    }

    [Fact]
    public void Create_ShouldRaiseCustomerEnrolledEvent()
    {
        // Arrange & Act
        var customer = Customer.Create(
            TenantId,
            "cust-001",
            "Ahmed",
            "Hassan",
            "ahmed@example.com"
        );

        // Assert
        var domainEvents = customer.GetDomainEvents();
        domainEvents.Should().ContainSingle();
        domainEvents.First().Should().BeOfType<CustomerEnrolledEvent>();
        
        var enrolledEvent = (CustomerEnrolledEvent)domainEvents.First();
        enrolledEvent.TenantId.Should().Be(TenantId);
        enrolledEvent.CustomerId.Should().Be("cust-001");
        enrolledEvent.Email.Should().Be("ahmed@example.com");
    }

    [Fact]
    public void AwardPoints_WithValidPoints_ShouldIncreaseBalance()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        var points = Points.Create(100);

        // Act
        customer.AwardPoints(points, "Purchase reward");

        // Assert
        customer.PointsBalance.Value.Should().Be(100);
        customer.LifetimePoints.Value.Should().Be(100);
    }

    [Fact]
    public void AwardPoints_Multiple_ShouldAccumulate()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");

        // Act
        customer.AwardPoints(Points.Create(100), "Reward 1");
        customer.AwardPoints(Points.Create(50), "Reward 2");
        customer.AwardPoints(Points.Create(25), "Reward 3");

        // Assert
        customer.PointsBalance.Value.Should().Be(175);
        customer.LifetimePoints.Value.Should().Be(175);
    }

    [Fact]
    public void AwardPoints_ShouldRaisePointsAwardedEvent()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.ClearDomainEvents(); // Clear enrollment event

        // Act
        customer.AwardPoints(Points.Create(100), "Purchase reward", "txn-123");

        // Assert
        var domainEvents = customer.GetDomainEvents();
        domainEvents.Should().ContainSingle();
        domainEvents.First().Should().BeOfType<PointsAwardedEvent>();
        
        var awardedEvent = (PointsAwardedEvent)domainEvents.First();
        awardedEvent.PointsAwarded.Value.Should().Be(100);
        awardedEvent.Reason.Should().Be("Purchase reward");
        awardedEvent.TransactionId.Should().Be("txn-123");
    }

    [Fact]
    public void AwardPoints_ToInactiveCustomer_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.Deactivate();

        // Act
        Action act = () => customer.AwardPoints(Points.Create(100), "Reward");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*inactive customer*");
    }

    [Fact]
    public void AwardPoints_WithZeroPoints_ShouldThrowArgumentException()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");

        // Act
        Action act = () => customer.AwardPoints(Points.Zero, "Reward");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Cannot award zero points*");
    }

    [Fact]
    public void RedeemPoints_WithSufficientBalance_ShouldDecreaseBalance()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.AwardPoints(Points.Create(200), "Initial reward");

        // Act
        customer.RedeemPoints(Points.Create(50), "Cashback redemption");

        // Assert
        customer.PointsBalance.Value.Should().Be(150);
        customer.LifetimePoints.Value.Should().Be(200); // Lifetime points don't decrease
    }

    [Fact]
    public void RedeemPoints_WithInsufficientBalance_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.AwardPoints(Points.Create(50), "Initial reward");

        // Act
        Action act = () => customer.RedeemPoints(Points.Create(100), "Redemption");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Insufficient points*");
    }

    [Fact]
    public void RedeemPoints_ShouldRaisePointsRedeemedEvent()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.AwardPoints(Points.Create(200), "Initial reward");
        customer.ClearDomainEvents();

        // Act
        customer.RedeemPoints(Points.Create(50), "Cashback");

        // Assert
        var domainEvents = customer.GetDomainEvents();
        domainEvents.Should().ContainSingle();
        domainEvents.First().Should().BeOfType<PointsRedeemedEvent>();
        
        var redeemedEvent = (PointsRedeemedEvent)domainEvents.First();
        redeemedEvent.PointsRedeemed.Value.Should().Be(50);
        redeemedEvent.NewBalance.Value.Should().Be(150);
    }

    [Theory]
    [InlineData(0, "Bronze")]
    [InlineData(9999, "Bronze")]
    [InlineData(10000, "Silver")]
    [InlineData(49999, "Silver")]
    [InlineData(50000, "Gold")]
    [InlineData(99999, "Gold")]
    [InlineData(100000, "Platinum")]
    public void AwardPoints_ShouldUpdateTierBasedOnLifetimePoints(decimal pointsToAward, string expectedTier)
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");

        // Act
        customer.AwardPoints(Points.Create(pointsToAward), "Tier test");

        // Assert
        customer.Tier.Should().Be(expectedTier);
    }

    [Fact]
    public void AwardPoints_TierUpgrade_ShouldRaiseTierUpgradedEvent()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.ClearDomainEvents();

        // Act - Award enough points to upgrade from Bronze to Silver
        customer.AwardPoints(Points.Create(10000), "Big purchase");

        // Assert
        var domainEvents = customer.GetDomainEvents();
        domainEvents.Should().Contain(e => e is CustomerTierUpgradedEvent);
        
        var tierEvent = domainEvents.OfType<CustomerTierUpgradedEvent>().First();
        tierEvent.OldTier.Should().Be("Bronze");
        tierEvent.NewTier.Should().Be("Silver");
    }

    [Fact]
    public void Deactivate_ActiveCustomer_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");

        // Act
        customer.Deactivate();

        // Assert
        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldRaiseCustomerDeactivatedEvent()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.ClearDomainEvents();

        // Act
        customer.Deactivate();

        // Assert
        var domainEvents = customer.GetDomainEvents();
        domainEvents.Should().ContainSingle();
        domainEvents.First().Should().BeOfType<CustomerDeactivatedEvent>();
    }

    [Fact]
    public void Deactivate_AlreadyInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.Deactivate();

        // Act
        Action act = () => customer.Deactivate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already inactive*");
    }

    [Fact]
    public void Reactivate_InactiveCustomer_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.Deactivate();

        // Act
        customer.Reactivate();

        // Assert
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Reactivate_ShouldRaiseCustomerReactivatedEvent()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.Deactivate();
        customer.ClearDomainEvents();

        // Act
        customer.Reactivate();

        // Assert
        var domainEvents = customer.GetDomainEvents();
        domainEvents.Should().ContainSingle();
        domainEvents.First().Should().BeOfType<CustomerReactivatedEvent>();
    }

    [Fact]
    public void UpdateInformation_WithValidData_ShouldUpdateFields()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");

        // Act
        customer.UpdateInformation("Ahmed Updated", "Hassan Updated", "ahmed.new@example.com", "+20987654321");

        // Assert
        customer.FirstName.Should().Be("Ahmed Updated");
        customer.LastName.Should().Be("Hassan Updated");
        customer.Email.Should().Be("ahmed.new@example.com");
        customer.PhoneNumber.Should().Be("+20987654321");
    }

    [Fact]
    public void UpdateInformation_WithEmptyFirstName_ShouldThrowArgumentException()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");

        // Act
        Action act = () => customer.UpdateInformation("", "Hassan", "ahmed@example.com", null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*First name is required*");
    }

    [Fact]
    public void FullName_ShouldReturnCombinedName()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");

        // Act
        var fullName = customer.FullName;

        // Assert
        fullName.Should().Be("Ahmed Hassan");
    }
}
