using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoyaltySphere.RewardService.Infrastructure.Persistence;

/// <summary>
/// Main database context for the Reward Service.
/// Implements multi-tenancy using PostgreSQL Row-Level Security (RLS).
/// All queries are automatically filtered by tenant_id at the database level.
/// </summary>
public class ApplicationDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ApplicationDbContext> _logger;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext,
        ILogger<ApplicationDbContext> logger)
        : base(options)
    {
        _tenantContext = tenantContext;
        _logger = logger;
    }

    // Domain entities
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<RewardRule> RewardRules => Set<RewardRule>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();

    // Outbox for reliable event publishing
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure multi-tenancy: Add tenant_id to all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Add tenant_id property if it doesn't exist
            if (entityType.ClrType.GetProperty("TenantId") != null)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<string>("TenantId")
                    .HasMaxLength(100)
                    .IsRequired();

                // Create index on tenant_id for performance
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("TenantId")
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_TenantId");

                // Add global query filter (defense in depth - RLS is primary)
                // This ensures tenant isolation even if RLS is misconfigured
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, "TenantId");
                var tenantId = System.Linq.Expressions.Expression.Constant(_tenantContext.TenantId);
                var equals = System.Linq.Expressions.Expression.Equal(property, tenantId);
                var lambda = System.Linq.Expressions.Expression.Lambda(equals, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Add interceptor to set tenant_id on save
        optionsBuilder.AddInterceptors(new TenantInterceptor(_tenantContext, _logger));
        
        // Add interceptor to populate outbox messages
        optionsBuilder.AddInterceptors(new OutboxInterceptor(_logger));

        // Enable sensitive data logging in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }
    }

    /// <summary>
    /// Sets the PostgreSQL session variable for Row-Level Security.
    /// This must be called before any queries to ensure RLS policies are applied.
    /// </summary>
    public async Task SetTenantContextAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.HasTenant)
        {
            throw new InvalidOperationException("Tenant context has not been set");
        }

        // Set PostgreSQL session variable that RLS policies will use
        var sql = $"SET app.current_tenant = '{_tenantContext.TenantId}';";
        
        _logger.LogDebug("Setting PostgreSQL tenant context: {TenantId}", _tenantContext.TenantId);
        
        await Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Ensure tenant context is set before saving
        await SetTenantContextAsync(cancellationToken);

        // Set tenant_id on all new entities
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Property("TenantId").CurrentValue == null)
                {
                    entry.Property("TenantId").CurrentValue = _tenantContext.TenantId;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
