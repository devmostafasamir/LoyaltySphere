using LoyaltySphere.RewardService.Application.Services;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LoyaltySphere.RewardService.Application.Commands.RedeemPoints;

/// <summary>
/// Handles the RedeemPointsCommand.
/// Validates redemption and updates customer balance.
/// </summary>
public class RedeemPointsCommandHandler : IRequestHandler<RedeemPointsCommand, RedeemPointsResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRewardCalculationService _calculationService;
    private readonly ILogger<RedeemPointsCommandHandler> _logger;

    public RedeemPointsCommandHandler(
        IUnitOfWork unitOfWork,
        IRewardCalculationService calculationService,
        ILogger<RedeemPointsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _calculationService = calculationService;
        _logger = logger;
    }

    public async Task<RedeemPointsResponse> Handle(
        RedeemPointsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing redemption for customer {CustomerId}, points {Points}",
            request.CustomerId,
            request.PointsToRedeem);

        // Step 1: Get customer
        var customer = await _unitOfWork.Customers
            .GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        if (customer == null)
        {
            throw new InvalidOperationException($"Customer {request.CustomerId} not found");
        }

        // Step 2: Validate redemption
        var pointsToRedeem = Points.Create(request.PointsToRedeem);
        var validationResult = await _calculationService.ValidateRedemptionAsync(
            customer,
            pointsToRedeem,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Redemption validation failed: {Error}", validationResult.ErrorMessage);
            throw new InvalidOperationException(validationResult.ErrorMessage);
        }

        // Step 3: Create redemption reward
        var redemption = Reward.CreateRedeemed(
            request.TenantId,
            customer.Id,
            customer.CustomerId,
            pointsToRedeem,
            $"{request.RedemptionType}: {request.RedemptionDetails ?? "N/A"}");

        await _unitOfWork.Rewards.AddAsync(redemption, cancellationToken);

        // Step 4: Redeem points from customer
        customer.RedeemPoints(
            pointsToRedeem,
            $"Redemption: {request.RedemptionType}");

        // Step 5: Mark redemption as processed
        redemption.MarkAsProcessed();

        // Step 6: Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Redemption successful. Customer {CustomerId} redeemed {Points} points. Remaining balance: {Balance}",
            customer.CustomerId,
            pointsToRedeem,
            customer.PointsBalance);

        return new RedeemPointsResponse
        {
            RedemptionId = redemption.Id,
            CustomerId = customer.CustomerId,
            PointsRedeemed = pointsToRedeem.Value,
            RemainingBalance = customer.PointsBalance.Value,
            RedemptionType = request.RedemptionType,
            RedeemedAt = DateTime.UtcNow
        };
    }
}
