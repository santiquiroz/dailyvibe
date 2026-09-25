using DailyVibe.Application.Common;
using DailyVibe.Application.Interfaces;
using DailyVibe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyVibe.Infrastructure.Persistence.Repositories;

public sealed class DailyMessageRepository(AppDbContext db) : IDailyMessageRepository
{
    public async Task AddAsync(DailyMessage message, CancellationToken cancellationToken)
    {
        db.DailyMessages.Add(message);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<DailyMessage?> FindLatestCreatedBetweenAsync(
        Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken) =>
        MessagesOf(userId)
            .Where(m => m.CreatedAt >= fromUtc && m.CreatedAt < toUtc)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<DailyMessage>> GetPageAsync(
        Guid userId, int page, int size, CancellationToken cancellationToken)
    {
        var totalCount = await MessagesOf(userId).CountAsync(cancellationToken);
        var items = await MessagesOf(userId)
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PagedResult<DailyMessage>(items, page, size, totalCount);
    }

    private IQueryable<DailyMessage> MessagesOf(Guid userId) =>
        db.DailyMessages.AsNoTracking().Where(m => m.UserId == userId);
}
