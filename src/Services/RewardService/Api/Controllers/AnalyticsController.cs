using Asp.Versioning;
using LoyaltySphere.RewardService.Application.DTOs;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySphere.RewardService.Api.Controllers;

/// <summary>
/// Analytics and dashboard API endpoints.
/// Requires admin role authorization.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/analytics")]
[Authorize(Roles = "Admin,TenantAdmin")]
public class AnalyticsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IUnitOfWork unitOfWork,
        ILogger<AnalyticsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Gets dashboard analytics for the current tenant.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardAnalyticsDto>> GetDashboardAnalytics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        _logger.LogInformation("Fetching dashboard analytics from {FromDate} to {ToDate}", from, to);

        // Total customers
        var totalCustomers = await _unitOfWork.Customers
            .CountAsync(null, null, cancellationToken);
        var activeCustomers = await _unitOfWork.Customers
            .CountAsync(null, true, cancellationToken);

        // Total points awarded and redeemed
        var rewards = await _unitOfWork.Rewards
            .GetByDateRangeAsync(from, to, cancellationToken);

        var totalPointsAwarded = rewards
            .Where(r => r.RewardType == RewardType.Earned)
            .Sum(r => r.PointsAwarded.Value);

        var totalPointsRedeemed = rewards
            .Where(r => r.RewardType == RewardType.Redeemed)
            .Sum(r => r.PointsAwarded.Value);

        // Active campaigns
        var activeCampaigns = await _unitOfWork.Campaigns
            .CountActiveAsync(cancellationToken);

        // Customer tier distribution
        var allCustomers = await _unitOfWork.Customers
            .GetAllAsync(null, null, 0, 10000, cancellationToken);
        
        var tierDistribution = allCustomers
            .GroupBy(c => c.Tier)
            .Select(g => new TierDistributionDto
            {
                Tier = g.Key.ToString(),
                Count = g.Count(),
                TotalPoints = g.Sum(c => c.PointsBalance.Value)
            })
            .ToList();

        // Recent transactions (last 7 days)
        var recentRewards = await _unitOfWork.Rewards
            .GetByDateRangeAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, cancellationToken);
        
        var recentTransactions = recentRewards
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new DailyTransactionDto
            {
                Date = g.Key,
                Count = g.Count(),
                TotalPoints = g.Sum(r => r.PointsAwarded.Value)
            })
            .OrderBy(d => d.Date)
            .ToList();

        var response = new DashboardAnalyticsDto
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

        _logger.LogInformation("Dashboard analytics retrieved successfully");
        return Ok(response);
    }

    /// <summary>
    /// Gets customer growth metrics over time.
    /// </summary>
    [HttpGet("customer-growth")]
    [ProducesResponseType(typeof(List<DailyTransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DailyTransactionDto>>> GetCustomerGrowth(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var customers = await _unitOfWork.Customers
            .GetAllAsync(null, null, 0, 10000, cancellationToken);

        var growth = customers
            .Where(c => c.CreatedAt >= from && c.CreatedAt <= to)
            .GroupBy(c => c.CreatedAt.Date)
            .Select(g => new DailyTransactionDto
            {
                Date = g.Key,
                Count = g.Count(),
                TotalPoints = g.Sum(c => c.PointsBalance.Value)
            })
            .OrderBy(d => d.Date)
            .ToList();

        return Ok(growth);
    }

    /// <summary>
    /// Gets campaign performance metrics.
    /// </summary>
    [HttpGet("campaign-performance")]
    [ProducesResponseType(typeof(List<CampaignPerformanceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CampaignPerformanceDto>>> GetCampaignPerformance(
        CancellationToken cancellationToken = default)
    {
        var campaigns = await _unitOfWork.Campaigns
            .GetAllAsync(null, 0, 50, cancellationToken);

        var performance = campaigns.Select(c => new CampaignPerformanceDto
        {
            CampaignId = c.Id,
            CampaignName = c.CampaignName,
            CampaignType = c.CampaignType.ToString(),
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            IsActive = c.IsActive,
            ParticipationCount = c.CurrentParticipations,
            BonusPoints = c.BonusPoints,
            PointsMultiplier = c.PointsMultiplier,
            CashbackPercentage = c.CashbackPercentage
        }).ToList();

        return Ok(performance);
    }
}

/// <summary>
/// Campaign performance metrics DTO.
/// </summary>
public class CampaignPerformanceDto
{
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string CampaignType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int ParticipationCount { get; set; }
    public decimal BonusPoints { get; set; }
    public decimal? PointsMultiplier { get; set; }
    public decimal? CashbackPercentage { get; set; }
}
