using LoyaltySphere.RewardService.Domain.Entities;

namespace LoyaltySphere.RewardService.Domain.Repositories;

/// <summary>
/// Repository interface for Campaign aggregate root.
/// Follows Repository pattern and Dependency Inversion Principle.
/// </summary>
public interface ICampaignRepository
{
    /// <summary>
    /// Gets a campaign by its ID.
    /// </summary>
    Task<Campaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all campaigns for the current tenant with optional filtering.
    /// </summary>
    Task<List<Campaign>> GetAllAsync(
        bool? isActive = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets currently active campaigns for the current tenant.
    /// </summary>
    Task<List<Campaign>> GetActiveCampaignsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active campaigns within a date range.
    /// </summary>
    Task<List<Campaign>> GetActiveCampaignsAsync(
        string tenantId,
        DateTime currentDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of active campaigns.
    /// </summary>
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of campaigns for the current tenant.
    /// </summary>
    Task<int> GetCountAsync(bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new campaign to the repository.
    /// </summary>
    Task AddAsync(Campaign campaign, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing campaign.
    /// </summary>
    void Update(Campaign campaign);

    /// <summary>
    /// Deletes a campaign.
    /// </summary>
    void Delete(Campaign campaign);
}
