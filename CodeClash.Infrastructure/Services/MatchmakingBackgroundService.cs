using CodeClash.Application.Common.Interfaces;
using CodeClash.Domain.Entities;
using CodeClash.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodeClash.Infrastructure.Services;

/// <summary>
/// Background service that runs every 5 seconds to:
///  1. Scan the MatchmakingQueue for active searchers
///  2. Try to pair compatible players
///  3. Select a problem for the match
///  4. Create MatchArena and notify both players via SignalR
///  5. Run a 10-second countdown before starting the match
/// </summary>
public class MatchmakingBackgroundService : BackgroundService
{
    private static readonly TimeSpan ScanInterval     = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan CountdownSeconds = TimeSpan.FromSeconds(10);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MatchmakingBackgroundService> _logger;

    public MatchmakingBackgroundService(
        IServiceProvider                      serviceProvider,
        ILogger<MatchmakingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger          = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MatchmakingBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanAndMatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in matchmaking scan loop.");
            }

            await Task.Delay(ScanInterval, stoppingToken);
        }

        _logger.LogInformation("MatchmakingBackgroundService stopped.");
    }

    private async Task ScanAndMatchAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var queueRepo      = scope.ServiceProvider.GetRequiredService<IMatchmakingQueueRepository>();
        var arenaRepo      = scope.ServiceProvider.GetRequiredService<IMatchArenaRepository>();
        var problemSelector = scope.ServiceProvider.GetRequiredService<IProblemSelectorService>();
        var hubService     = scope.ServiceProvider.GetRequiredService<IMatchHubService>();
        var db             = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var searching = await queueRepo.GetAllSearchingAsync(ct);
        var matched   = new HashSet<Guid>(); // prevent double-matching within one scan

        for (int i = 0; i < searching.Count; i++)
        {
            var entry = searching[i];
            if (matched.Contains(entry.UserId)) continue;

            // Find a compatible opponent
            MatchmakingQueue? opponent = null;
            for (int j = i + 1; j < searching.Count; j++)
            {
                var candidate = searching[j];
                if (matched.Contains(candidate.UserId)) continue;

                if (entry.IsCompatibleWith(candidate))
                {
                    opponent = candidate;
                    break;
                }
            }

            if (opponent is null) continue;

            matched.Add(entry.UserId);
            matched.Add(opponent.UserId);

            // Mark both as matched
            entry.MarkMatched();
            opponent.MarkMatched();

            // Gather recently played problems for both players
            var recentP1 = await arenaRepo.GetRecentlyPlayedProblemIdsAsync(entry.UserId, ct);
            var recentP2 = await arenaRepo.GetRecentlyPlayedProblemIdsAsync(opponent.UserId, ct);
            var recentIds = recentP1.Union(recentP2).Distinct();

            var problem = await problemSelector.SelectProblemAsync(
                entry.Difficulty, entry.Category, recentIds, ct);

            if (problem is null)
            {
                _logger.LogWarning(
                    "No eligible problem for Difficulty={D} Category={C}. Skipping match.",
                    entry.Difficulty, entry.Category);
                continue;
            }

            // Create MatchArena
            var arena = MatchArena.Create(
                entry.UserId,
                opponent.UserId,
                problem.Id,
                entry.DurationMinutes,
                entry.Elo,
                opponent.Elo);

            arena.StartCountdown();

            await arenaRepo.AddAsync(arena, ct);
            await db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Match {MatchId} created: {P1} vs {P2} on problem '{Problem}'.",
                arena.Id, entry.UserId, opponent.UserId, problem.Title);

            // Notify both players match is found
            var p1User = await db.Users.FindAsync([entry.UserId], ct);
            var p2User = await db.Users.FindAsync([opponent.UserId], ct);

            var payload = new MatchFoundPayload(
                arena.Id,
                p2User?.Username        ?? "Opponent",
                p2User?.ProfileImageUrl ?? string.Empty,
                opponent.Elo,
                problem.Title,
                problem.Slug,
                entry.DurationMinutes);

            // Notify both players asynchronously (fire-and-continue)
            _ = Task.Run(async () =>
            {
                try
                {
                    await hubService.NotifyMatchFoundAsync(entry.UserId, opponent.UserId, payload, ct);
                    await Task.Delay(CountdownSeconds, ct);

                    // Transition to InProgress
                    using var innerScope = _serviceProvider.CreateScope();
                    var innerArenaRepo   = innerScope.ServiceProvider.GetRequiredService<IMatchArenaRepository>();
                    var innerDb          = innerScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                    var innerHub         = innerScope.ServiceProvider.GetRequiredService<IMatchHubService>();

                    var liveArena = await innerArenaRepo.GetByIdAsync(arena.Id, ct);
                    if (liveArena is { Status: MatchStatus.Countdown })
                    {
                        liveArena.StartMatch();
                        await innerDb.SaveChangesAsync(ct);
                        await innerHub.NotifyMatchStartedAsync(arena.Id, ct);
                        _logger.LogInformation("Match {MatchId} started.", arena.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during match startup for {MatchId}.", arena.Id);
                }
            }, ct);
        }

        // Persist queue changes (MarkMatched)
        await db.SaveChangesAsync(ct);
    }
}
