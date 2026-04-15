using FluentAssertions;
using LoyaltySphere.RewardService.Application.Services;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using Xunit;

namespace LoyaltySphere.RewardService.Tests.Application.Services;

/// <summary>
/// Unit tests for RewardCalculationService.
/// Tests reward calculation logic, tier bonuses, and business rules.
/// </summary>
public class RewardCalculationServiceTests
{
    private readonly IRewardCalculationService _service;

    public RewardCalculationServiceTests()
    {
        _service = new RewardCalculationService();
    }

    [Fact]
    public void CalculatePoints_BronzeTier_ShouldApply10PercentRate()
    {
        // Arrange
        var transactionAmount = Money.Create(1000, "EGP");
        var customer = CreateCustomer("Bronze", 0);

        // Act
        var points = _service.CalculatePoints(transactionAmount, customer);

        // Assert
        points.Value.Should().Be(100); // 1000 * 0.10 = 100
    }

    [Fact]
    public void CalculatePoints_SilverTier_ShouldApply15PercentRate()
    {
        // Arrange
        var transactionAmount = Money.Create(1000, "EGP");
        var customer = CreateCustomer("Silver", 1500);

        // Act
        var points = _service.CalculatePoints(transactionAmount, customer);

        // Assert
        points.Value.Should().Be(150); // 1000 * 0.15 = 150
    }

    [Fact]
    public void CalculatePoints_GoldTier_ShouldApply20PercentRate()
    {
        // Arrange
        var transactionAmount = Money.Create(1000, "EGP");
        var customer = CreateCustomer("Gold", 6000);

        // Act
        var points = _service.CalculatePoints(transactionAmount, customer);

        // Assert
        points.Value.Should().Be(200); // 1000 * 0.20 = 200
    }

    [Fact]
    public void CalculatePoints_PlatinumTier_ShouldApply25PercentRate()
    {
        // Arrange
        var transactionAmount = Money.Create(1000, "EGP");
        var customer = CreateCustomer("Platinum", 12000);

        // Act
        var points = _service.CalculatePoints(transactionAmount, customer);

        // Assert
        points.Value.Should().Be(250); // 1000 * 0.25 = 250
    }

    [Fact]
    public void CalculatePoints_WithZeroAmount_ShouldReturnZeroPoints()
    {
        // Arrange
        var transactionAmount = Money.Create(0, "EGP");
        var customer = CreateCustomer("Bronze", 0);

        // Act
        var points = _service.CalculatePoints(transactionAmount, customer);

        // Assert
        points.Value.Should().Be(0);
    }

    [Fact]
    public void CalculatePoints_WithDecimalAmount_ShouldRoundDown()
    {
        // Arrange
        var transactionAmount = Money.Create(99.99m, "EGP");
        var customer = CreateCustomer("Bronze", 0);

        // Act
        var points = _service.CalculatePoints(transactionAmount, customer);

        // Assert
        points.Value.Should().Be(9); // 99.99 * 0.10 = 9.999, rounded down to 9
    }

    [Fact]
    public void DetermineNewTier_WithLessThan1000Points_ShouldReturnBronze()
    {
        // Arrange
        var lifetimePoints = Points.Create(500);

        // Act
        var tier = _service.DetermineNewTier(lifetimePoints);

        // Assert
        tier.Should().Be("Bronze");
    }

    [Fact]
    public void DetermineNewTier_WithExactly1000Points_ShouldReturnSilver()
    {
        // Arrange
        var lifetimePoints = Points.Create(1000);

        // Act
        var tier = _service.DetermineNewTier(lifetimePoints);

        // Assert
        tier.Should().Be("Silver");
    }

    [Fact]
    public void DetermineNewTier_With3000Points_ShouldReturnSilver()
    {
        // Arrange
        var lifetimePoints = Points.Create(3000);

        // Act
        var tier = _service.DetermineNewTier(lifetimePoints);

        // Assert
        tier.Should().Be("Silver");
    }

    [Fact]
    public void DetermineNewTier_WithExactly5000Points_ShouldReturnGold()
    {
        // Arrange
        var lifetimePoints = Points.Create(5000);

        // Act
        var tier = _service.DetermineNewTier(lifetimePoints);

        // Assert
        tier.Should().Be("Gold");
    }

    [Fact]
    public void DetermineNewTier_With8000Points_ShouldReturnGold()
    {
        // Arrange
        var lifetimePoints = Points.Create(8000);

        // Act
        var tier = _service.DetermineNewTier(lifetimePoints);

        // Assert
        tier.Should().Be("Gold");
    }

    [Fact]
    public void DetermineNewTier_WithExactly10000Points_ShouldReturnPlatinum()
    {
        // Arrange
        var lifetimePoints = Points.Create(10000);

        // Act
        var tier = _service.DetermineNewTier(lifetimePoints);

        // Assert
        tier.Should().Be("Platinum");
    }

    [Fact]
    public void DetermineNewTier_With50000Points_ShouldReturnPlatinum()
    {
        // Arrange
        var lifetimePoints = Points.Create(50000);

        // Act
        var tier = _service.DetermineNewTier(lifetimePoints);

        // Assert
        tier.Should().Be("Platinum");
    }

    [Theory]
    [InlineData(0, "Bronze")]
    [InlineData(999, "Bronze")]
    [InlineData(1000, "Silver")]
    [InlineData(4999, "Silver")]
    [InlineData(5000, "Gold")]
    [InlineData(9999, "Gold")]
    [InlineData(10000, "Platinum")]
    [InlineData(100000, "Platinum")]
    public void DetermineNewTier_WithVariousPoints_ShouldReturnCorrectTier(
        decimal pointsValue, 
        string expectedTier)
    {
        // Arrange
        var lifetimePoints = Points.Create(pointsValue);

        // Act
        var tier = _service.DetermineNewTier(lifetimePoints);

        // Assert
        tier.Should().Be(expectedTier);
    }

    [Fact]
    public void CalculatePoints_LargeTransaction_ShouldHandleCorrectly()
    {
        // Arrange
        var transactionAmount = Money.Create(100000, "EGP");
        var customer = CreateCustomer("Platinum", 50000);

        // Act
        var points = _service.CalculatePoints(transactionAmount, customer);

        // Assert
        points.Value.Should().Be(25000); // 100000 * 0.25 = 25000
    }

    [Fact]
    public void CalculatePoints_UnknownTier_ShouldDefaultToBronzeRate()
    {
        // Arrange
        var transactionAmount = Money.Create(1000, "EGP");
        var customer = CreateCustomer("Unknown", 0);

        // Act
        var points = _service.CalculatePoints(transactionAmount, customer);

        // Assert
        points.Value.Should().Be(100); // Default to Bronze: 1000 * 0.10 = 100
    }

    // Helper method to create test customer
    private Customer CreateCustomer(string tier, decimal lifetimePoints)
    {
        var tenantId = TenantId.Create("test-tenant");
        var customer = Customer.Create(
            tenantId,
            "test-customer",
            "Test",
            "Customer",
            "test@example.com"
        );

        // Set tier and lifetime points using reflection or internal methods
        // In real implementation, you'd have proper methods to set these
        typeof(Customer)
            .GetProperty("Tier")!
            .SetValue(customer, tier);

        typeof(Customer)
            .GetProperty("LifetimePoints")!
            .SetValue(customer, Points.Create(lifetimePoints));

        return customer;
    }
}
