using CodeClash.Application.Common.Interfaces;
using CodeClash.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeClash.Infrastructure.Persistence.Repositories;

public class MatchmakingQueueRepository : IMatchmakingQueueRepository
{
    private readonly ApplicationDbContext _db;

    public MatchmakingQueueRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<MatchmakingQueue?> GetActiveEntryForUserAsync(
        Guid userId, CancellationToken ct = default)
        => await _db.MatchmakingQueues
            .FirstOrDefaultAsync(q => q.UserId == userId && q.IsSearching, ct);

    public async Task<List<MatchmakingQueue>> GetAllSearchingAsync(CancellationToken ct = default)
        => await _db.MatchmakingQueues
            .Include(q => q.User)
            .Where(q => q.IsSearching)
            .OrderBy(q => q.QueuedAt)          // FIFO — oldest first
            .ToListAsync(ct);

    public async Task AddAsync(MatchmakingQueue entry, CancellationToken ct = default)
        => await _db.MatchmakingQueues.AddAsync(entry, ct);

    public Task RemoveAsync(MatchmakingQueue entry, CancellationToken ct = default)
    {
        _db.MatchmakingQueues.Remove(entry);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
