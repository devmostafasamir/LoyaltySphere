using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LoyaltySphere.MultiTenancy.Middleware;

/// <summary>
/// Middleware that resolves the tenant for each incoming HTTP request.
/// Extracts tenant identifier from headers, query string, or subdomain.
/// Sets the tenant context for the entire request pipeline.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    // Header name for tenant identification
    private const string TenantHeaderName = "X-Tenant-Id";
    
    // Query parameter name for tenant identification
    private const string TenantQueryParam = "tenantId";

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Try to resolve tenant from multiple sources (priority order)
        var tenantId = ResolveTenantId(context);

        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant identifier found in request. Path: {Path}", context.Request.Path);
            
            // For public endpoints, allow anonymous access
            if (IsPublicEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Return 400 Bad Request for protected endpoints without tenant
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Tenant identifier is required",
                message = $"Please provide tenant ID via '{TenantHeaderName}' header or '{TenantQueryParam}' query parameter"
            });
            return;
        }

        // Validate tenant exists and is active
        var tenantInfo = await ValidateTenantAsync(tenantId);
        if (tenantInfo == null)
        {
            _logger.LogWarning("Invalid or inactive tenant: {TenantId}", tenantId);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid tenant",
                message = "The specified tenant does not exist or is inactive"
            });
            return;
        }

        // Set tenant context for the request
        if (tenantContext is TenantContext concreteContext)
        {
            concreteContext.SetTenant(tenantInfo);
        }
        
        _logger.LogInformation(
            "Tenant resolved: {TenantId} - {TenantName}",
            tenantInfo.Id,
            tenantInfo.Name);

        // Add tenant ID to response headers for debugging
        context.Response.Headers.Append("X-Tenant-Resolved", tenantInfo.Id);

        await _next(context);
    }

    /// <summary>
    /// Resolves tenant ID from request in priority order:
    /// 1. HTTP Header (X-Tenant-Id)
    /// 2. Query parameter (tenantId)
    /// 3. Subdomain (tenant.loyaltysphere.com)
    /// 4. JWT claim (tenant_id)
    /// </summary>
    private string? ResolveTenantId(HttpContext context)
    {
        // 1. Check HTTP header (most common for API calls)
        if (context.Request.Headers.TryGetValue(TenantHeaderName, out var headerValue))
        {
            var tenantId = headerValue.ToString();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                _logger.LogDebug("Tenant resolved from header: {TenantId}", tenantId);
                return tenantId;
            }
        }

        // 2. Check query parameter (useful for webhooks and callbacks)
        if (context.Request.Query.TryGetValue(TenantQueryParam, out var queryValue))
        {
            var tenantId = queryValue.ToString();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                _logger.LogDebug("Tenant resolved from query: {TenantId}", tenantId);
                return tenantId;
            }
        }

        // 3. Check subdomain (e.g., nationalbank.loyaltysphere.com)
        var host = context.Request.Host.Host;
        if (!string.IsNullOrEmpty(host) && host.Contains('.'))
        {
            var subdomain = host.Split('.')[0];
            if (!IsReservedSubdomain(subdomain))
            {
                _logger.LogDebug("Tenant resolved from subdomain: {TenantId}", subdomain);
                return subdomain;
            }
        }

        // 4. Check JWT claims (if authenticated)
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrWhiteSpace(tenantClaim))
            {
                _logger.LogDebug("Tenant resolved from JWT claim: {TenantId}", tenantClaim);
                return tenantClaim;
            }
        }

        return null;
    }

    /// <summary>
    /// Validates that the tenant exists and is active.
    /// In production, this would query the database or cache.
    /// </summary>
    private async Task<TenantInfo?> ValidateTenantAsync(string tenantId)
    {
        // TODO: Replace with actual database lookup or cache
        // For now, return hardcoded tenants for demo purposes
        var validTenants = new Dictionary<string, TenantInfo>
        {
            ["national-bank"] = new TenantInfo
            {
                Id = "national-bank",
                Name = "National Bank of Egypt",
                IsActive = true,
                ConnectionString = "Host=localhost;Database=loyalty_sphere;Username=postgres;Password=postgres",
                Features = new[] { "instant-cashback", "pos-rewards", "campaigns" }
            },
            ["suez-bank"] = new TenantInfo
            {
                Id = "suez-bank",
                Name = "Suez Canal Bank",
                IsActive = true,
                ConnectionString = "Host=localhost;Database=loyalty_sphere;Username=postgres;Password=postgres",
                Features = new[] { "instant-cashback", "real-time-rewards" }
            },
            ["shell-egypt"] = new TenantInfo
            {
                Id = "shell-egypt",
                Name = "Shell Egypt",
                IsActive = true,
                ConnectionString = "Host=localhost;Database=loyalty_sphere;Username=postgres;Password=postgres",
                Features = new[] { "points-accumulation", "reward-catalog" }
            },
            ["kellogg"] = new TenantInfo
            {
                Id = "kellogg",
                Name = "Kellogg",
                IsActive = true,
                ConnectionString = "Host=localhost;Database=loyalty_sphere;Username=postgres;Password=postgres",
                Features = new[] { "campaigns", "customer-engagement" }
            }
        };

        await Task.CompletedTask; // Simulate async operation

        return validTenants.TryGetValue(tenantId.ToLowerInvariant(), out var tenant)
            ? tenant
            : null;
    }

    /// <summary>
    /// Checks if the endpoint is public and doesn't require tenant resolution.
    /// </summary>
    private bool IsPublicEndpoint(PathString path)
    {
        var publicPaths = new[]
        {
            "/health",
            "/metrics",
            "/swagger",
            "/api/auth/login",
            "/api/auth/register"
        };

        return publicPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if subdomain is reserved (www, api, admin, etc.)
    /// </summary>
    private bool IsReservedSubdomain(string subdomain)
    {
        var reserved = new[] { "www", "api", "admin", "localhost", "127" };
        return reserved.Contains(subdomain.ToLowerInvariant());
    }
}
