using MediatR;

namespace LoyaltySphere.RewardService.Application.Queries.GetCustomerBalance;

/// <summary>
/// Query to get customer's current points balance and tier information.
/// </summary>
public record GetCustomerBalanceQuery : IRequest<CustomerBalanceResponse>
{
    public required string TenantId { get; init; }
    public required string CustomerId { get; init; }
}

/// <summary>
/// Response containing customer balance information.
/// </summary>
public record CustomerBalanceResponse
{
    public required string CustomerId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required decimal PointsBalance { get; init; }
    public required decimal LifetimePoints { get; init; }
    public required string Tier { get; init; }
    public required DateTime EnrolledAt { get; init; }
    public required bool IsActive { get; init; }
    public decimal NextTierThreshold { get; init; }
    public decimal ProgressToNextTier { get; init; }
}
