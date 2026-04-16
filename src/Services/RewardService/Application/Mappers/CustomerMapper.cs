using LoyaltySphere.RewardService.Application.DTOs;
using LoyaltySphere.RewardService.Domain.Entities;

namespace LoyaltySphere.RewardService.Application.Mappers;

/// <summary>
/// Mapper for Customer entity to DTOs.
/// Follows Single Responsibility Principle - only handles Customer mapping.
/// </summary>
public static class CustomerMapper
{
    /// <summary>
    /// Maps Customer entity to CustomerDto.
    /// </summary>
    public static CustomerDto ToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            CustomerId = customer.CustomerId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Tier = customer.Tier.ToString(),
            PointsBalance = customer.PointsBalance.Value,
            LifetimePoints = customer.LifetimePoints.Value,
            IsActive = customer.IsActive,
            EnrolledAt = customer.EnrolledAt,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    /// <summary>
    /// Maps collection of Customer entities to PagedCustomersDto.
    /// </summary>
    public static PagedCustomersDto ToPagedDto(
        List<Customer> customers,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedCustomersDto
        {
            Customers = customers.Select(ToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }
}
