using CodeClash.Application.Common.Interfaces;
using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Leaderboard.DTOs;
using MediatR;

namespace CodeClash.Application.Features.Leaderboard.Queries.GetTop100Leaderboard;

public class GetTop100LeaderboardQueryHandler
    : IRequestHandler<GetTop100LeaderboardQuery, Result<List<LeaderboardEntryDto>>>
{
    private readonly IMatchArenaRepository _arenaRepo;

    public GetTop100LeaderboardQueryHandler(IMatchArenaRepository arenaRepo)
    {
        _arenaRepo = arenaRepo;
    }

    public async Task<Result<List<LeaderboardEntryDto>>> Handle(
        GetTop100LeaderboardQuery request,
        CancellationToken         cancellationToken)
    {
        var rows = await _arenaRepo.GetLeaderboardAsync(0, 100, cancellationToken);

        int rank  = 1;
        var items = rows.Select(r =>
        {
            int    total = r.Wins + r.Losses;
            double rate  = total == 0 ? 0 : Math.Round((double)r.Wins / total * 100, 1);
            return new LeaderboardEntryDto(rank++, r.UserId, r.Username, r.ProfileImageUrl, r.Elo, r.Wins, r.Losses, rate);
        }).ToList();

        return Result<List<LeaderboardEntryDto>>.Success(items, "Top 100 leaderboard retrieved.");
    }
}
