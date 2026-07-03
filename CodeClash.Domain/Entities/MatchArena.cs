using CodeClash.Domain.Enums;

namespace CodeClash.Domain.Entities;

/// <summary>
/// MatchArena aggregate root.
/// Represents a live 1v1 coding battle between two players.
/// All state transitions are enforced through domain methods.
/// </summary>
public class MatchArena
{
    public Guid         Id                    { get; private set; }
    public Guid         PlayerOneId           { get; private set; }
    public Guid         PlayerTwoId           { get; private set; }
    public Guid         ProblemId             { get; private set; }
    public int          PlayerOneRatingBefore { get; private set; }
    public int          PlayerTwoRatingBefore { get; private set; }
    public int?         PlayerOneRatingAfter  { get; private set; }
    public int?         PlayerTwoRatingAfter  { get; private set; }
    public MatchStatus  Status                { get; private set; }
    public DateTime     CreatedAt             { get; private set; }
    public DateTime?    StartedAt             { get; private set; }
    public DateTime?    EndedAt               { get; private set; }
    public Guid?        WinnerId              { get; private set; }
    public int          DurationMinutes       { get; private set; }

    // Navigation
    public User?           PlayerOne    { get; private set; }
    public User?           PlayerTwo    { get; private set; }
    public Problem?        Problem      { get; private set; }

    // EF constructor
    private MatchArena() { }

    // ── Factory ──────────────────────────────────────────────────────────────

    public static MatchArena Create(
        Guid playerOneId,
        Guid playerTwoId,
        Guid problemId,
        int  durationMinutes,
        int  playerOneRating,
        int  playerTwoRating)
    {
        return new MatchArena
        {
            Id                    = Guid.NewGuid(),
            PlayerOneId           = playerOneId,
            PlayerTwoId           = playerTwoId,
            ProblemId             = problemId,
            DurationMinutes       = durationMinutes,
            PlayerOneRatingBefore = playerOneRating,
            PlayerTwoRatingBefore = playerTwoRating,
            Status                = MatchStatus.WaitingForOpponent,
            CreatedAt             = DateTime.UtcNow
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    /// <summary>Transition to countdown phase (both players notified, timer begins).</summary>
    public void StartCountdown()
    {
        if (Status != MatchStatus.WaitingForOpponent)
            throw new InvalidOperationException($"Cannot start countdown from status {Status}.");

        Status = MatchStatus.Countdown;
    }

    /// <summary>Transition to active battle.</summary>
    public void StartMatch()
    {
        if (Status != MatchStatus.Countdown)
            throw new InvalidOperationException($"Cannot start match from status {Status}.");

        Status    = MatchStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Complete the match. Pass null winnerId for a draw.
    /// Updated ratings are provided by the EloRatingService.
    /// </summary>
    public void Complete(
        Guid? winnerId,
        int   playerOneRatingAfter,
        int   playerTwoRatingAfter)
    {
        if (Status != MatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete match from status {Status}.");

        Status               = MatchStatus.Completed;
        WinnerId             = winnerId;
        PlayerOneRatingAfter = playerOneRatingAfter;
        PlayerTwoRatingAfter = playerTwoRatingAfter;
        EndedAt              = DateTime.UtcNow;
    }

    /// <summary>Cancel the match before it begins (e.g. opponent disconnects pre-start).</summary>
    public void Cancel()
    {
        if (Status is MatchStatus.Completed or MatchStatus.Disconnected)
            throw new InvalidOperationException($"Cannot cancel a match in status {Status}.");

        Status  = MatchStatus.Cancelled;
        EndedAt = DateTime.UtcNow;
    }

    /// <summary>Mark as disconnected mid-match. The non-disconnected player wins.</summary>
    public void Disconnect(Guid disconnectedUserId, int winnerRatingAfter, int loserRatingAfter)
    {
        if (Status != MatchStatus.InProgress)
            throw new InvalidOperationException($"Cannot disconnect match from status {Status}.");

        var winnerId = disconnectedUserId == PlayerOneId ? PlayerTwoId : PlayerOneId;

        Status               = MatchStatus.Disconnected;
        WinnerId             = winnerId;
        PlayerOneRatingAfter = disconnectedUserId == PlayerOneId ? loserRatingAfter  : winnerRatingAfter;
        PlayerTwoRatingAfter = disconnectedUserId == PlayerTwoId ? loserRatingAfter  : winnerRatingAfter;
        EndedAt              = DateTime.UtcNow;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    public bool IsActive      => Status is MatchStatus.InProgress or MatchStatus.Countdown;
    public bool IsFinished    => Status is MatchStatus.Completed or MatchStatus.Cancelled or MatchStatus.Disconnected;
    public bool InvolvesUser(Guid userId) => PlayerOneId == userId || PlayerTwoId == userId;
    public Guid GetOpponentId(Guid userId) => PlayerOneId == userId ? PlayerTwoId : PlayerOneId;
}
