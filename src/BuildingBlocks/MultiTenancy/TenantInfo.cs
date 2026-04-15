namespace LoyaltySphere.MultiTenancy;

/// <summary>
/// Represents tenant configuration and metadata.
/// Contains all information needed to isolate and configure tenant-specific behavior.
/// </summary>
public class TenantInfo
{
    /// <summary>
    /// Unique identifier for the tenant (e.g., "national-bank", "suez-bank")
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display name of the tenant (e.g., "National Bank of Egypt")
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Indicates if the tenant is active and can process requests
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Database connection string for this tenant.
    /// In shared database approach, this is the same for all tenants.
    /// In database-per-tenant approach, each tenant has its own connection string.
    /// </summary>
    public required string ConnectionString { get; init; }

    /// <summary>
    /// Feature flags enabled for this tenant
    /// </summary>
    public string[] Features { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Custom configuration settings for this tenant (JSON)
    /// </summary>
    public Dictionary<string, object>? Settings { get; init; }

    /// <summary>
    /// Tenant creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
