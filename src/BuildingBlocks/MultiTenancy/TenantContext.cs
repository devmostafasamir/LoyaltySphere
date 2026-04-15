namespace LoyaltySphere.MultiTenancy;

/// <summary>
/// Implementation of ITenantContext that stores tenant information for the current request.
/// Registered as a scoped service in DI container.
/// </summary>
public class TenantContext : ITenantContext
{
    private TenantInfo? _currentTenant;

    public TenantInfo? CurrentTenant => _currentTenant;

    public string TenantId => _currentTenant?.Id 
        ?? throw new InvalidOperationException("Tenant has not been resolved for this request");

    public bool HasTenant => _currentTenant != null;

    public void SetTenant(TenantInfo tenantInfo)
    {
        if (_currentTenant != null)
        {
            throw new InvalidOperationException("Tenant has already been set for this request");
        }

        _currentTenant = tenantInfo ?? throw new ArgumentNullException(nameof(tenantInfo));
    }
}
