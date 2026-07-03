using CodeClash.Domain.Entities;
using CodeClash.Domain.Enums;

namespace CodeClash.Application.Common.Interfaces;

/// <summary>
/// Selects a suitable problem for a match based on difficulty, category,
/// and the player's recently played problem list.
/// </summary>
public interface IProblemSelectorService
{
    /// <summary>
    /// Returns a randomly selected active, non-deleted problem that:
    ///  - matches the requested difficulty
    ///  - matches the requested category
    ///  - was not played by either player in the last 30 days
    /// Returns null if no eligible problem is found.
    /// </summary>
    Task<Problem?> SelectProblemAsync(
        Difficulty       difficulty,
        ProblemCategory  category,
        IEnumerable<Guid> recentlyPlayedProblemIds,
        CancellationToken ct = default);
}
