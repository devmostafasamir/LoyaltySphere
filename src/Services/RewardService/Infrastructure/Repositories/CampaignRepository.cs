using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySphere.RewardService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ICampaignRepository.
/// Implements Repository pattern - infrastructure concern.
/// </summary>
public class CampaignRepository : ICampaignRepository
{
    private readonly ApplicationDbContext _context;

    public CampaignRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Campaign>> GetAllAsync(
        bool? isActive = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Campaigns.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        return await query
            .OrderByDescending(c => c.Priority)
            .ThenByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Campaign>> GetActiveCampaignsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        return await _context.Campaigns
            .Where(c => c.IsActive 
                && c.StartDate <= now 
                && c.EndDate >= now)
            .OrderByDescending(c => c.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Campaigns.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default)
    {
        await _context.Campaigns.AddAsync(campaign, cancellationToken);
    }

    public void Update(Campaign campaign)
    {
        _context.Campaigns.Update(campaign);
    }

    public void Delete(Campaign campaign)
    {
        _context.Campaigns.Remove(campaign);
    }
}
