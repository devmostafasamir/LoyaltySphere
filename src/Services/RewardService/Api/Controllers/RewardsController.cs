using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Application.Commands.CalculateReward;
using LoyaltySphere.RewardService.Application.Commands.RedeemPoints;
using LoyaltySphere.RewardService.Application.Queries.GetCustomerBalance;
using LoyaltySphere.RewardService.Application.Queries.GetRewardHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySphere.RewardService.Api.Controllers;

/// <summary>
/// API endpoints for reward operations.
/// Handles reward calculation, redemption, and balance queries.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class RewardsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<RewardsController> _logger;

    public RewardsController(
        IMediator mediator,
        ITenantContext tenantContext,
        ILogger<RewardsController> logger)
    {
        _mediator = mediator;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Calculates and awards reward points for a transaction.
    /// This is the main endpoint called by POS systems.
    /// </summary>
    /// <param name="request">Transaction details</param>
    /// <returns>Reward calculation result with points awarded</returns>
    /// <response code="200">Reward calculated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">Customer not found</response>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(CalculateRewardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CalculateRewardResponse>> CalculateReward(
        [FromBody] CalculateRewardRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Calculating reward for customer {CustomerId}, amount {Amount}",
            request.CustomerId,
            request.TransactionAmount);

        var command = new CalculateRewardCommand
        {
            TenantId = _tenantContext.TenantId,
            CustomerId = request.CustomerId,
            TransactionAmount = request.TransactionAmount,
            Currency = request.Currency ?? "EGP",
            TransactionId = request.TransactionId,
            MerchantId = request.MerchantId,
            MerchantCategory = request.MerchantCategory,
            ProductCategory = request.ProductCategory,
            TransactionDate = request.TransactionDate ?? DateTime.UtcNow
        };

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Redeems loyalty points for rewards.
    /// </summary>
    /// <param name="request">Redemption details</param>
    /// <returns>Redemption result with new balance</returns>
    /// <response code="200">Points redeemed successfully</response>
    /// <response code="400">Insufficient points or invalid request</response>
    [HttpPost("redeem")]
    [ProducesResponseType(typeof(RedeemPointsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RedeemPointsResponse>> RedeemPoints(
        [FromBody] RedeemPointsRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Redeeming {Points} points for customer {CustomerId}",
            request.PointsToRedeem,
            request.CustomerId);

        var command = new RedeemPointsCommand
        {
            TenantId = _tenantContext.TenantId,
            CustomerId = request.CustomerId,
            PointsToRedeem = request.PointsToRedeem,
            RedemptionType = request.RedemptionType,
            RedemptionDetails = request.RedemptionDetails
        };

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Gets customer's current points balance and tier information.
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>Customer balance and tier details</returns>
    /// <response code="200">Balance retrieved successfully</response>
    /// <response code="404">Customer not found</response>
    [HttpGet("balance/{customerId}")]
    [ProducesResponseType(typeof(CustomerBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerBalanceResponse>> GetBalance(
        string customerId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting balance for customer {CustomerId}", customerId);

        var query = new GetCustomerBalanceQuery
        {
            TenantId = _tenantContext.TenantId,
            CustomerId = customerId
        };

        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Gets customer's reward transaction history.
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="rewardType">Filter by reward type (optional)</param>
    /// <param name="fromDate">Filter from date (optional)</param>
    /// <param name="toDate">Filter to date (optional)</param>
    /// <returns>Paginated reward history</returns>
    /// <response code="200">History retrieved successfully</response>
    [HttpGet("history/{customerId}")]
    [ProducesResponseType(typeof(RewardHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RewardHistoryResponse>> GetHistory(
        string customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? rewardType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting history for customer {CustomerId}, page {Page}",
            customerId,
            pageNumber);

        var query = new GetRewardHistoryQuery
        {
            TenantId = _tenantContext.TenantId,
            CustomerId = customerId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            RewardType = rewardType,
            FromDate = fromDate,
            ToDate = toDate
        };

        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Health check endpoint for the rewards service.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "reward-service",
            timestamp = DateTime.UtcNow,
            tenant = _tenantContext.HasTenant ? _tenantContext.TenantId : "none"
        });
    }
}

// ============================================
// Request DTOs
// ============================================

/// <summary>
/// Request to calculate reward for a transaction.
/// </summary>
public record CalculateRewardRequest
{
    public required string CustomerId { get; init; }
    public required decimal TransactionAmount { get; init; }
    public string? Currency { get; init; }
    public string? TransactionId { get; init; }
    public string? MerchantId { get; init; }
    public string? MerchantCategory { get; init; }
    public string? ProductCategory { get; init; }
    public DateTime? TransactionDate { get; init; }
}

/// <summary>
/// Request to redeem loyalty points.
/// </summary>
public record RedeemPointsRequest
{
    public required string CustomerId { get; init; }
    public required decimal PointsToRedeem { get; init; }
    public required string RedemptionType { get; init; }
    public string? RedemptionDetails { get; init; }
}
