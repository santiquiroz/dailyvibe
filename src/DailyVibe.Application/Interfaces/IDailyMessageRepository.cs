using DailyVibe.Application.Common;
using DailyVibe.Domain.Entities;

namespace DailyVibe.Application.Interfaces;

public interface IDailyMessageRepository
{
    Task AddAsync(DailyMessage message, CancellationToken cancellationToken);
    Task<DailyMessage?> FindLatestCreatedBetweenAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);
    Task<PagedResult<DailyMessage>> GetPageAsync(Guid userId, int page, int size, CancellationToken cancellationToken);
}
