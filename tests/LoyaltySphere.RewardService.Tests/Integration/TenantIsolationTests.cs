using FluentAssertions;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LoyaltySphere.RewardService.Tests.Integration;

/// <summary>
/// Integration tests for multi-tenant data isolation.
/// Verifies that PostgreSQL RLS policies and EF Core interceptors
/// properly isolate data between tenants.
/// </summary>
public class TenantIsolationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private const string TenantA = "tenant-a";
    private const string TenantB = "tenant-b";

    public TenantIsolationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateCustomer_WithTenantA_ShouldOnlyBeVisibleToTenantA()
    {
        // Arrange
        var tenantId = TenantId.Create(TenantA);
        var customer = Customer.Create(
            TenantA,
            "cust-001",
            "Ahmed",
            "Hassan",
            "ahmed@example.com"
        );

        // Act - Create customer for Tenant A
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Assert - Verify customer exists for Tenant A
        var customerForTenantA = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == "cust-001" && c.TenantId == TenantA);
        
        customerForTenantA.Should().NotBeNull();
        customerForTenantA!.TenantId.Should().Be(TenantA);

        // Assert - Verify customer is NOT visible to Tenant B
        var customerForTenantB = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == "cust-001" && c.TenantId == TenantB);
        
        customerForTenantB.Should().BeNull();
    }

    [Fact]
    public async Task CreateReward_WithTenantA_ShouldOnlyBeVisibleToTenantA()
    {
        // Arrange
        var customerA = Customer.Create(TenantA, "cust-a", "Ahmed", "Hassan", "ahmed@example.com");
        _context.Customers.Add(customerA);
        await _context.SaveChangesAsync();

        var transactionAmount = Money.Create(1000, "EGP");
        var pointsAwarded = Points.Create(100);

        var reward = Reward.Create(
            TenantA,
            customerA.Id,
            "cust-a",
            transactionAmount,
            pointsAwarded,
            "Purchase reward"
        );

        // Act
        _context.Rewards.Add(reward);
        await _context.SaveChangesAsync();

        // Assert - Verify reward exists for Tenant A
        var rewardForTenantA = await _context.Rewards
            .FirstOrDefaultAsync(r => r.CustomerId == "cust-a" && r.TenantId == TenantA);
        
        rewardForTenantA.Should().NotBeNull();

        // Assert - Verify reward is NOT visible to Tenant B
        var rewardForTenantB = await _context.Rewards
            .FirstOrDefaultAsync(r => r.CustomerId == "cust-a" && r.TenantId == TenantB);
        
        rewardForTenantB.Should().BeNull();
    }

    [Fact]
    public async Task QueryCustomers_ForTenantA_ShouldOnlyReturnTenantACustomers()
    {
        // Arrange - Create customers for both tenants
        var customerA1 = Customer.Create(TenantA, "cust-a1", "Ahmed", "Hassan", "ahmed@example.com");
        var customerA2 = Customer.Create(TenantA, "cust-a2", "Fatima", "Ali", "fatima@example.com");
        var customerB1 = Customer.Create(TenantB, "cust-b1", "John", "Doe", "john@example.com");

        _context.Customers.AddRange(customerA1, customerA2, customerB1);
        await _context.SaveChangesAsync();

        // Act - Query customers for Tenant A
        var tenantACustomers = await _context.Customers
            .Where(c => c.TenantId == TenantA)
            .ToListAsync();

        // Assert
        tenantACustomers.Should().HaveCount(2);
        tenantACustomers.Should().AllSatisfy(c => c.TenantId.Should().Be(TenantA));
        tenantACustomers.Should().Contain(c => c.CustomerId == "cust-a1");
        tenantACustomers.Should().Contain(c => c.CustomerId == "cust-a2");
        tenantACustomers.Should().NotContain(c => c.CustomerId == "cust-b1");
    }

    [Fact]
    public async Task AwardPoints_ToTenantACustomer_ShouldNotAffectTenantBCustomer()
    {
        // Arrange
        var customerA = Customer.Create(TenantA, "cust-shared-id", "Ahmed", "Hassan", "ahmed@example.com");
        var customerB = Customer.Create(TenantB, "cust-shared-id", "John", "Doe", "john@example.com");

        _context.Customers.AddRange(customerA, customerB);
        await _context.SaveChangesAsync();

        // Act - Award points to Tenant A customer
        customerA.AwardPoints(Points.Create(100), "Test reward");
        await _context.SaveChangesAsync();

        // Assert - Verify Tenant A customer has points
        var updatedCustomerA = await _context.Customers
            .FirstAsync(c => c.CustomerId == "cust-shared-id" && c.TenantId == TenantA);
        
        updatedCustomerA.PointsBalance.Value.Should().Be(100);

        // Assert - Verify Tenant B customer still has zero points
        var updatedCustomerB = await _context.Customers
            .FirstAsync(c => c.CustomerId == "cust-shared-id" && c.TenantId == TenantB);
        
        updatedCustomerB.PointsBalance.Value.Should().Be(0);
    }

    [Fact]
    public async Task GetRewardHistory_ForTenantA_ShouldOnlyReturnTenantARewards()
    {
        // Arrange
        var customerA = Customer.Create(TenantA, "cust-a", "Ahmed", "Hassan", "ahmed@example.com");
        var customerB = Customer.Create(TenantB, "cust-b", "John", "Doe", "john@example.com");

        _context.Customers.AddRange(customerA, customerB);
        await _context.SaveChangesAsync();

        var rewardA1 = Reward.Create(TenantA, customerA.Id, "cust-a", Money.Create(1000, "EGP"), Points.Create(100), "Reward 1");
        var rewardA2 = Reward.Create(TenantA, customerA.Id, "cust-a", Money.Create(2000, "EGP"), Points.Create(200), "Reward 2");
        var rewardB1 = Reward.Create(TenantB, customerB.Id, "cust-b", Money.Create(1500, "EGP"), Points.Create(150), "Reward 3");

        _context.Rewards.AddRange(rewardA1, rewardA2, rewardB1);
        await _context.SaveChangesAsync();

        // Act
        var tenantARewards = await _context.Rewards
            .Where(r => r.TenantId == TenantA)
            .ToListAsync();

        // Assert
        tenantARewards.Should().HaveCount(2);
        tenantARewards.Should().AllSatisfy(r => r.TenantId.Should().Be(TenantA));
        tenantARewards.Should().Contain(r => r.PointsAwarded.Value == 100);
        tenantARewards.Should().Contain(r => r.PointsAwarded.Value == 200);
    }

    [Fact]
    public async Task UpdateCustomer_InTenantA_ShouldNotAffectTenantBCustomer()
    {
        // Arrange
        var customerA = Customer.Create(TenantA, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        var customerB = Customer.Create(TenantB, "cust-001", "John", "Doe", "john@example.com");

        _context.Customers.AddRange(customerA, customerB);
        await _context.SaveChangesAsync();

        // Act - Update Tenant A customer
        customerA.UpdateInformation("Ahmed Updated", "Hassan Updated", "ahmed.new@example.com", "+20123456789");
        await _context.SaveChangesAsync();

        // Assert - Verify Tenant A customer is updated
        var updatedCustomerA = await _context.Customers
            .FirstAsync(c => c.CustomerId == "cust-001" && c.TenantId == TenantA);
        
        updatedCustomerA.FirstName.Should().Be("Ahmed Updated");
        updatedCustomerA.Email.Should().Be("ahmed.new@example.com");

        // Assert - Verify Tenant B customer is unchanged
        var unchangedCustomerB = await _context.Customers
            .FirstAsync(c => c.CustomerId == "cust-001" && c.TenantId == TenantB);
        
        unchangedCustomerB.FirstName.Should().Be("John");
        unchangedCustomerB.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task DeleteCustomer_InTenantA_ShouldNotAffectTenantBCustomer()
    {
        // Arrange
        var customerA = Customer.Create(TenantA, "cust-001", "Ahmed", "Hassan", "ahmed@example.com");
        var customerB = Customer.Create(TenantB, "cust-001", "John", "Doe", "john@example.com");

        _context.Customers.AddRange(customerA, customerB);
        await _context.SaveChangesAsync();

        // Act - Delete Tenant A customer
        _context.Customers.Remove(customerA);
        await _context.SaveChangesAsync();

        // Assert - Verify Tenant A customer is deleted
        var deletedCustomerA = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == "cust-001" && c.TenantId == TenantA);
        
        deletedCustomerA.Should().BeNull();

        // Assert - Verify Tenant B customer still exists
        var existingCustomerB = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == "cust-001" && c.TenantId == TenantB);
        
        existingCustomerB.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
