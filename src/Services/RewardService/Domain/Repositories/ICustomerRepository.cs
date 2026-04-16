using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;

namespace LoyaltySphere.RewardService.Domain.Repositories;

/// <summary>
/// Repository interface for Customer aggregate root.
/// Follows Repository pattern and Dependency Inversion Principle.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Gets a customer by their unique customer ID within the tenant.
    /// </summary>
    Task<Customer?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by their internal ID.
    /// </summary>
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all customers for the current tenant with optional filtering.
    /// </summary>
    Task<List<Customer>> GetAllAsync(
        string? tier = null,
        bool? isActive = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of customers for the current tenant.
    /// </summary>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of customers with optional filters.
    /// </summary>
    Task<int> CountAsync(string? tier = null, bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated customers with optional filters.
    /// </summary>
    Task<List<Customer>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? tier = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new customer to the repository.
    /// </summary>
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    void Update(Customer customer);

    /// <summary>
    /// Checks if a customer exists by customer ID.
    /// </summary>
    Task<bool> ExistsAsync(string customerId, CancellationToken cancellationToken = default);
}
