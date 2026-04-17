namespace LoyaltySphere.RewardService.Application.DTOs;

/// <summary>
/// Data Transfer Object for Customer information.
/// Used for API responses.
/// </summary>
public record CustomerDto
{
    public Guid Id { get; init; }
    public required string CustomerId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public required string Tier { get; init; }
    public required decimal PointsBalance { get; init; }
    public required decimal LifetimePoints { get; init; }
    public required bool IsActive { get; init; }
    public required DateTime EnrolledAt { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Paginated response for customer list.
/// </summary>
public record PagedCustomersDto
{
    public required List<CustomerDto> Customers { get; init; }
    public required int TotalCount { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required int TotalPages { get; init; }
}
