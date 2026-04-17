using Asp.Versioning;
using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Api.Contracts.Campaigns;
using LoyaltySphere.RewardService.Application.DTOs;
using LoyaltySphere.RewardService.Application.Mappers;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySphere.RewardService.Api.Controllers;

/// <summary>
/// Campaign management API endpoints.
/// Requires admin role authorization.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/campaigns")]
[Authorize(Roles = "Admin,TenantAdmin")]
public class CampaignsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CampaignsController> _logger;

    public CampaignsController(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CampaignsController> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets all campaigns for the current tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CampaignDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CampaignDto>>> GetCampaigns(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var campaigns = await _unitOfWork.Campaigns
            .GetAllAsync(isActive, 0, 50, cancellationToken);

        var response = campaigns.Select(CampaignMapper.ToDto).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Gets a specific campaign by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CampaignDto>> GetCampaign(
        Guid id,
        CancellationToken cancellationToken)
    {
        var campaign = await _unitOfWork.Campaigns
            .GetByIdAsync(id, cancellationToken);

        if (campaign == null)
            return NotFound();

        return Ok(CampaignMapper.ToDto(campaign));
    }

    /// <summary>
    /// Creates a new campaign.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CampaignDto>> CreateCampaign(
        [FromBody] CreateCampaignRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating campaign {CampaignName}", request.CampaignName);

        // Parse campaign type from string to enum
        if (!Enum.TryParse<CampaignType>(request.CampaignType, true, out var campaignType))
        {
            return BadRequest($"Invalid campaign type: {request.CampaignType}");
        }

        Campaign campaign = campaignType switch
        {
            CampaignType.Bonus => Campaign.CreateBonusCampaign(
                _tenantContext.TenantId ?? string.Empty,
                request.CampaignName,
                request.Description,
                request.BonusPoints ?? 0,
                request.StartDate,
                request.EndDate,
                request.TargetCustomerSegment,
                request.MinimumTransactionAmount,
                request.MaxParticipations,
                request.Terms),

            CampaignType.Multiplier => Campaign.CreateMultiplierCampaign(
                _tenantContext.TenantId ?? string.Empty,
                request.CampaignName,
                request.Description,
                request.PointsMultiplier ?? 1,
                request.StartDate,
                request.EndDate,
                request.TargetMerchantCategory,
                request.MinimumTransactionAmount,
                request.Terms),

            CampaignType.Cashback => Campaign.CreateCashbackCampaign(
                _tenantContext.TenantId ?? string.Empty,
                request.CampaignName,
                request.Description,
                request.CashbackPercentage ?? 0,
                request.StartDate,
                request.EndDate,
                request.TargetMerchantCategory,
                request.MinimumTransactionAmount,
                request.Terms),

            _ => throw new ArgumentException($"Invalid campaign type: {campaignType}")
        };

        await _unitOfWork.Campaigns.AddAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaign {CampaignId} created successfully", campaign.Id);

        var response = CampaignMapper.ToDto(campaign);
        return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, response);
    }

    /// <summary>
    /// Activates a campaign.
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateCampaign(
        Guid id,
        CancellationToken cancellationToken)
    {
        var campaign = await _unitOfWork.Campaigns
            .GetByIdAsync(id, cancellationToken);

        if (campaign == null)
            return NotFound();

        campaign.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaign {CampaignId} activated", id);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a campaign.
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateCampaign(
        Guid id,
        CancellationToken cancellationToken)
    {
        var campaign = await _unitOfWork.Campaigns
            .GetByIdAsync(id, cancellationToken);

        if (campaign == null)
            return NotFound();

        campaign.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Campaign {CampaignId} deactivated", id);
        return NoContent();
    }
}
