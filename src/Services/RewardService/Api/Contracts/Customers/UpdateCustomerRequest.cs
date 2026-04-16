namespace LoyaltySphere.RewardService.Api.Contracts.Customers;

/// <summary>
/// Request to update customer information.
/// </summary>
public record UpdateCustomerRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
}
