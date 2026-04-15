using LoyaltySphere.MultiTenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LoyaltySphere.RewardService.Infrastructure.SignalR;

/// <summary>
/// SignalR hub for real-time reward notifications.
/// Pushes live updates to connected clients when points are awarded or redeemed.
/// </summary>
[Authorize]
public class RewardHub : Hub
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<RewardHub> _logger;

    public RewardHub(
        ITenantContext tenantContext,
        ILogger<RewardHub> logger)
    {
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// Adds the client to their tenant and customer groups.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var customerId = Context.User?.FindFirst("customer_id")?.Value;

        if (_tenantContext.HasTenant)
        {
            // Add to tenant group for tenant-wide broadcasts
            await Groups.AddToGroupAsync(connectionId, $"tenant:{_tenantContext.TenantId}");
            
            _logger.LogInformation(
                "Client {ConnectionId} connected to tenant {TenantId}",
                connectionId,
                _tenantContext.TenantId);
        }

        if (!string.IsNullOrEmpty(customerId))
        {
            // Add to customer-specific group for targeted notifications
            await Groups.AddToGroupAsync(connectionId, $"customer:{customerId}");
            
            _logger.LogInformation(
                "Client {ConnectionId} connected for customer {CustomerId}",
                connectionId,
                customerId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation(
            "Client {ConnectionId} disconnected. Error: {Error}",
            connectionId,
            exception?.Message ?? "none");

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows clients to subscribe to specific customer updates.
    /// </summary>
    public async Task SubscribeToCustomer(string customerId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"customer:{customerId}");
        
        _logger.LogInformation(
            "Client {ConnectionId} subscribed to customer {CustomerId}",
            Context.ConnectionId,
            customerId);
    }

    /// <summary>
    /// Allows clients to unsubscribe from customer updates.
    /// </summary>
    public async Task UnsubscribeFromCustomer(string customerId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"customer:{customerId}");
        
        _logger.LogInformation(
            "Client {ConnectionId} unsubscribed from customer {CustomerId}",
            Context.ConnectionId,
            customerId);
    }
}

/// <summary>
/// Service for sending real-time notifications via SignalR.
/// Called by domain event handlers to push updates to clients.
/// </summary>
public interface IRewardNotificationService
{
    Task NotifyPointsAwardedAsync(
        string tenantId,
        string customerId,
        decimal pointsAwarded,
        decimal newBalance,
        string reason,
        CancellationToken cancellationToken = default);

    Task NotifyPointsRedeemedAsync(
        string tenantId,
        string customerId,
        decimal pointsRedeemed,
        decimal newBalance,
        string reason,
        CancellationToken cancellationToken = default);

    Task NotifyTierUpgradedAsync(
        string tenantId,
        string customerId,
        string previousTier,
        string newTier,
        decimal lifetimePoints,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of reward notification service using SignalR.
/// </summary>
public class RewardNotificationService : IRewardNotificationService
{
    private readonly IHubContext<RewardHub> _hubContext;
    private readonly ILogger<RewardNotificationService> _logger;

    public RewardNotificationService(
        IHubContext<RewardHub> hubContext,
        ILogger<RewardNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyPointsAwardedAsync(
        string tenantId,
        string customerId,
        decimal pointsAwarded,
        decimal newBalance,
        string reason,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending points awarded notification to customer {CustomerId}: {Points} points",
            customerId,
            pointsAwarded);

        var notification = new
        {
            type = "PointsAwarded",
            customerId,
            pointsAwarded,
            newBalance,
            reason,
            timestamp = DateTime.UtcNow
        };

        // Send to specific customer
        await _hubContext.Clients
            .Group($"customer:{customerId}")
            .SendAsync("PointsAwarded", notification, cancellationToken);

        // Also send to tenant group for admin dashboards
        await _hubContext.Clients
            .Group($"tenant:{tenantId}")
            .SendAsync("CustomerPointsAwarded", notification, cancellationToken);
    }

    public async Task NotifyPointsRedeemedAsync(
        string tenantId,
        string customerId,
        decimal pointsRedeemed,
        decimal newBalance,
        string reason,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending points redeemed notification to customer {CustomerId}: {Points} points",
            customerId,
            pointsRedeemed);

        var notification = new
        {
            type = "PointsRedeemed",
            customerId,
            pointsRedeemed,
            newBalance,
            reason,
            timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients
            .Group($"customer:{customerId}")
            .SendAsync("PointsRedeemed", notification, cancellationToken);

        await _hubContext.Clients
            .Group($"tenant:{tenantId}")
            .SendAsync("CustomerPointsRedeemed", notification, cancellationToken);
    }

    public async Task NotifyTierUpgradedAsync(
        string tenantId,
        string customerId,
        string previousTier,
        string newTier,
        decimal lifetimePoints,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending tier upgrade notification to customer {CustomerId}: {PreviousTier} → {NewTier}",
            customerId,
            previousTier,
            newTier);

        var notification = new
        {
            type = "TierUpgraded",
            customerId,
            previousTier,
            newTier,
            lifetimePoints,
            timestamp = DateTime.UtcNow,
            celebration = true // Triggers celebration animation in UI
        };

        await _hubContext.Clients
            .Group($"customer:{customerId}")
            .SendAsync("TierUpgraded", notification, cancellationToken);

        await _hubContext.Clients
            .Group($"tenant:{tenantId}")
            .SendAsync("CustomerTierUpgraded", notification, cancellationToken);
    }
}
