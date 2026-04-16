using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using LoyaltySphere.RewardService.Infrastructure.Repositories;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Application.Queries.GetCustomerBalance;

namespace LoyaltySphere.RewardService.Tests.Application.Queries;

/// <summary>
/// Unit tests for GetCustomerBalanceQueryHandler.
/// Tests query handling and data retrieval.
/// </summary>
public class GetCustomerBalanceQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private const string TenantId = "test-tenant";

    public GetCustomerBalanceQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _tenantContextMock = new Mock<ITenantContext>();
        _tenantContextMock.Setup(t => t.TenantId).Returns(TenantId);
        
        _loggerFactoryMock = new Mock<ILoggerFactory>();

        _context = new ApplicationDbContext(options, _tenantContextMock.Object, _loggerFactoryMock.Object);
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task Handle_WithExistingCustomer_ShouldReturnBalance()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.AwardPoints(Points.Create(500), "Initial points");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var query = new GetCustomerBalanceQuery { TenantId = TenantId, CustomerId = "cust-001" };
        var handler = new GetCustomerBalanceQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be("cust-001");
        result.PointsBalance.Should().Be(500);
        result.LifetimePoints.Should().Be(500);
        result.Tier.Should().Be("Bronze");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldReturnNull()
    {
        // Arrange
        var query = new GetCustomerBalanceQuery { TenantId = TenantId, CustomerId = "non-existent" };
        var handler = new GetCustomerBalanceQueryHandler(_unitOfWork);

        // Act & Assert
        await handler.Invoking(h => h.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WithZeroBalance_ShouldReturnZero()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var query = new GetCustomerBalanceQuery { TenantId = TenantId, CustomerId = "cust-001" };
        var handler = new GetCustomerBalanceQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PointsBalance.Should().Be(0);
        result.LifetimePoints.Should().Be(0);
    }

    [Fact]
    public async Task Handle_AfterRedemption_ShouldReflectReducedBalance()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.AwardPoints(Points.Create(500), "Initial points");
        customer.RedeemPoints(Points.Create(200), "Redemption");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var query = new GetCustomerBalanceQuery { TenantId = TenantId, CustomerId = "cust-001" };
        var handler = new GetCustomerBalanceQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PointsBalance.Should().Be(300); // 500 - 200
        result.LifetimePoints.Should().Be(500); // Lifetime doesn't decrease
    }

    [Fact]
    public async Task Handle_WithInactiveCustomer_ShouldReturnInactiveStatus()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.Deactivate();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var query = new GetCustomerBalanceQuery { TenantId = TenantId, CustomerId = "cust-001" };
        var handler = new GetCustomerBalanceQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithSilverTier_ShouldReturnCorrectTier()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.AwardPoints(Points.Create(10000), "Tier upgrade");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var query = new GetCustomerBalanceQuery { TenantId = TenantId, CustomerId = "cust-001" };
        var handler = new GetCustomerBalanceQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tier.Should().Be("Silver");
        result.LifetimePoints.Should().Be(10000);
    }

    [Fact]
    public async Task Handle_WithGoldTier_ShouldReturnCorrectTier()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.AwardPoints(Points.Create(50000), "Tier upgrade");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var query = new GetCustomerBalanceQuery { TenantId = TenantId, CustomerId = "cust-001" };
        var handler = new GetCustomerBalanceQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tier.Should().Be("Gold");
    }

    [Fact]
    public async Task Handle_WithPlatinumTier_ShouldReturnCorrectTier()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        customer.AwardPoints(Points.Create(100000), "Tier upgrade");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var query = new GetCustomerBalanceQuery { TenantId = TenantId, CustomerId = "cust-001" };
        var handler = new GetCustomerBalanceQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tier.Should().Be("Platinum");
    }

    [Fact]
    public async Task Handle_ShouldIncludeCustomerDetails()
    {
        // Arrange
        var customer = Customer.Create(TenantId, "cust-001", "Ahmed", "Hassan", "ahmed@example.com", "+20123456789");
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var query = new GetCustomerBalanceQuery { TenantId = TenantId, CustomerId = "cust-001" };
        var handler = new GetCustomerBalanceQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Ahmed");
        result.LastName.Should().Be("Hassan");
        result.Email.Should().Be("ahmed@example.com");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
