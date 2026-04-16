using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySphere.RewardService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IRewardRepository.
/// Implements Repository pattern - infrastructure concern.
/// </summary>
public class RewardRepository : IRewardRepository
{
    private readonly ApplicationDbContext _context;

    public RewardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Reward?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Rewards
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<Reward>> GetByCustomerIdAsync(
        string customerId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? rewardType = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Rewards
            .Where(r => r.CustomerId == customerId);

        if (startDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(rewardType))
        {
            query = query.Where(r => r.RewardType == rewardType);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByCustomerIdAsync(
        string customerId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? rewardType = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Rewards
            .Where(r => r.CustomerId == customerId);

        if (startDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(rewardType))
        {
            query = query.Where(r => r.RewardType == rewardType);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Reward reward, CancellationToken cancellationToken = default)
    {
        await _context.Rewards.AddAsync(reward, cancellationToken);
    }

    public void Update(Reward reward)
    {
        _context.Rewards.Update(reward);
    }
}
