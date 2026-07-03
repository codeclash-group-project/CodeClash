using CodeClash.Domain.Entities;

namespace CodeClash.Application.Common.Interfaces;

/// <summary>
/// Repository for the MatchmakingQueue aggregate.
/// </summary>
public interface IMatchmakingQueueRepository
{
    Task<MatchmakingQueue?> GetActiveEntryForUserAsync(Guid userId, CancellationToken ct = default);
    Task<List<MatchmakingQueue>> GetAllSearchingAsync(CancellationToken ct = default);
    Task AddAsync(MatchmakingQueue entry, CancellationToken ct = default);
    Task RemoveAsync(MatchmakingQueue entry, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
