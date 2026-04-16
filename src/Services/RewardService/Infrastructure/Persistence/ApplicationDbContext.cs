using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Infrastructure.Persistence.Interceptors;
using LoyaltySphere.EventBus.Outbox;
using System.Linq.Expressions;
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
    private readonly ILoggerFactory _loggerFactory;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext,
        ILoggerFactory loggerFactory)
        : base(options)
    {
        _tenantContext = tenantContext;
        _loggerFactory = loggerFactory;
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
            // Configure multi-tenancy for entities that have a TenantId property
            if (entityType.ClrType.GetProperty("TenantId") != null)
            {
                modelBuilder.Entity(entityType.ClrType).Property("TenantId").IsRequired();
                
                // Configure global query filter for defense-in-depth
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Equal(
                    Expression.Property(parameter, "TenantId"),
                    Expression.Property(Expression.Constant(_tenantContext), nameof(ITenantContext.TenantId))
                );
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Add interceptors - create loggers from factory
        optionsBuilder.AddInterceptors(
            new TenantInterceptor(_tenantContext, _loggerFactory.CreateLogger<TenantInterceptor>()),
            new OutboxInterceptor(_loggerFactory.CreateLogger<OutboxInterceptor>()));

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
        // For relational databases (PostgreSQL), we MUST have a tenant context for RLS
        if (Database.IsRelational())
        {
            if (!_tenantContext.HasTenant)
            {
                throw new InvalidOperationException("Tenant context has not been set");
            }

            // Set PostgreSQL session variable that RLS policies will use
            var sql = $"SET app.current_tenant = '{_tenantContext.TenantId}';";
            
            var logger = _loggerFactory.CreateLogger<ApplicationDbContext>();
            logger.LogDebug("Setting PostgreSQL tenant context: {TenantId}", _tenantContext.TenantId);
            
            await Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
        // For non-relational (InMemory), we don't need to throw if it's missing,
        // although the Interceptor will still try to set TenantId on entities if context exists.
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
                var tenantIdProperty = entry.Metadata.FindProperty("TenantId");
                if (tenantIdProperty != null && _tenantContext.HasTenant)
                {
                    var currentValue = entry.Property("TenantId").CurrentValue;
                    if (currentValue == null || (currentValue is string s && string.IsNullOrEmpty(s)))
                    {
                        entry.Property("TenantId").CurrentValue = _tenantContext.TenantId;
                    }
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
