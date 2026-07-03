using CodeClash.Application.Common.Interfaces;
using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Matchmaking.DTOs;
using CodeClash.Domain.Enums;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Queries.GetMatchArena;

public class GetMatchArenaQueryHandler : IRequestHandler<GetMatchArenaQuery, Result<MatchArenaDto>>
{
    private readonly IMatchArenaRepository _arenaRepo;
    private readonly IApplicationDbContext _db;

    public GetMatchArenaQueryHandler(
        IMatchArenaRepository arenaRepo,
        IApplicationDbContext db)
    {
        _arenaRepo = arenaRepo;
        _db        = db;
    }

    public async Task<Result<MatchArenaDto>> Handle(
        GetMatchArenaQuery request,
        CancellationToken  cancellationToken)
    {
        var arena = await _arenaRepo.GetByIdAsync(request.MatchId, cancellationToken);
        if (arena is null)
            return Result<MatchArenaDto>.Failure("Match not found.");

        if (!arena.InvolvesUser(request.RequestingUserId))
            return Result<MatchArenaDto>.Failure("You are not a participant in this match.");

        var opponentId = arena.GetOpponentId(request.RequestingUserId);
        var opponent   = await _db.Users.FindAsync([opponentId], cancellationToken);
        var problem    = await _db.Problems.FindAsync([arena.ProblemId], cancellationToken);

        if (opponent is null || problem is null)
            return Result<MatchArenaDto>.Failure("Match data is incomplete.");

        bool isP1 = arena.PlayerOneId == request.RequestingUserId;

        int timeRemaining = 0;
        if (arena.StartedAt.HasValue && arena.Status == MatchStatus.InProgress)
        {
            var elapsed = (DateTime.UtcNow - arena.StartedAt.Value).TotalSeconds;
            timeRemaining = Math.Max(0, arena.DurationMinutes * 60 - (int)elapsed);
        }

        var dto = new MatchArenaDto(
            arena.Id,
            arena.Status,
            new OpponentDto(
                opponent.Id,
                opponent.Username,
                opponent.ProfileImageUrl,
                isP1 ? arena.PlayerTwoRatingBefore : arena.PlayerOneRatingBefore),
            new ProblemBriefDto(
                problem.Id,
                problem.Title,
                problem.Slug,
                problem.Difficulty.ToString(),
                problem.Category.ToString(),
                problem.StatementMarkdown),
            arena.DurationMinutes,
            arena.StartedAt,
            timeRemaining,
            isP1 ? arena.PlayerOneRatingBefore : arena.PlayerTwoRatingBefore,
            isP1 ? arena.PlayerTwoRatingBefore : arena.PlayerOneRatingBefore);

        return Result<MatchArenaDto>.Success(dto, "Match arena retrieved.");
    }
}
