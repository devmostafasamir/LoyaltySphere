using LoyaltySphere.RewardService.Application.Services;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LoyaltySphere.RewardService.Application.Commands.CalculateReward;

/// <summary>
/// Handles the CalculateRewardCommand.
/// Orchestrates reward calculation, customer update, and persistence.
/// </summary>
public class CalculateRewardCommandHandler : IRequestHandler<CalculateRewardCommand, CalculateRewardResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRewardCalculationService _calculationService;
    private readonly ILogger<CalculateRewardCommandHandler> _logger;

    public CalculateRewardCommandHandler(
        IUnitOfWork unitOfWork,
        IRewardCalculationService calculationService,
        ILogger<CalculateRewardCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _calculationService = calculationService;
        _logger = logger;
    }

    public async Task<CalculateRewardResponse> Handle(
        CalculateRewardCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing reward calculation for customer {CustomerId}, amount {Amount}",
            request.CustomerId,
            request.TransactionAmount);

        // Step 1: Get or create customer
        var customer = await GetOrCreateCustomerAsync(request.TenantId, request.CustomerId, cancellationToken);

        // Step 2: Get applicable reward rules
        var applicableRules = await _unitOfWork.RewardRules
            .GetActiveByTenantAsync(request.TenantId, cancellationToken);

        // Step 3: Get active campaigns
        var now = DateTime.UtcNow;
        var activeCampaigns = await _unitOfWork.Campaigns
            .GetActiveCampaignsAsync(request.TenantId, now, cancellationToken);

        // Step 4: Calculate reward
        var transactionAmount = Money.Create(request.TransactionAmount, request.Currency);
        var calculationResult = await _calculationService.CalculateRewardAsync(
            customer,
            transactionAmount,
            applicableRules,
            activeCampaigns,
            request.MerchantId,
            request.MerchantCategory,
            cancellationToken);

        if (!calculationResult.IsSuccess)
        {
            _logger.LogWarning("Reward calculation failed: {Error}", calculationResult.ErrorMessage);
            throw new InvalidOperationException(calculationResult.ErrorMessage);
        }

        // Step 5: Create reward entity
        var reward = Reward.CreateEarned(
            request.TenantId,
            customer.Id,
            customer.CustomerId,
            calculationResult.TotalPoints,
            transactionAmount,
            "POS",
            $"Transaction reward: {calculationResult.AppliedRule?.RuleName ?? "Standard"}",
            request.TransactionId,
            request.MerchantId);

        await _unitOfWork.Rewards.AddAsync(reward, cancellationToken);

        // Step 6: Award points to customer
        customer.AwardPoints(
            calculationResult.TotalPoints,
            $"Transaction {request.TransactionId}",
            request.TransactionId);

        // Step 7: Record campaign participation if applicable
        if (calculationResult.AppliedCampaign != null)
        {
            calculationResult.AppliedCampaign.RecordParticipation();
        }

        // Step 8: Mark reward as processed
        reward.MarkAsProcessed();

        // Step 9: Save changes (this will publish domain events via outbox)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Reward calculated successfully. Customer {CustomerId} earned {Points} points. New balance: {Balance}",
            customer.CustomerId,
            calculationResult.TotalPoints,
            customer.PointsBalance);

        // Step 10: Return response
        return new CalculateRewardResponse
        {
            RewardId = reward.Id,
            CustomerId = customer.CustomerId,
            PointsAwarded = calculationResult.TotalPoints.Value,
            BasePoints = calculationResult.BasePoints.Value,
            CampaignBonus = calculationResult.CampaignBonus.Value,
            NewBalance = customer.PointsBalance.Value,
            CustomerTier = customer.Tier.ToString(),
            AppliedRuleName = calculationResult.AppliedRule?.RuleName,
            AppliedCampaignName = calculationResult.AppliedCampaign?.CampaignName,
            TierMultiplier = calculationResult.TierMultiplier,
            ProcessedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets existing customer or creates a new one if not found.
    /// </summary>
    private async Task<Customer> GetOrCreateCustomerAsync(
        string tenantId,
        string customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers
            .GetByCustomerIdAsync(customerId, cancellationToken);

        if (customer == null)
        {
            _logger.LogInformation("Creating new customer: {CustomerId}", customerId);

            customer = Customer.Create(
                tenantId,
                customerId,
                "Unknown", // First name - should be provided in real scenario
                "Customer", // Last name
                $"{customerId}@example.com"); // Email - should be provided

            await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return customer;
    }
}
