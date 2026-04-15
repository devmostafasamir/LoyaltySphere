using FluentAssertions;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using Xunit;

namespace LoyaltySphere.RewardService.Tests.Domain.ValueObjects;

/// <summary>
/// Unit tests for Points value object.
/// Tests immutability, validation, and arithmetic operations.
/// </summary>
public class PointsTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldSucceed()
    {
        // Arrange & Act
        var points = Points.Create(100);

        // Assert
        points.Value.Should().Be(100);
    }

    [Fact]
    public void Create_WithZero_ShouldSucceed()
    {
        // Arrange & Act
        var points = Points.Create(0);

        // Assert
        points.Value.Should().Be(0);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => Points.Create(-10);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Points cannot be negative*");
    }

    [Fact]
    public void Add_TwoPointsObjects_ShouldReturnSum()
    {
        // Arrange
        var points1 = Points.Create(100);
        var points2 = Points.Create(50);

        // Act
        var result = points1.Add(points2);

        // Assert
        result.Value.Should().Be(150);
    }

    [Fact]
    public void Subtract_TwoPointsObjects_ShouldReturnDifference()
    {
        // Arrange
        var points1 = Points.Create(100);
        var points2 = Points.Create(30);

        // Act
        var result = points1.Subtract(points2);

        // Assert
        result.Value.Should().Be(70);
    }

    [Fact]
    public void Subtract_ResultingInNegative_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var points1 = Points.Create(50);
        var points2 = Points.Create(100);

        // Act
        Action act = () => points1.Subtract(points2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot subtract*");
    }

    [Fact]
    public void Multiply_ByDecimal_ShouldReturnScaledPoints()
    {
        // Arrange
        var points = Points.Create(100);

        // Act
        var result = points.Multiply(1.5m);

        // Assert
        result.Value.Should().Be(150);
    }

    [Fact]
    public void Multiply_ByNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var points = Points.Create(100);

        // Act
        Action act = () => points.Multiply(-1.5m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_SameValue_ShouldReturnTrue()
    {
        // Arrange
        var points1 = Points.Create(100);
        var points2 = Points.Create(100);

        // Act & Assert
        points1.Should().Be(points2);
        (points1 == points2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var points1 = Points.Create(100);
        var points2 = Points.Create(50);

        // Act & Assert
        points1.Should().NotBe(points2);
        (points1 != points2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var points1 = Points.Create(100);
        var points2 = Points.Create(100);

        // Act & Assert
        points1.GetHashCode().Should().Be(points2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var points = Points.Create(1000);

        // Act
        var result = points.ToString();

        // Assert
        result.Should().Be("1000 points");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(999999)]
    public void Create_WithVariousValidAmounts_ShouldSucceed(decimal amount)
    {
        // Arrange & Act
        var points = Points.Create(amount);

        // Assert
        points.Value.Should().Be(amount);
    }
}
