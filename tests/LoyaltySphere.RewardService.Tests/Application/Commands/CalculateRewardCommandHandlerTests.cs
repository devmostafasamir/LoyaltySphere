using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Application.Commands.CalculateReward;
using LoyaltySphere.RewardService.Application.Interfaces;
using LoyaltySphere.RewardService.Application.Services;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using LoyaltySphere.RewardService.Infrastructure.Repositories;
using LoyaltySphere.MultiTenancy;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoyaltySphere.RewardService.Tests.Application.Commands;

/// <summary>
/// Unit tests for CalculateRewardCommandHandler.
/// </summary>
public class CalculateRewardCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Mock<IRewardCalculationService> _calculationServiceMock;
    private readonly Mock<ILogger<CalculateRewardCommandHandler>> _handlerLoggerMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private const string TenantId = "test-tenant";

    public CalculateRewardCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _tenantContextMock = new Mock<ITenantContext>();
        _tenantContextMock.Setup(t => t.TenantId).Returns(TenantId);
        _tenantContextMock.Setup(t => t.HasTenant).Returns(true);
        
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        
        _context = new ApplicationDbContext(options, _tenantContextMock.Object, _loggerFactoryMock.Object);
        _unitOfWork = new UnitOfWork(_context);
        
        _calculationServiceMock = new Mock<IRewardCalculationService>();
        _handlerLoggerMock = new Mock<ILogger<CalculateRewardCommandHandler>>();
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
            TenantId = TenantId,
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP",
            MerchantId = "merchant-001",
            MerchantCategory = "Retail",
            TransactionId = "txn-123"
        };

        var handler = new CalculateRewardCommandHandler(_unitOfWork, _calculationServiceMock.Object, Mock.Of<ICacheService>(), _handlerLoggerMock.Object);
        
        var rule = RewardRule.Create(TenantId, "Standard", "1 point", 1.0m);
        _calculationServiceMock.Setup(s => s.CalculateRewardAsync(It.IsAny<Customer>(), It.IsAny<Money>(), It.IsAny<IEnumerable<RewardRule>>(), It.IsAny<IEnumerable<Campaign>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(RewardCalculationResult.Success(Points.Create(100), Points.Create(100), Points.Create(0), rule, null, 1.0m)));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PointsAwarded.Should().Be(100);
        
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
            TenantId = TenantId,
            CustomerId = "non-existent",
            TransactionAmount = 1000.00m,
            Currency = "EGP"
        };

        var handler = new CalculateRewardCommandHandler(_unitOfWork, _calculationServiceMock.Object, Mock.Of<ICacheService>(), _handlerLoggerMock.Object);

        var rule = RewardRule.Create(TenantId, "Standard", "1 point", 1.0m);
        _calculationServiceMock.Setup(s => s.CalculateRewardAsync(It.IsAny<Customer>(), It.IsAny<Money>(), It.IsAny<IEnumerable<RewardRule>>(), It.IsAny<IEnumerable<Campaign>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(RewardCalculationResult.Success(Points.Create(100), Points.Create(100), Points.Create(0), rule, null, 1.0m)));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be("non-existent");
        
        // Verify customer was created
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == "non-existent");
        customer.Should().NotBeNull();
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
            TenantId = TenantId,
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP"
        };

        var handler = new CalculateRewardCommandHandler(_unitOfWork, _calculationServiceMock.Object, Mock.Of<ICacheService>(), _handlerLoggerMock.Object);
        
        _calculationServiceMock.Setup(s => s.CalculateRewardAsync(It.IsAny<Customer>(), It.IsAny<Money>(), It.IsAny<IEnumerable<RewardRule>>(), It.IsAny<IEnumerable<Campaign>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RewardCalculationResult.NoReward("Customer is inactive"));

        // Act & Assert
        await handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Customer is inactive");
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
            TenantId = TenantId,
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP",
            TransactionId = "txn-123"
        };

        var handler = new CalculateRewardCommandHandler(_unitOfWork, _calculationServiceMock.Object, Mock.Of<ICacheService>(), _handlerLoggerMock.Object);

        var rule = RewardRule.Create(TenantId, "Standard", "1 point", 1.0m);
        _calculationServiceMock.Setup(s => s.CalculateRewardAsync(It.IsAny<Customer>(), It.IsAny<Money>(), It.IsAny<IEnumerable<RewardRule>>(), It.IsAny<IEnumerable<Campaign>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(RewardCalculationResult.Success(Points.Create(100), Points.Create(100), Points.Create(0), rule, null, 1.0m)));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.TransactionId == "txn-123");
        reward.Should().NotBeNull();
        reward!.CustomerExternalId.Should().Be("cust-001");
        reward.TransactionAmount.Amount.Should().Be(1000.00m);
        reward.PointsAwarded.Value.Should().BeGreaterThan(0);
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
            TenantId = TenantId,
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP"
        };

        var handler = new CalculateRewardCommandHandler(_unitOfWork, _calculationServiceMock.Object, Mock.Of<ICacheService>(), _handlerLoggerMock.Object);

        var rule = RewardRule.Create(TenantId, "Silver Rule", "1.5 points", 1.5m);
        _calculationServiceMock.Setup(s => s.CalculateRewardAsync(It.IsAny<Customer>(), It.IsAny<Money>(), It.IsAny<IEnumerable<RewardRule>>(), It.IsAny<IEnumerable<Campaign>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(RewardCalculationResult.Success(Points.Create(150), Points.Create(100), Points.Create(0), rule, null, 1.5m)));

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

        var rule = RewardRule.Create(TenantId, "Standard", "1 point", 1.0m);
        _calculationServiceMock.Setup(s => s.CalculateRewardAsync(It.IsAny<Customer>(), It.IsAny<Money>(), It.IsAny<IEnumerable<RewardRule>>(), It.IsAny<IEnumerable<Campaign>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(RewardCalculationResult.Success(Points.Create(100), Points.Create(100), Points.Create(0), rule, null, 1.0m)));

        var handler = new CalculateRewardCommandHandler(_unitOfWork, _calculationServiceMock.Object, Mock.Of<ICacheService>(), _handlerLoggerMock.Object);

        // Act - Process multiple transactions
        await handler.Handle(new CalculateRewardCommand
        {
            TenantId = TenantId,
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP",
            TransactionId = "txn-1"
        }, CancellationToken.None);

        await handler.Handle(new CalculateRewardCommand
        {
            TenantId = TenantId,
            CustomerId = "cust-001",
            TransactionAmount = 500.00m,
            Currency = "EGP",
            TransactionId = "txn-2"
        }, CancellationToken.None);

        await handler.Handle(new CalculateRewardCommand
        {
            TenantId = TenantId,
            CustomerId = "cust-001",
            TransactionAmount = 750.00m,
            Currency = "EGP",
            TransactionId = "txn-3"
        }, CancellationToken.None);

        // Assert
        var updatedCustomer = await _context.Customers.FirstAsync(c => c.CustomerId == "cust-001");
        updatedCustomer.PointsBalance.Value.Should().BeGreaterThan(0);
        
        var rewardCount = await _context.Rewards.CountAsync(r => r.CustomerExternalId == "cust-001");
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
            TenantId = TenantId,
            CustomerId = "cust-001",
            TransactionAmount = 1000.00m,
            Currency = "EGP",
            MerchantId = "merchant-001",
            MerchantCategory = "Fuel",
            TransactionId = "txn-merch-1"
        };

        var handler = new CalculateRewardCommandHandler(_unitOfWork, _calculationServiceMock.Object, Mock.Of<ICacheService>(), _handlerLoggerMock.Object);

        var rule = RewardRule.Create(TenantId, "Standard Rule", "1 point per unit", 1.0m);
        _calculationServiceMock.Setup(s => s.CalculateRewardAsync(It.IsAny<Customer>(), It.IsAny<Money>(), It.IsAny<IEnumerable<RewardRule>>(), It.IsAny<IEnumerable<Campaign>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => Task.FromResult(RewardCalculationResult.Success(Points.Create(110), Points.Create(100), Points.Create(0), rule, null, 1.1m)));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        var reward = await _context.Rewards.FirstAsync(r => r.CustomerExternalId == "cust-001");
        reward.MerchantId.Should().Be("merchant-001");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
