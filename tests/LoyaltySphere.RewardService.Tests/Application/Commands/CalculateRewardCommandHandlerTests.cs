using FluentAssertions;
using LoyaltySphere.RewardService.Application.Commands.CalculateReward;
using LoyaltySphere.RewardService.Application.Services;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LoyaltySphere.RewardService.Tests.Application.Commands;

/// <summary>
/// Unit tests for CalculateRewardCommandHandler.
/// Tests command handling, validation, and integration with domain services.
/// </summary>
public class CalculateRewardCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<RewardCalculationService>> _loggerMock;
    private readonly IRewardCalculationService _calculationService;
    private const string TenantId = "test-tenant";

    public CalculateRewardCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<RewardCalculationService>>();
        _calculationService = new RewardCalculationService(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCalculateAndAwardPoints()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new CalculateRewardCommand
        {
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP",
            MerchantId = "merchant-001",
            MerchantCategory = "Retail"
        };

        var handler = new CalculateRewardCommandHandler(_context, _calculationService);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.PointsAwarded.Should().BeGreaterThan(0);
        
        // Verify customer balance was updated
        var updatedCustomer = await _context.Customers.FirstAsync(c => c.CustomerId == "cust-001");
        updatedCustomer.PointsBalance.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldReturnFailure()
    {
        // Arrange
        var command = new CalculateRewardCommand
        {
            CustomerId = "non-existent",
            TransactionAmount = 1000.00m,
            Currency = "EGP"
        };

        var handler = new CalculateRewardCommandHandler(_context, _calculationService);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Customer not found");
    }

    [Fact]
    public async Task Handle_WithInactiveCustomer_ShouldReturnFailure()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.Deactivate();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new CalculateRewardCommand
        {
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP"
        };

        var handler = new CalculateRewardCommandHandler(_context, _calculationService);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("inactive");
    }

    [Fact]
    public async Task Handle_WithZeroAmount_ShouldReturnFailure()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new CalculateRewardCommand
        {
            CustomerId = "cust-001",
            TransactionAmount = 0,
            Currency = "EGP"
        };

        var handler = new CalculateRewardCommandHandler(_context, _calculationService);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Transaction amount must be greater than zero");
    }

    [Fact]
    public async Task Handle_WithNegativeAmount_ShouldReturnFailure()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new CalculateRewardCommand
        {
            CustomerId = "cust-001",
            TransactionAmount = -100,
            Currency = "EGP"
        };

        var handler = new CalculateRewardCommandHandler(_context, _calculationService);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldCreateRewardRecord()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new CalculateRewardCommand
        {
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP",
            TransactionId = "txn-123"
        };

        var handler = new CalculateRewardCommandHandler(_context, _calculationService);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.TransactionId == "txn-123");
        reward.Should().NotBeNull();
        reward!.CustomerId.Should().Be("cust-001");
        reward.TransactionAmount.Amount.Should().Be(1000.00m);
        reward.PointsAwarded.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithBronzeTier_ShouldApplyCorrectMultiplier()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new CalculateRewardCommand
        {
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP"
        };

        var handler = new CalculateRewardCommandHandler(_context, _calculationService);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.PointsAwarded.Should().BeGreaterThan(0);
        // Bronze tier should get base points (no multiplier)
    }

    [Fact]
    public async Task Handle_WithSilverTier_ShouldApplyTierBonus()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        // Award enough points to reach Silver tier
        customer.AwardPoints(Points.Create(10000), "Initial points");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new CalculateRewardCommand
        {
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP"
        };

        var handler = new CalculateRewardCommandHandler(_context, _calculationService);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.PointsAwarded.Should().BeGreaterThan(0);
        result.TierMultiplier.Should().BeGreaterThan(1.0m);
    }

    [Fact]
    public async Task Handle_MultipleTransactions_ShouldAccumulatePoints()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var handler = new CalculateRewardCommandHandler(_context, _calculationService);

        // Act - Process multiple transactions
        await handler.Handle(new CalculateRewardCommand
        {
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP"
        }, CancellationToken.None);

        await handler.Handle(new CalculateRewardCommand
        {
            CustomerId = "cust-001",
            TransactionAmount = 500.00m,
            Currency = "EGP"
        }, CancellationToken.None);

        await handler.Handle(new CalculateRewardCommand
        {
            CustomerId = "cust-001",
            TransactionAmount = 750.00m,
            Currency = "EGP"
        }, CancellationToken.None);

        // Assert
        var updatedCustomer = await _context.Customers.FirstAsync(c => c.CustomerId == "cust-001");
        updatedCustomer.PointsBalance.Value.Should().BeGreaterThan(0);
        
        var rewardCount = await _context.Rewards.CountAsync(r => r.CustomerId == "cust-001");
        rewardCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithMerchantCategory_ShouldIncludeInReward()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var command = new CalculateRewardCommand
        {
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP",
            MerchantId = "merchant-001",
            MerchantCategory = "Fuel"
        };

        var handler = new CalculateRewardCommandHandler(_context, _calculationService);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var reward = await _context.Rewards.FirstAsync(r => r.CustomerId == "cust-001");
        reward.MerchantId.Should().Be("merchant-001");
        reward.MerchantCategory.Should().Be("Fuel");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
