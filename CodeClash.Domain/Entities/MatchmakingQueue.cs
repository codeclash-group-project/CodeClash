using CodeClash.Domain.Enums;

namespace CodeClash.Domain.Entities;

/// <summary>
/// Represents a player currently waiting in the ranked matchmaking queue.
/// IsSearching = false when the player has been matched and is no longer available.
/// </summary>
public class MatchmakingQueue
{
    public Guid                Id                { get; private set; }
    public Guid                UserId            { get; private set; }
    public int                 Elo               { get; private set; }
    public Difficulty          Difficulty        { get; private set; }
    public ProblemCategory     Category          { get; private set; }
    public ProgrammingLanguage PreferredLanguage { get; private set; }
    public int                 DurationMinutes   { get; private set; }
    public DateTime            QueuedAt          { get; private set; }
    public bool                IsSearching       { get; private set; }

    // Navigation
    public User? User { get; private set; }

    // EF constructor
    private MatchmakingQueue() { }

    // ── Factory ──────────────────────────────────────────────────────────────

    public static MatchmakingQueue Create(
        Guid               userId,
        int                elo,
        Difficulty         difficulty,
        ProblemCategory    category,
        ProgrammingLanguage preferredLanguage,
        int                durationMinutes)
    {
        return new MatchmakingQueue
        {
            Id                = Guid.NewGuid(),
            UserId            = userId,
            Elo               = elo,
            Difficulty        = difficulty,
            Category          = category,
            PreferredLanguage = preferredLanguage,
            DurationMinutes   = durationMinutes,
            QueuedAt          = DateTime.UtcNow,
            IsSearching       = true
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    /// <summary>Mark as matched — removes from active queue search.</summary>
    public void MarkMatched()
    {
        IsSearching = false;
    }

    /// <summary>Compute the current ELO search range based on elapsed time in queue.</summary>
    public (int Min, int Max) GetEloSearchRange()
    {
        var secondsInQueue = (DateTime.UtcNow - QueuedAt).TotalSeconds;
        var expansions     = (int)(secondsInQueue / 15);   // expand every 15 seconds
        var range          = 100 + expansions * 50;         // base ±100, +50 per expansion

        return (Elo - range, Elo + range);
    }

    /// <summary>Returns true if the opponent queue entry is compatible for a match.</summary>
    public bool IsCompatibleWith(MatchmakingQueue opponent)
    {
        if (opponent.UserId == UserId) return false;
        if (!opponent.IsSearching)     return false;
        if (opponent.Difficulty != Difficulty) return false;

        var (min, max) = GetEloSearchRange();
        if (opponent.Elo < min || opponent.Elo > max) return false;

        // Category must be an exact match
        if (opponent.Category != Category) return false;

        return true;
    }
}
