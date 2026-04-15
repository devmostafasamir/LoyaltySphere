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

        if (!_tenantContext.HasTenant)
        {
            _logger.LogWarning("Attempting to save changes without tenant context");
            throw new InvalidOperationException("Tenant context is required for save operations");
        }

        var tenantId = _tenantContext.TenantId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Only set tenant_id for new entities
            if (entry.State == EntityState.Added)
            {
                var tenantIdProperty = entry.Property("TenantId");
                
                if (tenantIdProperty != null)
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
            }
            // Prevent modification of tenant_id
            else if (entry.State == EntityState.Modified)
            {
                var tenantIdProperty = entry.Property("TenantId");
                
                if (tenantIdProperty != null && tenantIdProperty.IsModified)
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
