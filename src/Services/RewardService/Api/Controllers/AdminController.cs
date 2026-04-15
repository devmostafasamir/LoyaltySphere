using Asp.Versioning;
using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySphere.RewardService.Api.Controllers;

/// <summary>
/// Admin API endpoints for tenant management, campaigns, and analytics.
/// Requires admin role authorization.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = "Admin,TenantAdmin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ApplicationDbContext context,
        ITenantContext tenantContext,
        ILogger<AdminController> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    // ============================================
    // CAMPAIGN MANAGEMENT
    // ============================================

    /// <summary>
    /// Gets all campaigns for the current tenant.
    /// </summary>
    [HttpGet("campaigns")]
    [ProducesResponseType(typeof(List<CampaignResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CampaignResponse>>> GetCampaigns(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Campaign>().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var response = campaigns.Select(MapToCampaignResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Gets a specific campaign by ID.
    /// </summary>
    [HttpGet("campaigns/{id}")]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CampaignResponse>> GetCampaign(
        Guid id,
        CancellationToken cancellationToken)
    {
        var campaign = await _context.Set<Campaign>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null)
            return NotFound();

        return Ok(MapToCampaignResponse(campaign));
    }

    /// <summary>
    /// Creates a new campaign.
    /// </summary>
    [HttpPost("campaigns")]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CampaignResponse>> CreateCampaign(
        [FromBody] CreateCampaignRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating campaign {CampaignName}", request.CampaignName);

        Campaign campaign = request.CampaignType switch
        {
            "Bonus" => Campaign.CreateBonusCampaign(
                _tenantContext.TenantId,
                request.CampaignName,
                request.Description,
                request.BonusPoints ?? 0,
                request.StartDate,
                request.EndDate,
                request.TargetCustomerSegment,
                request.MinimumTransactionAmount,
                request.MaxParticipations,
                request.Terms),

            "Multiplier" => Campaign.CreateMultiplierCampaign(
                _tenantContext.TenantId,
                request.CampaignName,
                request.Description,
                request.PointsMultiplier ?? 1,
                request.StartDate,
                request.EndDate,
                request.TargetMerchantCategory,
                request.MinimumTransactionAmount,
                request.Terms),

            "Cashback" => Campaign.CreateCashbackCampaign(
                _tenantContext.TenantId,
                request.CampaignName,
                request.Description,
                request.CashbackPercentage ?? 0,
                request.StartDate,
                request.EndDate,
                request.TargetMerchantCategory,
                request.MinimumTransactionAmount,
                request.Terms),

            _ => throw new ArgumentException($"Invalid campaign type: {request.CampaignType}")
        };

        _context.Set<Campaign>().Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaign {CampaignId} created successfully", campaign.Id);

        var response = MapToCampaignResponse(campaign);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, response);
    }

    /// <summary>
    /// Activates a campaign.
    /// </summary>
    [HttpPost("campaigns/{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateCampaign(
        Guid id,
        CancellationToken cancellationToken)
    {
        var campaign = await _context.Set<Campaign>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null)
            return NotFound();

        campaign.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaign {CampaignId} activated", id);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a campaign.
    /// </summary>
    [HttpPost("campaigns/{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateCampaign(
        Guid id,
        CancellationToken cancellationToken)
    {
        var campaign = await _context.Set<Campaign>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null)
            return NotFound();

        campaign.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaign {CampaignId} deactivated", id);
        return NoContent();
    }

    // ============================================
    // REWARD RULES MANAGEMENT
    // ============================================

    /// <summary>
    /// Gets all reward rules for the current tenant.
    /// </summary>
    [HttpGet("reward-rules")]
    [ProducesResponseType(typeof(List<RewardRuleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RewardRuleResponse>>> GetRewardRules(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<RewardRule>().AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        var rules = await query
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.RuleName)
            .ToListAsync(cancellationToken);

        var response = rules.Select(MapToRewardRuleResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Creates a new reward rule.
    /// </summary>
    [HttpPost("reward-rules")]
    [ProducesResponseType(typeof(RewardRuleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RewardRuleResponse>> CreateRewardRule(
        [FromBody] CreateRewardRuleRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating reward rule {RuleName}", request.RuleName);

        var rule = RewardRule.Create(
            _tenantContext.TenantId,
            request.RuleName,
            request.Description,
            request.PointsPerUnit,
            request.RuleType ?? "Standard",
            request.Priority,
            request.MinimumTransactionAmount,
            request.MaximumTransactionAmount,
            request.MerchantCategory,
            request.MerchantId,
            request.ProductCategory,
            request.ValidFrom,
            request.ValidUntil);

        _context.Set<RewardRule>().Add(rule);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reward rule {RuleId} created successfully", rule.Id);

        var response = MapToRewardRuleResponse(rule);
        return CreatedAtAction(nameof(GetRewardRules), new { id = rule.Id }, response);
    }

    /// <summary>
    /// Updates an existing reward rule.
    /// </summary>
    [HttpPut("reward-rules/{id}")]
    [ProducesResponseType(typeof(RewardRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RewardRuleResponse>> UpdateRewardRule(
        Guid id,
        [FromBody] UpdateRewardRuleRequest request,
        CancellationToken cancellationToken)
    {
        var rule = await _context.Set<RewardRule>()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule == null)
            return NotFound();

        rule.Update(
            request.RuleName,
            request.Description,
            request.PointsPerUnit,
            request.Priority,
            request.MinimumTransactionAmount,
            request.MaximumTransactionAmount);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reward rule {RuleId} updated", id);

        return Ok(MapToRewardRuleResponse(rule));
    }

    /// <summary>
    /// Activates a reward rule.
    /// </summary>
    [HttpPost("reward-rules/{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateRewardRule(
        Guid id,
        CancellationToken cancellationToken)
    {
        var rule = await _context.Set<RewardRule>()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule == null)
            return NotFound();

        rule.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reward rule {RuleId} activated", id);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a reward rule.
    /// </summary>
    [HttpPost("reward-rules/{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateRewardRule(
        Guid id,
        CancellationToken cancellationToken)
    {
        var rule = await _context.Set<RewardRule>()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (rule == null)
            return NotFound();

        rule.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reward rule {RuleId} deactivated", id);
        return NoContent();
    }

    // ============================================
    // ANALYTICS & DASHBOARD
    // ============================================

    /// <summary>
    /// Gets dashboard analytics for the current tenant.
    /// </summary>
    [HttpGet("analytics/dashboard")]
    [ProducesResponseType(typeof(DashboardAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardAnalyticsResponse>> GetDashboardAnalytics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        // Total customers
        var totalCustomers = await _context.Customers.CountAsync(cancellationToken);
        var activeCustomers = await _context.Customers
            .CountAsync(c => c.IsActive, cancellationToken);

        // Total points awarded and redeemed
        var rewards = await _context.Set<Reward>()
            .Where(r => r.CreatedAt >= from && r.CreatedAt <= to)
            .ToListAsync(cancellationToken);

        var totalPointsAwarded = rewards
            .Where(r => r.RewardType == "Earned")
            .Sum(r => r.PointsAwarded.Value);

        var totalPointsRedeemed = rewards
            .Where(r => r.RewardType == "Redeemed")
            .Sum(r => r.PointsAwarded.Value);

        // Active campaigns
        var activeCampaigns = await _context.Set<Campaign>()
            .CountAsync(c => c.IsActive, cancellationToken);

        // Customer tier distribution
        var tierDistribution = await _context.Customers
            .GroupBy(c => c.Tier)
            .Select(g => new TierDistribution
            {
                Tier = g.Key,
                Count = g.Count(),
                TotalPoints = g.Sum(c => c.PointsBalance.Value)
            })
            .ToListAsync(cancellationToken);

        // Recent transactions (last 7 days)
        var recentTransactions = await _context.Set<Reward>()
            .Where(r => r.CreatedAt >= DateTime.UtcNow.AddDays(-7))
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new DailyTransaction
            {
                Date = g.Key,
                Count = g.Count(),
                TotalPoints = g.Sum(r => r.PointsAwarded.Value)
            })
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        var response = new DashboardAnalyticsResponse
        {
            TotalCustomers = totalCustomers,
            ActiveCustomers = activeCustomers,
            TotalPointsAwarded = totalPointsAwarded,
            TotalPointsRedeemed = totalPointsRedeemed,
            ActiveCampaigns = activeCampaigns,
            TierDistribution = tierDistribution,
            RecentTransactions = recentTransactions,
            FromDate = from,
            ToDate = to
        };

        return Ok(response);
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private static CampaignResponse MapToCampaignResponse(Campaign campaign)
    {
        return new CampaignResponse
        {
            Id = campaign.Id,
            CampaignName = campaign.CampaignName,
            Description = campaign.Description,
            CampaignType = campaign.CampaignType,
            BonusPoints = campaign.BonusPoints,
            PointsMultiplier = campaign.PointsMultiplier,
            CashbackPercentage = campaign.CashbackPercentage,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            IsActive = campaign.IsActive,
            TargetCustomerSegment = campaign.TargetCustomerSegment,
            TargetMerchantCategory = campaign.TargetMerchantCategory,
            MinimumTransactionAmount = campaign.MinimumTransactionAmount,
            MaxParticipations = campaign.MaxParticipations,
            CurrentParticipations = campaign.CurrentParticipations,
            Terms = campaign.Terms,
            CreatedAt = campaign.CreatedAt
        };
    }

    private static RewardRuleResponse MapToRewardRuleResponse(RewardRule rule)
    {
        return new RewardRuleResponse
        {
            Id = rule.Id,
            RuleName = rule.RuleName,
            Description = rule.Description,
            PointsPerUnit = rule.PointsPerUnit,
            MinimumTransactionAmount = rule.MinimumTransactionAmount,
            MaximumTransactionAmount = rule.MaximumTransactionAmount,
            MerchantCategory = rule.MerchantCategory,
            MerchantId = rule.MerchantId,
            ProductCategory = rule.ProductCategory,
            Priority = rule.Priority,
            IsActive = rule.IsActive,
            ValidFrom = rule.ValidFrom,
            ValidUntil = rule.ValidUntil,
            RuleType = rule.RuleType,
            CreatedAt = rule.CreatedAt
        };
    }
}

// ============================================
// REQUEST/RESPONSE DTOs
// ============================================

public record CreateCampaignRequest
{
    public required string CampaignName { get; init; }
    public required string Description { get; init; }
    public required string CampaignType { get; init; } // Bonus, Multiplier, Cashback
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public decimal? BonusPoints { get; init; }
    public decimal? PointsMultiplier { get; init; }
    public decimal? CashbackPercentage { get; init; }
    public string? TargetCustomerSegment { get; init; }
    public string? TargetMerchantCategory { get; init; }
    public decimal? MinimumTransactionAmount { get; init; }
    public int? MaxParticipations { get; init; }
    public string? Terms { get; init; }
}

public record CampaignResponse
{
    public Guid Id { get; init; }
    public string CampaignName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CampaignType { get; init; } = string.Empty;
    public decimal BonusPoints { get; init; }
    public decimal? PointsMultiplier { get; init; }
    public decimal? CashbackPercentage { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsActive { get; init; }
    public string? TargetCustomerSegment { get; init; }
    public string? TargetMerchantCategory { get; init; }
    public decimal? MinimumTransactionAmount { get; init; }
    public int? MaxParticipations { get; init; }
    public int CurrentParticipations { get; init; }
    public string? Terms { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateRewardRuleRequest
{
    public required string RuleName { get; init; }
    public required string Description { get; init; }
    public required decimal PointsPerUnit { get; init; }
    public string? RuleType { get; init; }
    public int Priority { get; init; }
    public decimal? MinimumTransactionAmount { get; init; }
    public decimal? MaximumTransactionAmount { get; init; }
    public string? MerchantCategory { get; init; }
    public string? MerchantId { get; init; }
    public string? ProductCategory { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
}

public record UpdateRewardRuleRequest
{
    public required string RuleName { get; init; }
    public required string Description { get; init; }
    public required decimal PointsPerUnit { get; init; }
    public int Priority { get; init; }
    public decimal? MinimumTransactionAmount { get; init; }
    public decimal? MaximumTransactionAmount { get; init; }
}

public record RewardRuleResponse
{
    public Guid Id { get; init; }
    public string RuleName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal PointsPerUnit { get; init; }
    public decimal? MinimumTransactionAmount { get; init; }
    public decimal? MaximumTransactionAmount { get; init; }
    public string? MerchantCategory { get; init; }
    public string? MerchantId { get; init; }
    public string? ProductCategory { get; init; }
    public int Priority { get; init; }
    public bool IsActive { get; init; }
    public DateTime? ValidFrom { get; init; }
    public DateTime? ValidUntil { get; init; }
    public string RuleType { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record DashboardAnalyticsResponse
{
    public int TotalCustomers { get; init; }
    public int ActiveCustomers { get; init; }
    public decimal TotalPointsAwarded { get; init; }
    public decimal TotalPointsRedeemed { get; init; }
    public int ActiveCampaigns { get; init; }
    public List<TierDistribution> TierDistribution { get; init; } = new();
    public List<DailyTransaction> RecentTransactions { get; init; } = new();
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
}

public record TierDistribution
{
    public string Tier { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal TotalPoints { get; init; }
}

public record DailyTransaction
{
    public DateTime Date { get; init; }
    public int Count { get; init; }
    public decimal TotalPoints { get; init; }
}
