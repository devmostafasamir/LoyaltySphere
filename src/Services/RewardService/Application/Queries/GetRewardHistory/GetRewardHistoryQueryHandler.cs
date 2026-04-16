using LoyaltySphere.RewardService.Application.Mappers;
using LoyaltySphere.RewardService.Domain.Repositories;
using MediatR;

namespace LoyaltySphere.RewardService.Application.Queries.GetRewardHistory;

/// <summary>
/// Handles the GetRewardHistoryQuery.
/// Retrieves paginated reward transaction history for a customer.
/// </summary>
public class GetRewardHistoryQueryHandler : IRequestHandler<GetRewardHistoryQuery, RewardHistoryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRewardHistoryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RewardHistoryResponse> Handle(
        GetRewardHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Get rewards with filters
        var rewards = await _unitOfWork.Rewards
            .GetByCustomerIdAsync(
                request.CustomerId,
                request.RewardType,
                request.FromDate,
                request.ToDate,
                cancellationToken);

        // Apply pagination
        var totalCount = rewards.Count;
        var transactions = rewards
            .OrderByDescending(r => r.ProcessedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(RewardMapper.ToTransactionDto)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new RewardHistoryResponse
        {
            CustomerId = request.CustomerId,
            Transactions = transactions,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }
}
