namespace LoyaltySphere.MultiTenancy;

/// <summary>
/// Provides access to the current tenant information.
/// </summary>
public interface ITenantContext
{
    string? TenantId { get; }
    TenantInfo? TenantInfo { get; }
    bool HasTenant { get; }
}
