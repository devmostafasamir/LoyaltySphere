namespace LoyaltySphere.RewardService.Api.Contracts.Customers;

/// <summary>
/// Request to enroll a new customer in the loyalty program.
/// </summary>
public record EnrollCustomerRequest
{
    public required string CustomerId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
}
