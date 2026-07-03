using CodeClash.Domain.Entities;
using CodeClash.Domain.Enums;

namespace CodeClash.Application.Common.Interfaces;

/// <summary>
/// Repository for MatchArena aggregate.
/// </summary>
public interface IMatchArenaRepository
{
    Task<MatchArena?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<MatchArena?> GetActiveMatchForUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(MatchArena arena, CancellationToken ct = default);
    Task<List<MatchSubmission>> GetSubmissionsForMatchAsync(Guid matchArenaId, CancellationToken ct = default);
    Task AddSubmissionAsync(MatchSubmission submission, CancellationToken ct = default);
    Task AddHistoryAsync(MatchHistory history, CancellationToken ct = default);
    Task<List<MatchHistory>> GetRecentHistoryAsync(Guid userId, int count = 10, CancellationToken ct = default);

    /// <summary>Returns problem IDs played by userId in the last 30 days.</summary>
    Task<List<Guid>> GetRecentlyPlayedProblemIdsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Leaderboard: all users sorted by ELO descending with win/loss stats.</summary>
    Task<List<LeaderboardRow>> GetLeaderboardAsync(int skip, int take, CancellationToken ct = default);
    Task<int> GetLeaderboardCountAsync(CancellationToken ct = default);
}

/// <summary>Raw row from the leaderboard query — mapped to DTO in the query handler.</summary>
public sealed record LeaderboardRow(
    Guid   UserId,
    string Username,
    string? ProfileImageUrl,
    int    Elo,
    int    Wins,
    int    Losses);
