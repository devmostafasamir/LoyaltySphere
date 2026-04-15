namespace LoyaltySphere.MultiTenancy;

/// <summary>
/// Provides access to the current tenant context throughout the request pipeline.
/// Scoped service that maintains tenant information for the duration of the HTTP request.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the current tenant information.
    /// Returns null if no tenant has been resolved.
    /// </summary>
    TenantInfo? CurrentTenant { get; }

    /// <summary>
    /// Gets the current tenant ID.
    /// Throws if no tenant has been resolved.
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// Checks if a tenant has been resolved for the current request.
    /// </summary>
    bool HasTenant { get; }

    /// <summary>
    /// Sets the tenant for the current request.
    /// Should only be called by TenantResolutionMiddleware.
    /// </summary>
    void SetTenant(TenantInfo tenantInfo);
}
