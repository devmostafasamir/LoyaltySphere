using Asp.Versioning;
using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySphere.RewardService.Api.Controllers;

/// <summary>
/// API endpoints for customer management.
/// Handles customer enrollment, profile updates, and status management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ApplicationDbContext context,
        ITenantContext tenantContext,
        ILogger<CustomersController> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Enrolls a new customer in the loyalty program.
    /// </summary>
    /// <param name="request">Customer enrollment details</param>
    /// <returns>Created customer with initial balance</returns>
    /// <response code="201">Customer enrolled successfully</response>
    /// <response code="400">Invalid request or customer already exists</response>
    [HttpPost("enroll")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerResponse>> EnrollCustomer(
        [FromBody] EnrollCustomerRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Enrolling customer {CustomerId} in tenant {TenantId}",
            request.CustomerId,
            _tenantContext.TenantId);

        // Check if customer already exists
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (existingCustomer != null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Customer Already Exists",
                Detail = $"Customer with ID {request.CustomerId} is already enrolled",
                Status = StatusCodes.Status400BadRequest
            });
        }

        // Create new customer
        var customer = Customer.Create(
            _tenantContext.TenantId,
            request.CustomerId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} enrolled successfully with ID {Id}",
            customer.CustomerId,
            customer.Id);

        var response = MapToResponse(customer);
        return CreatedAtAction(nameof(GetCustomer), new { customerId = customer.CustomerId }, response);
    }

    /// <summary>
    /// Gets customer details by customer ID.
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>Customer details</returns>
    /// <response code="200">Customer found</response>
    /// <response code="404">Customer not found</response>
    [HttpGet("{customerId}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetCustomer(
        string customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

        if (customer == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Customer Not Found",
                Detail = $"Customer with ID {customerId} was not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(MapToResponse(customer));
    }

    /// <summary>
    /// Updates customer profile information.
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <param name="request">Updated customer information</param>
    /// <returns>Updated customer details</returns>
    /// <response code="200">Customer updated successfully</response>
    /// <response code="404">Customer not found</response>
    [HttpPut("{customerId}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> UpdateCustomer(
        string customerId,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

        if (customer == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Customer Not Found",
                Detail = $"Customer with ID {customerId} was not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        customer.UpdateInformation(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer {CustomerId} updated successfully", customerId);

        return Ok(MapToResponse(customer));
    }

    /// <summary>
    /// Deactivates a customer account.
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>No content</returns>
    /// <response code="204">Customer deactivated successfully</response>
    /// <response code="404">Customer not found</response>
    [HttpPost("{customerId}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateCustomer(
        string customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

        if (customer == null)
        {
            return NotFound();
        }

        customer.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer {CustomerId} deactivated", customerId);

        return NoContent();
    }

    /// <summary>
    /// Reactivates a customer account.
    /// </summary>
    /// <param name="customerId">Customer identifier</param>
    /// <returns>No content</returns>
    /// <response code="204">Customer reactivated successfully</response>
    /// <response code="404">Customer not found</response>
    [HttpPost("{customerId}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateCustomer(
        string customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);

        if (customer == null)
        {
            return NotFound();
        }

        customer.Reactivate();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer {CustomerId} reactivated", customerId);

        return NoContent();
    }

    /// <summary>
    /// Gets all customers for the current tenant (paginated).
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="tier">Filter by tier (optional)</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <returns>Paginated list of customers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedCustomersResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedCustomersResponse>> GetCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? tier = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(tier))
        {
            query = query.Where(c => c.Tier == tier);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated results
        var customers = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var response = new PagedCustomersResponse
        {
            Customers = customers.Select(MapToResponse).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };

        return Ok(response);
    }

    private static CustomerResponse MapToResponse(Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            PointsBalance = customer.PointsBalance.Value,
            LifetimePoints = customer.LifetimePoints.Value,
            Tier = customer.Tier,
            IsActive = customer.IsActive,
            EnrolledAt = customer.EnrolledAt,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}

// ============================================
// Request/Response DTOs
// ============================================

public record EnrollCustomerRequest
{
    public required string CustomerId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
}

public record UpdateCustomerRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
}

public record CustomerResponse
{
    public Guid Id { get; init; }
    public string CustomerId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public decimal PointsBalance { get; init; }
    public decimal LifetimePoints { get; init; }
    public string Tier { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime EnrolledAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record PagedCustomersResponse
{
    public List<CustomerResponse> Customers { get; init; } = new();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}
