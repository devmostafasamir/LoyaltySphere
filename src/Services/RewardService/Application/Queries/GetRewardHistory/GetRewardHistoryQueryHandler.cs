using LoyaltySphere.RewardService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LoyaltySphere.RewardService.Application.Queries.GetRewardHistory;

/// <summary>
/// Handles the GetRewardHistoryQuery.
/// Retrieves paginated reward transaction history for a customer.
/// </summary>
public class GetRewardHistoryQueryHandler : IRequestHandler<GetRewardHistoryQuery, RewardHistoryResponse>
{
    private readonly ApplicationDbContext _context;

    public GetRewardHistoryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RewardHistoryResponse> Handle(
        GetRewardHistoryQuery request,
        CancellationToken cancellationToken)
    {
        // Build query
        var query = _context.Rewards
            .AsNoTracking()
            .Where(r => r.TenantId == request.TenantId 
                && r.CustomerExternalId == request.CustomerId
                && r.IsProcessed);

        // Apply filters
        if (!string.IsNullOrEmpty(request.RewardType))
        {
            query = query.Where(r => r.RewardType == request.RewardType);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(r => r.ProcessedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(r => r.ProcessedAt <= request.ToDate.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var transactions = await query
            .OrderByDescending(r => r.ProcessedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new RewardTransactionDto
            {
                Id = r.Id,
                Points = r.PointsAwarded.Value,
                TransactionAmount = r.TransactionAmount.Amount,
                RewardType = r.RewardType,
                Source = r.Source,
                Description = r.Description,
                TransactionId = r.TransactionId,
                CampaignId = r.CampaignId,
                ProcessedAt = r.ProcessedAt
            })
            .ToListAsync(cancellationToken);

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
