using Asp.Versioning;
using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Api.Contracts.RewardRules;
using LoyaltySphere.RewardService.Application.DTOs;
using LoyaltySphere.RewardService.Application.Mappers;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySphere.RewardService.Api.Controllers;

/// <summary>
/// Reward rules management API endpoints.
/// Requires admin role authorization.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/reward-rules")]
[Authorize(Roles = "Admin,TenantAdmin")]
public class RewardRulesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<RewardRulesController> _logger;

    public RewardRulesController(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<RewardRulesController> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets all reward rules for the current tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RewardRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RewardRuleDto>>> GetRewardRules(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var rules = await _unitOfWork.RewardRules
            .GetAllAsync(isActive, 0, 50, cancellationToken);

        var response = rules.Select(RewardRuleMapper.ToDto).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Gets a specific reward rule by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RewardRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RewardRuleDto>> GetRewardRule(
        Guid id,
        CancellationToken cancellationToken)
    {
        var rule = await _unitOfWork.RewardRules
            .GetByIdAsync(id, cancellationToken);

        if (rule == null)
            return NotFound();

        return Ok(RewardRuleMapper.ToDto(rule));
    }

    /// <summary>
    /// Creates a new reward rule.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RewardRuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RewardRuleDto>> CreateRewardRule(
        [FromBody] CreateRewardRuleRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating reward rule {RuleName}", request.RuleName);

        var rule = RewardRule.Create(
            _tenantContext.TenantId ?? string.Empty,
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

        await _unitOfWork.RewardRules.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reward rule {RuleId} created successfully", rule.Id);

        var response = RewardRuleMapper.ToDto(rule);
        return CreatedAtAction(nameof(GetRewardRule), new { id = rule.Id }, response);
    }

    /// <summary>
    /// Updates an existing reward rule.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RewardRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RewardRuleDto>> UpdateRewardRule(
        Guid id,
        [FromBody] UpdateRewardRuleRequest request,
        CancellationToken cancellationToken)
    {
        var rule = await _unitOfWork.RewardRules
            .GetByIdAsync(id, cancellationToken);

        if (rule == null)
            return NotFound();

        rule.Update(
            request.RuleName,
            request.Description,
            request.PointsPerUnit,
            request.Priority,
            request.MinimumTransactionAmount,
            request.MaximumTransactionAmount);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reward rule {RuleId} updated", id);

        return Ok(RewardRuleMapper.ToDto(rule));
    }

    /// <summary>
    /// Activates a reward rule.
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateRewardRule(
        Guid id,
        CancellationToken cancellationToken)
    {
        var rule = await _unitOfWork.RewardRules
            .GetByIdAsync(id, cancellationToken);

        if (rule == null)
            return NotFound();

        rule.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reward rule {RuleId} activated", id);
        return NoContent();
    }

    /// <summary>
    /// Deactivates a reward rule.
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateRewardRule(
        Guid id,
        CancellationToken cancellationToken)
    {
        var rule = await _unitOfWork.RewardRules
            .GetByIdAsync(id, cancellationToken);

        if (rule == null)
            return NotFound();

        rule.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reward rule {RuleId} deactivated", id);
        return NoContent();
    }
}
