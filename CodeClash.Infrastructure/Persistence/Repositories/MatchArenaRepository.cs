using CodeClash.Application.Common.Interfaces;
using CodeClash.Domain.Entities;
using CodeClash.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CodeClash.Infrastructure.Persistence.Repositories;

public class MatchArenaRepository : IMatchArenaRepository
{
    private readonly ApplicationDbContext _db;

    public MatchArenaRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<MatchArena?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.MatchArenas
            .Include(m => m.PlayerOne)
            .Include(m => m.PlayerTwo)
            .Include(m => m.Problem)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<MatchArena?> GetActiveMatchForUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.MatchArenas
            .FirstOrDefaultAsync(m =>
                (m.PlayerOneId == userId || m.PlayerTwoId == userId) &&
                (m.Status == MatchStatus.InProgress ||
                 m.Status == MatchStatus.Countdown  ||
                 m.Status == MatchStatus.WaitingForOpponent), ct);

    public async Task AddAsync(MatchArena arena, CancellationToken ct = default)
        => await _db.MatchArenas.AddAsync(arena, ct);

    public async Task<List<MatchSubmission>> GetSubmissionsForMatchAsync(
        Guid matchArenaId, CancellationToken ct = default)
        => await _db.MatchSubmissions
            .Where(s => s.MatchArenaId == matchArenaId)
            .OrderBy(s => s.SubmittedAt)
            .ToListAsync(ct);

    public async Task AddSubmissionAsync(MatchSubmission submission, CancellationToken ct = default)
        => await _db.MatchSubmissions.AddAsync(submission, ct);

    public async Task AddHistoryAsync(MatchHistory history, CancellationToken ct = default)
        => await _db.MatchHistories.AddAsync(history, ct);

    public async Task<List<MatchHistory>> GetRecentHistoryAsync(
        Guid userId, int count = 10, CancellationToken ct = default)
        => await _db.MatchHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.PlayedAt)
            .Take(count)
            .ToListAsync(ct);

    public async Task<List<Guid>> GetRecentlyPlayedProblemIdsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        return await _db.MatchHistories
            .Where(h => h.UserId == userId && h.PlayedAt >= cutoff)
            .Select(h => h.ProblemId)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<List<LeaderboardRow>> GetLeaderboardAsync(
        int skip, int take, CancellationToken ct = default)
    {
        // Aggregate win/loss from MatchHistories, then join with Users for ELO.
        // ELO is derived from the most recent RatingAfter value per user.
        var latestRating = await _db.MatchHistories
            .GroupBy(h => h.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Elo    = g.OrderByDescending(h => h.PlayedAt).First().RatingAfter
            })
            .ToDictionaryAsync(x => x.UserId, x => x.Elo, ct);

        var stats = await _db.MatchHistories
            .GroupBy(h => h.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Wins   = g.Count(h => h.Result == MatchResult.Win),
                Losses = g.Count(h => h.Result == MatchResult.Loss)
            })
            .ToListAsync(ct);

        var userIds = stats.Select(s => s.UserId).ToList();
        var users   = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Username, u.ProfileImageUrl })
            .ToDictionaryAsync(u => u.Id, ct);

        return stats
            .Select(s => new LeaderboardRow(
                s.UserId,
                users.TryGetValue(s.UserId, out var u) ? u.Username   : "Unknown",
                users.TryGetValue(s.UserId, out var ui) ? ui.ProfileImageUrl : null,
                latestRating.TryGetValue(s.UserId, out var elo) ? elo : 1000,
                s.Wins,
                s.Losses))
            .OrderByDescending(r => r.Elo)
            .ThenByDescending(r => r.Wins)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    public async Task<int> GetLeaderboardCountAsync(CancellationToken ct = default)
        => await _db.MatchHistories
            .Select(h => h.UserId)
            .Distinct()
            .CountAsync(ct);
}
