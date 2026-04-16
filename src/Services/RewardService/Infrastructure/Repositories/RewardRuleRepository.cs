using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySphere.RewardService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IRewardRuleRepository.
/// Implements Repository pattern - infrastructure concern.
/// </summary>
public class RewardRuleRepository : IRewardRuleRepository
{
    private readonly ApplicationDbContext _context;

    public RewardRuleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RewardRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RewardRules
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<RewardRule>> GetAllAsync(
        bool? isActive = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.RewardRules.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        return await query
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RewardRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.RewardRules
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RewardRule>> GetActiveByTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RewardRules
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.RewardRules.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(RewardRule rule, CancellationToken cancellationToken = default)
    {
        await _context.RewardRules.AddAsync(rule, cancellationToken);
    }

    public void Update(RewardRule rule)
    {
        _context.RewardRules.Update(rule);
    }

    public void Delete(RewardRule rule)
    {
        _context.RewardRules.Remove(rule);
    }
}
