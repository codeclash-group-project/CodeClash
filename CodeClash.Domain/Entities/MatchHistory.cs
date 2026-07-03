using CodeClash.Domain.Enums;

namespace CodeClash.Domain.Entities;

/// <summary>
/// Immutable record of a match outcome from a single player's perspective.
/// Two MatchHistory rows are created per completed match (one per player).
/// </summary>
public class MatchHistory
{
    public Guid        Id           { get; private set; }
    public Guid        MatchId      { get; private set; }
    public Guid        UserId       { get; private set; }
    public Guid        OpponentId   { get; private set; }
    public Guid        ProblemId    { get; private set; }
    public MatchResult Result       { get; private set; }
    public int         RatingBefore { get; private set; }
    public int         RatingAfter  { get; private set; }
    public DateTime    PlayedAt     { get; private set; }

    // Navigation
    public MatchArena? Match    { get; private set; }
    public User?       User     { get; private set; }
    public User?       Opponent { get; private set; }
    public Problem?    Problem  { get; private set; }

    // EF constructor
    private MatchHistory() { }

    // ── Factory ──────────────────────────────────────────────────────────────

    public static MatchHistory Create(
        Guid        matchId,
        Guid        userId,
        Guid        opponentId,
        Guid        problemId,
        MatchResult result,
        int         ratingBefore,
        int         ratingAfter)
    {
        return new MatchHistory
        {
            Id           = Guid.NewGuid(),
            MatchId      = matchId,
            UserId       = userId,
            OpponentId   = opponentId,
            ProblemId    = problemId,
            Result       = result,
            RatingBefore = ratingBefore,
            RatingAfter  = ratingAfter,
            PlayedAt     = DateTime.UtcNow
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    public int RatingChange => RatingAfter - RatingBefore;
    public bool Won  => Result == MatchResult.Win;
    public bool Lost => Result == MatchResult.Loss;
}
