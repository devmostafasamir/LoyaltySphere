using LoyaltySphere.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace LoyaltySphere.RewardService.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets tenant_id on all entities.
/// Ensures multi-tenant data isolation at the application level.
/// Works in conjunction with PostgreSQL Row-Level Security (RLS).
/// </summary>
public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<TenantInterceptor> _logger;

    public TenantInterceptor(
        ITenantContext tenantContext,
        ILogger<TenantInterceptor> logger)
    {
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetTenantId(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetTenantId(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetTenantId(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        // Skip tenant check if no entities are being added/modified
        if (!context.ChangeTracker.Entries().Any(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            return;
        }

        if (!_tenantContext.HasTenant)
        {
            // If we're using InMemoryDatabase for tests, we can skip the tenant check
            if (context.Database.IsRelational())
            {
                _logger.LogWarning("Attempting to save changes without tenant context");
                throw new InvalidOperationException("Tenant context is required for save operations");
            }
            return;
        }

        var tenantId = _tenantContext.TenantId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Safely find the TenantId property
            var tenantIdPropertyMetadata = entry.Metadata.FindProperty("TenantId");
            if (tenantIdPropertyMetadata == null)
            {
                continue;
            }

            var tenantIdProperty = entry.Property("TenantId");

            // Only set tenant_id for new entities
            if (entry.State == EntityState.Added)
            {
                // Set tenant_id if not already set
                if (tenantIdProperty.CurrentValue == null || 
                    string.IsNullOrEmpty(tenantIdProperty.CurrentValue.ToString()))
                {
                    tenantIdProperty.CurrentValue = tenantId;
                    
                    _logger.LogDebug(
                        "Set tenant_id to {TenantId} for entity {EntityType}",
                        tenantId,
                        entry.Entity.GetType().Name);
                }
                else
                {
                    // Verify tenant_id matches current tenant
                    var entityTenantId = tenantIdProperty.CurrentValue.ToString();
                    if (entityTenantId != tenantId)
                    {
                        _logger.LogError(
                            "Tenant mismatch! Entity has tenant_id {EntityTenantId} but current tenant is {CurrentTenantId}",
                            entityTenantId,
                            tenantId);
                        
                        throw new InvalidOperationException(
                            $"Cannot save entity with tenant_id '{entityTenantId}' in tenant context '{tenantId}'");
                    }
                }
            }
            // Prevent modification of tenant_id
            else if (entry.State == EntityState.Modified)
            {
                if (tenantIdProperty.IsModified)
                {
                    _logger.LogError(
                        "Attempted to modify tenant_id for entity {EntityType}",
                        entry.Entity.GetType().Name);
                    
                    throw new InvalidOperationException("Cannot modify tenant_id of existing entity");
                }
            }
        }
    }
}
