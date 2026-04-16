using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace LoyaltySphere.RewardService.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation using EF Core.
/// Manages transactions and coordinates repository operations.
/// Implements Dependency Inversion Principle - infrastructure implements domain contract.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy initialization of repositories
    private ICustomerRepository? _customers;
    private IRewardRepository? _rewards;
    private ICampaignRepository? _campaigns;
    private IRewardRuleRepository? _rewardRules;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public ICustomerRepository Customers
    {
        get
        {
            _customers ??= new CustomerRepository(_context);
            return _customers;
        }
    }

    public IRewardRepository Rewards
    {
        get
        {
            _rewards ??= new RewardRepository(_context);
            return _rewards;
        }
    }

    public ICampaignRepository Campaigns
    {
        get
        {
            _campaigns ??= new CampaignRepository(_context);
            return _campaigns;
        }
    }

    public IRewardRuleRepository RewardRules
    {
        get
        {
            _rewardRules ??= new RewardRuleRepository(_context);
            return _rewardRules;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
