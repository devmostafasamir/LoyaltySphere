using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Domain.Enums;
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
        string? rewardType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Rewards
            .Where(r => r.CustomerExternalId == customerId && r.IsProcessed);

        if (!string.IsNullOrWhiteSpace(rewardType))
        {
            // Parse string to enum for comparison
            if (Enum.TryParse<RewardType>(rewardType, true, out var rewardTypeEnum))
            {
                query = query.Where(r => r.RewardType == rewardTypeEnum);
            }
        }

        if (startDate.HasValue)
        {
            query = query.Where(r => r.ProcessedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.ProcessedAt <= endDate.Value);
        }

        return await query
            .OrderByDescending(r => r.ProcessedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Reward>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Rewards
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .OrderByDescending(r => r.CreatedAt)
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
            .Where(r => r.CustomerExternalId == customerId);

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
            // Parse string to enum for comparison
            if (Enum.TryParse<RewardType>(rewardType, true, out var rewardTypeEnum))
            {
                query = query.Where(r => r.RewardType == rewardTypeEnum);
            }
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
