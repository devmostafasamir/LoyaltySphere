using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.Repositories;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySphere.RewardService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ICustomerRepository.
/// Implements Repository pattern - infrastructure concern.
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Customer>> GetAllAsync(
        string? tier = null,
        bool? isActive = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tier))
        {
            query = query.Where(c => c.Tier == tier);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
    }

    public async Task<bool> ExistsAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AnyAsync(c => c.CustomerId == customerId, cancellationToken);
    }
}
