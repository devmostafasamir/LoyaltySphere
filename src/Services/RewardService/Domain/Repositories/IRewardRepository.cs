using LoyaltySphere.RewardService.Domain.Entities;

namespace LoyaltySphere.RewardService.Domain.Repositories;

/// <summary>
/// Repository interface for Reward aggregate root.
/// Follows Repository pattern and Dependency Inversion Principle.
/// </summary>
public interface IRewardRepository
{
    /// <summary>
    /// Gets a reward by its ID.
    /// </summary>
    Task<Reward?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets reward history for a specific customer.
    /// </summary>
    Task<List<Reward>> GetByCustomerIdAsync(
        string customerId,
        string? rewardType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets rewards by date range.
    /// </summary>
    Task<List<Reward>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of rewards for a customer.
    /// </summary>
    Task<int> GetCountByCustomerIdAsync(
        string customerId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? rewardType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new reward to the repository.
    /// </summary>
    Task AddAsync(Reward reward, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing reward.
    /// </summary>
    void Update(Reward reward);
}
