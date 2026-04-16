using LoyaltySphere.RewardService.Domain.Entities;

namespace LoyaltySphere.RewardService.Domain.Repositories;

/// <summary>
/// Repository interface for RewardRule aggregate root.
/// Follows Repository pattern and Dependency Inversion Principle.
/// </summary>
public interface IRewardRuleRepository
{
    /// <summary>
    /// Gets a reward rule by its ID.
    /// </summary>
    Task<RewardRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all reward rules for the current tenant with optional filtering.
    /// </summary>
    Task<List<RewardRule>> GetAllAsync(
        bool? isActive = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active reward rules for the current tenant.
    /// </summary>
    Task<List<RewardRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of reward rules for the current tenant.
    /// </summary>
    Task<int> GetCountAsync(bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new reward rule to the repository.
    /// </summary>
    Task AddAsync(RewardRule rule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing reward rule.
    /// </summary>
    void Update(RewardRule rule);

    /// <summary>
    /// Deletes a reward rule.
    /// </summary>
    void Delete(RewardRule rule);
}
