using LoyaltySphere.RewardService.Application.Interfaces;
using LoyaltySphere.RewardService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LoyaltySphere.RewardService.Application.Queries.GetCustomerBalance;

/// <summary>
/// Handles the GetCustomerBalanceQuery.
/// Retrieves customer balance and tier information with Redis cache-aside.
/// </summary>
public class GetCustomerBalanceQueryHandler : IRequestHandler<GetCustomerBalanceQuery, CustomerBalanceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetCustomerBalanceQueryHandler> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public GetCustomerBalanceQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<GetCustomerBalanceQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<CustomerBalanceResponse> Handle(
        GetCustomerBalanceQuery request,
        CancellationToken cancellationToken)
    {
        // Step 1: Try cache first (cache-aside pattern)
        var cacheKey = $"customer:balance:{request.CustomerId}";
        var cached = await _cacheService.GetAsync<CustomerBalanceResponse>(cacheKey, cancellationToken);

        if (cached != null)
        {
            _logger.LogDebug("Balance for {CustomerId} served from cache", request.CustomerId);
            return cached;
        }

        // Step 2: Cache miss — fetch from database
        var customer = await _unitOfWork.Customers
            .GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        if (customer == null)
        {
            throw new InvalidOperationException($"Customer {request.CustomerId} not found");
        }

        var (nextTierThreshold, progressPercentage) = CalculateTierProgress(
            customer.Tier.ToString(),
            customer.LifetimePoints.Value);

        var response = new CustomerBalanceResponse
        {
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PointsBalance = customer.PointsBalance.Value,
            LifetimePoints = customer.LifetimePoints.Value,
            Tier = customer.Tier.ToString(),
            EnrolledAt = customer.EnrolledAt,
            IsActive = customer.IsActive,
            NextTierThreshold = nextTierThreshold,
            ProgressToNextTier = progressPercentage
        };

        // Step 3: Write result to cache for subsequent reads
        await _cacheService.SetAsync(cacheKey, response, CacheDuration, cancellationToken);
        _logger.LogDebug("Balance for {CustomerId} cached for {Duration}", request.CustomerId, CacheDuration);

        return response;
    }

    private (decimal threshold, decimal progress) CalculateTierProgress(string currentTier, decimal lifetimePoints)
    {
        var thresholds = new Dictionary<string, decimal>
        {
            ["Bronze"] = 10000,
            ["Silver"] = 50000,
            ["Gold"] = 100000,
            ["Platinum"] = decimal.MaxValue
        };

        var nextThreshold = currentTier switch
        {
            "Bronze" => thresholds["Silver"],
            "Silver" => thresholds["Gold"],
            "Gold" => thresholds["Platinum"],
            "Platinum" => decimal.MaxValue,
            _ => thresholds["Silver"]
        };

        if (nextThreshold == decimal.MaxValue)
        {
            return (nextThreshold, 100m);
        }

        var previousThreshold = currentTier switch
        {
            "Silver" => thresholds["Bronze"],
            "Gold" => thresholds["Silver"],
            "Platinum" => thresholds["Gold"],
            _ => 0m
        };

        var progress = ((lifetimePoints - previousThreshold) / (nextThreshold - previousThreshold)) * 100m;
        return (nextThreshold, Math.Min(progress, 100m));
    }
}

