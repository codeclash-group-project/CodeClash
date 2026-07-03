using CodeClash.Application.Common.Interfaces;
using CodeClash.Domain.Entities;
using CodeClash.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CodeClash.Infrastructure.Services;

/// <summary>
/// Sends real-time SignalR events to match participants via IHubContext&lt;MatchHub&gt;.
/// Users receive events via their personal group "user-{userId}".
/// Match events are broadcast to group "match-{matchId}".
/// </summary>
public class MatchHubService : IMatchHubService
{
    private readonly IHubContext<MatchHub> _hubContext;

    public MatchHubService(IHubContext<MatchHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyMatchFoundAsync(
        Guid              playerOneId,
        Guid              playerTwoId,
        MatchFoundPayload payload,
        CancellationToken ct = default)
    {
        await _hubContext.Clients
            .Group($"user-{playerOneId}")
            .SendAsync("MatchFound", payload, ct);

        await _hubContext.Clients
            .Group($"user-{playerTwoId}")
            .SendAsync("MatchFound", payload, ct);
    }

    public async Task NotifyCountdownStartedAsync(Guid matchId, CancellationToken ct = default)
        => await _hubContext.Clients
            .Group($"match-{matchId}")
            .SendAsync("CountdownStarted", new { MatchId = matchId }, ct);

    public async Task NotifyMatchStartedAsync(Guid matchId, CancellationToken ct = default)
        => await _hubContext.Clients
            .Group($"match-{matchId}")
            .SendAsync("MatchStarted", new { MatchId = matchId }, ct);

    public async Task NotifyOpponentSubmittedAsync(
        Guid matchId, Guid opponentId, CancellationToken ct = default)
        => await _hubContext.Clients
            .Group($"match-{matchId}")
            .SendAsync("OpponentSubmitted", new { MatchId = matchId, OpponentId = opponentId }, ct);

    public async Task NotifyMatchCompletedAsync(
        Guid                 matchId,
        MatchCompletedPayload payload,
        CancellationToken    ct = default)
        => await _hubContext.Clients
            .Group($"match-{matchId}")
            .SendAsync("MatchCompleted", new { MatchId = matchId, Result = payload }, ct);

    public async Task NotifyOpponentDisconnectedAsync(Guid matchId, CancellationToken ct = default)
        => await _hubContext.Clients
            .Group($"match-{matchId}")
            .SendAsync("OpponentDisconnected", new { MatchId = matchId }, ct);
}
