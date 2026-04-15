using LoyaltySphere.RewardService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySphere.RewardService.Application.Queries.GetCustomerBalance;

/// <summary>
/// Handles the GetCustomerBalanceQuery.
/// Retrieves customer balance and tier information.
/// </summary>
public class GetCustomerBalanceQueryHandler : IRequestHandler<GetCustomerBalanceQuery, CustomerBalanceResponse>
{
    private readonly ApplicationDbContext _context;

    public GetCustomerBalanceQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerBalanceResponse> Handle(
        GetCustomerBalanceQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.TenantId == request.TenantId && c.CustomerId == request.CustomerId,
                cancellationToken);

        if (customer == null)
        {
            throw new InvalidOperationException($"Customer {request.CustomerId} not found");
        }

        var (nextTierThreshold, progressPercentage) = CalculateTierProgress(
            customer.Tier,
            customer.LifetimePoints.Value);

        return new CustomerBalanceResponse
        {
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PointsBalance = customer.PointsBalance.Value,
            LifetimePoints = customer.LifetimePoints.Value,
            Tier = customer.Tier,
            EnrolledAt = customer.EnrolledAt,
            IsActive = customer.IsActive,
            NextTierThreshold = nextTierThreshold,
            ProgressToNextTier = progressPercentage
        };
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
