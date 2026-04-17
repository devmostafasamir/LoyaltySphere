using Asp.Versioning;
using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Api.Contracts.Customers;
using LoyaltySphere.RewardService.Application.DTOs;
using LoyaltySphere.RewardService.Application.Mappers;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CustomersController> logger)
    {
        _unitOfWork = unitOfWork;
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
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> EnrollCustomer(
        [FromBody] EnrollCustomerRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Enrolling customer {CustomerId} in tenant {TenantId}",
            request.CustomerId,
            _tenantContext.TenantId);

        // Check if customer already exists
        var existingCustomer = await _unitOfWork.Customers
            .GetByCustomerIdAsync(request.CustomerId, cancellationToken);

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
            _tenantContext.TenantId ?? string.Empty,
            request.CustomerId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} enrolled successfully with ID {Id}",
            customer.CustomerId,
            customer.Id);

        var response = CustomerMapper.ToDto(customer);
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
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetCustomer(
        string customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers
            .GetByCustomerIdAsync(customerId, cancellationToken);

        if (customer == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Customer Not Found",
                Detail = $"Customer with ID {customerId} was not found",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(CustomerMapper.ToDto(customer));
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
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> UpdateCustomer(
        string customerId,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers
            .GetByCustomerIdAsync(customerId, cancellationToken);

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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer {CustomerId} updated successfully", customerId);

        return Ok(CustomerMapper.ToDto(customer));
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
        var customer = await _unitOfWork.Customers
            .GetByCustomerIdAsync(customerId, cancellationToken);

        if (customer == null)
        {
            return NotFound();
        }

        customer.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var customer = await _unitOfWork.Customers
            .GetByCustomerIdAsync(customerId, cancellationToken);

        if (customer == null)
        {
            return NotFound();
        }

        customer.Reactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
    [ProducesResponseType(typeof(PagedCustomersDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedCustomersDto>> GetCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? tier = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        // Get paginated customers with filters
        var customers = await _unitOfWork.Customers
            .GetPagedAsync(pageNumber, pageSize, tier, isActive, cancellationToken);

        // Get total count for pagination
        var totalCount = await _unitOfWork.Customers
            .CountAsync(tier, isActive, cancellationToken);

        var response = CustomerMapper.ToPagedDto(customers, totalCount, pageNumber, pageSize);

        return Ok(response);
    }
}
