using LoyaltySphere.RewardService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace LoyaltySphere.RewardService.Domain.Services;

/// <summary>
/// Implementation of points capping logic.
/// Follows Single Responsibility Principle - only handles points caps.
/// </summary>
public class PointsCapService : IPointsCapService
{
    private readonly ILogger<PointsCapService> _logger;
    private const decimal MaxPointsPercentage = 0.10m; // 10% of transaction amount

    public PointsCapService(ILogger<PointsCapService> logger)
    {
        _logger = logger;
    }

    public Points ApplyPointsCap(Points points, Money transactionAmount)
    {
        var maxPoints = GetMaximumPoints(transactionAmount);

        if (points.IsGreaterThan(maxPoints))
        {
            _logger.LogWarning(
                "Points capped from {Original} to {Capped} (10% of transaction)",
                points,
                maxPoints);
            return maxPoints;
        }

        return points;
    }

    public Points GetMaximumPoints(Money transactionAmount)
    {
        return Points.Create(transactionAmount.Amount * MaxPointsPercentage);
    }
}
