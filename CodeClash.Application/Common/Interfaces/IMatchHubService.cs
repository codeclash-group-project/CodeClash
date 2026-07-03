namespace CodeClash.Application.Common.Interfaces;

/// <summary>
/// Sends real-time SignalR events to match participants.
/// Implemented in Infrastructure using IHubContext&lt;MatchHub&gt;.
/// </summary>
public interface IMatchHubService
{
    Task NotifyMatchFoundAsync(Guid playerOneId, Guid playerTwoId, MatchFoundPayload payload, CancellationToken ct = default);
    Task NotifyCountdownStartedAsync(Guid matchId, CancellationToken ct = default);
    Task NotifyMatchStartedAsync(Guid matchId, CancellationToken ct = default);
    Task NotifyOpponentSubmittedAsync(Guid matchId, Guid opponentId, CancellationToken ct = default);
    Task NotifyMatchCompletedAsync(Guid matchId, MatchCompletedPayload payload, CancellationToken ct = default);
    Task NotifyOpponentDisconnectedAsync(Guid matchId, CancellationToken ct = default);
}

/// <summary>Payload sent when a match is found.</summary>
public sealed record MatchFoundPayload(
    Guid   MatchId,
    string OpponentUsername,
    string OpponentProfileImageUrl,
    int    OpponentElo,
    string ProblemTitle,
    string ProblemSlug,
    int    DurationMinutes);

/// <summary>Payload sent when a match completes.</summary>
public sealed record MatchCompletedPayload(
    Guid?  WinnerId,
    string WinnerUsername,
    int    PlayerOneRatingAfter,
    int    PlayerTwoRatingAfter);
