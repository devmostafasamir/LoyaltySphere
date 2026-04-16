using LoyaltySphere.RewardService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoyaltySphere.RewardService.Infrastructure.Persistence.Seeds;

/// <summary>
/// Handles initializing the database with required default data.
/// Adheres to SOLID Single Responsibility Principle by decoupling seed logic
/// from EF Core configuration (IEntityTypeConfiguration).
/// </summary>
public class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger<ApplicationDbContextSeed> logger, string defaultTenantId = "national-bank")
    {
        try
        {
            if (!await context.RewardRules.AnyAsync(r => r.TenantId == defaultTenantId))
            {
                logger.LogInformation("Seeding default Reward Rules for tenant {TenantId}...", defaultTenantId);
                await context.RewardRules.AddRangeAsync(GetDefaultRewardRules(defaultTenantId));
            }

            if (!await context.Customers.AnyAsync(c => c.TenantId == defaultTenantId))
            {
                logger.LogInformation("Seeding default Customers for tenant {TenantId}...", defaultTenantId);
                await context.Customers.AddRangeAsync(GetDefaultCustomers(defaultTenantId));
            }

            // We must explicitly set the tenant context to bypass RLS/isolation during cross-tenant seed or initial seed
            // However, since we're generating entities that are tenant-bound, EF Core interceptors might intercept.
            // But we typically seed per-tenant.
            
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static IEnumerable<RewardRule> GetDefaultRewardRules(string tenantId)
    {
        return new List<RewardRule>
        {
            RewardRule.Create(
                tenantId: tenantId,
                ruleName: "Standard Purchase",
                description: "1 point for every $1 spent",
                pointsPerUnit: 1m,
                ruleType: "Standard",
                priority: 1
            ),
            RewardRule.Create(
                tenantId: tenantId,
                ruleName: "VIP Bonus",
                description: "1.5 points for every $1 spent over $1000",
                pointsPerUnit: 1.5m,
                ruleType: "Bonus",
                priority: 10,
                minimumTransactionAmount: 1000m
            )
        };
    }

    private static IEnumerable<Customer> GetDefaultCustomers(string tenantId)
    {
        return new List<Customer>
        {
            Customer.Create(
                tenantId: tenantId,
                customerId: "CUST-001",
                firstName: "John",
                lastName: "Doe",
                email: "john.doe@example.com",
                phoneNumber: "+15550100"
            ),
            Customer.Create(
                tenantId: tenantId,
                customerId: "CUST-002",
                firstName: "Jane",
                lastName: "Smith",
                email: "jane.smith@example.com",
                phoneNumber: "+15550200"
            )
        };
    }
}
