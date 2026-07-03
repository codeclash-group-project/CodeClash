using CodeClash.Application.Common.Interfaces;
using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Leaderboard.DTOs;
using MediatR;

namespace CodeClash.Application.Features.Leaderboard.Queries.GetGlobalLeaderboard;

public class GetGlobalLeaderboardQueryHandler
    : IRequestHandler<GetGlobalLeaderboardQuery, Result<PaginatedList<LeaderboardEntryDto>>>
{
    private readonly IMatchArenaRepository _arenaRepo;

    public GetGlobalLeaderboardQueryHandler(IMatchArenaRepository arenaRepo)
    {
        _arenaRepo = arenaRepo;
    }

    public async Task<Result<PaginatedList<LeaderboardEntryDto>>> Handle(
        GetGlobalLeaderboardQuery request,
        CancellationToken         cancellationToken)
    {
        int skip  = (request.Page - 1) * request.PageSize;
        int count = await _arenaRepo.GetLeaderboardCountAsync(cancellationToken);
        var rows  = await _arenaRepo.GetLeaderboardAsync(skip, request.PageSize, cancellationToken);

        int rank = skip + 1;
        var items = rows.Select(r =>
        {
            int total   = r.Wins + r.Losses;
            double rate = total == 0 ? 0 : Math.Round((double)r.Wins / total * 100, 1);
            return new LeaderboardEntryDto(rank++, r.UserId, r.Username, r.ProfileImageUrl, r.Elo, r.Wins, r.Losses, rate);
        }).ToList();

        var paged = new PaginatedList<LeaderboardEntryDto>(items, count, request.Page, request.PageSize);
        return Result<PaginatedList<LeaderboardEntryDto>>.Success(paged, "Leaderboard retrieved.");
    }
}
