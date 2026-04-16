using FluentAssertions;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using Xunit;

namespace LoyaltySphere.RewardService.Tests.Domain.ValueObjects;

/// <summary>
/// Unit tests for Money value object.
/// Tests currency validation, arithmetic operations, and formatting.
/// </summary>
public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ShouldSucceed()
    {
        // Arrange & Act
        var money = Money.Create(100.50m, "USD");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldSucceed()
    {
        // Arrange & Act
        var money = Money.Create(0, "EGP");

        // Assert
        money.Amount.Should().Be(0);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => Money.Create(-10, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Amount cannot be negative*");
    }

    [Fact]
    public void Create_WithNullCurrency_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => Money.Create(100, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Currency is required*");
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => Money.Create(100, "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Currency is required*");
    }

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = Money.Create(100, "USD");
        var money2 = Money.Create(50, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(100, "USD");
        var money2 = Money.Create(50, "EGP");

        // Act
        Action act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot operate on different currencies*");
    }

    [Fact]
    public void Subtract_SameCurrency_ShouldReturnDifference()
    {
        // Arrange
        var money1 = Money.Create(100, "USD");
        var money2 = Money.Create(30, "USD");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Subtract_DifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(100, "USD");
        var money2 = Money.Create(30, "EGP");

        // Act
        Action act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtract_ResultingInNegative_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var money1 = Money.Create(50, "USD");
        var money2 = Money.Create(100, "USD");

        // Act
        Action act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot subtract*");
    }

    [Fact]
    public void Multiply_ByPositiveNumber_ShouldReturnScaledMoney()
    {
        // Arrange
        var money = Money.Create(100, "USD");

        // Act
        var result = money.Multiply(1.5m);

        // Assert
        result.Amount.Should().Be(150);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Multiply_ByNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var money = Money.Create(100, "USD");

        // Act
        Action act = () => money.Multiply(-1.5m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_SameAmountAndCurrency_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.Create(100, "USD");
        var money2 = Money.Create(100, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentAmount_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.Create(100, "USD");
        var money2 = Money.Create(50, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void Equals_DifferentCurrency_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.Create(100, "USD");
        var money2 = Money.Create(100, "EGP");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = Money.Create(1234.56m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("1234.56 USD");
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EGP")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public void Create_WithVariousCurrencies_ShouldSucceed(string currency)
    {
        // Arrange & Act
        var money = Money.Create(100, currency);

        // Assert
        money.Currency.Should().Be(currency);
    }
}
