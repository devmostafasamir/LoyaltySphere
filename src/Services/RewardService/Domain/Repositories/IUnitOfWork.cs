namespace LoyaltySphere.RewardService.Domain.Repositories;

/// <summary>
/// Unit of Work pattern interface for managing transactions.
/// Follows Dependency Inversion Principle - domain defines the contract.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Customer repository.
    /// </summary>
    ICustomerRepository Customers { get; }

    /// <summary>
    /// Reward repository.
    /// </summary>
    IRewardRepository Rewards { get; }

    /// <summary>
    /// Campaign repository.
    /// </summary>
    ICampaignRepository Campaigns { get; }

    /// <summary>
    /// Reward rule repository.
    /// </summary>
    IRewardRuleRepository RewardRules { get; }

    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
