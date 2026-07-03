using CodeClash.Application.Common.Interfaces;
using CodeClash.Domain.Entities;
using CodeClash.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CodeClash.Infrastructure.Services;

/// <summary>
/// Selects a random active problem matching the requested difficulty and category,
/// excluding problems played by any player in the last 30 days.
/// </summary>
public class ProblemSelectorService : IProblemSelectorService
{
    private readonly IApplicationDbContext _db;

    public ProblemSelectorService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Problem?> SelectProblemAsync(
        Difficulty        difficulty,
        ProblemCategory   category,
        IEnumerable<Guid> recentlyPlayedProblemIds,
        CancellationToken ct = default)
    {
        var excludedIds = recentlyPlayedProblemIds.ToHashSet();

        // EF Core global query filter on Problem already excludes soft-deleted records.
        var candidates = await _db.Problems
            .Where(p => p.IsActive
                     && p.Difficulty == difficulty
                     && p.Category   == category
                     && !excludedIds.Contains(p.Id))
            .ToListAsync(ct);

        if (candidates.Count == 0)
        {
            // Fallback: ignore recently-played filter if no fresh problems exist
            candidates = await _db.Problems
                .Where(p => p.IsActive
                         && p.Difficulty == difficulty
                         && p.Category   == category)
                .ToListAsync(ct);
        }

        if (candidates.Count == 0) return null;

        // Uniform random selection
        int index = Random.Shared.Next(candidates.Count);
        return candidates[index];
    }
}
