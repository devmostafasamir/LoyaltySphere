using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Infrastructure.Persistence.Interceptors;
using LoyaltySphere.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq.Expressions;

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
    private readonly ILogger<ApplicationDbContext> _logger;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext,
        ILoggerFactory? loggerFactory = null)
        : base(options)
    {
        _tenantContext = tenantContext;
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        _logger = _loggerFactory.CreateLogger<ApplicationDbContext>() ?? NullLogger<ApplicationDbContext>.Instance;
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

        // Multi-tenancy is handled via TenantInterceptor for saving
        // and database-level RLS policies for querying.
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Add interceptors - create loggers from factory (handle null/broken factory for tests)
        // Use non-generic CreateLogger to avoid broken Logger<T> wrappers that crash when inner logger is null
        var tenantLogger = _loggerFactory.CreateLogger(typeof(TenantInterceptor).FullName!) ?? NullLogger.Instance;
        var outboxLogger = _loggerFactory.CreateLogger(typeof(OutboxInterceptor).FullName!) ?? NullLogger.Instance;
        
        optionsBuilder.AddInterceptors(
            new TenantInterceptor(_tenantContext, tenantLogger),
            new OutboxInterceptor(outboxLogger));

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
            
            _logger.LogDebug("Setting PostgreSQL tenant context: {TenantId}", _tenantContext.TenantId);
            
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
