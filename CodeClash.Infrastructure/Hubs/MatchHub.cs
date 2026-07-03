using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CodeClash.Infrastructure.Hubs;

/// <summary>
/// Real-time hub for matchmaking and live coding battle events.
///
/// Groups:
///  "user-{userId}"    → personal notifications (MatchFound)
///  "match-{matchId}"  → in-match events (CountdownStarted, MatchStarted, etc.)
///
/// Client Events (server → client):
///  MatchFound              { matchId, opponentUsername, opponentElo, problemTitle, durationMinutes }
///  CountdownStarted        { matchId }
///  MatchStarted            { matchId }
///  OpponentSubmitted       { matchId, opponentId }
///  MatchCompleted          { matchId, result }
///  OpponentDisconnected    { matchId }
///
/// Client Methods (client → server):
///  JoinMatchGroup(matchId)
/// </summary>
[Authorize]
public class MatchHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        await base.OnConnectedAsync();
    }

    /// <summary>Client calls this once a match is found to receive in-match events.</summary>
    public async Task JoinMatchGroup(string matchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"match-{matchId}");
    }

    /// <summary>Client calls this when leaving the match page.</summary>
    public async Task LeaveMatchGroup(string matchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"match-{matchId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // The MatchmakingBackgroundService handles disconnect logic
        // by watching for timed-out InProgress matches.
        await base.OnDisconnectedAsync(exception);
    }
}
