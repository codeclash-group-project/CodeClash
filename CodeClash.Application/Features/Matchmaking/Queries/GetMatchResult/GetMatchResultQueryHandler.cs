using CodeClash.Application.Common.Interfaces;
using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Matchmaking.DTOs;
using CodeClash.Domain.Enums;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Queries.GetMatchResult;

public class GetMatchResultQueryHandler : IRequestHandler<GetMatchResultQuery, Result<MatchResultDto>>
{
    private readonly IMatchArenaRepository _arenaRepo;
    private readonly IApplicationDbContext _db;

    public GetMatchResultQueryHandler(
        IMatchArenaRepository arenaRepo,
        IApplicationDbContext db)
    {
        _arenaRepo = arenaRepo;
        _db        = db;
    }

    public async Task<Result<MatchResultDto>> Handle(
        GetMatchResultQuery request,
        CancellationToken   cancellationToken)
    {
        var arena = await _arenaRepo.GetByIdAsync(request.MatchId, cancellationToken);
        if (arena is null)
            return Result<MatchResultDto>.Failure("Match not found.");

        if (!arena.InvolvesUser(request.RequestingUserId))
            return Result<MatchResultDto>.Failure("You are not a participant in this match.");

        if (!arena.IsFinished)
            return Result<MatchResultDto>.Failure("Match is not yet completed.");

        bool isP1       = arena.PlayerOneId == request.RequestingUserId;
        bool isWinner   = arena.WinnerId == request.RequestingUserId;
        bool isDraw     = arena.WinnerId is null &&
                          arena.Status is MatchStatus.Completed;

        int myBefore    = isP1 ? arena.PlayerOneRatingBefore : arena.PlayerTwoRatingBefore;
        int myAfter     = isP1 ? (arena.PlayerOneRatingAfter ?? myBefore)
                               : (arena.PlayerTwoRatingAfter ?? myBefore);
        int oppBefore   = isP1 ? arena.PlayerTwoRatingBefore : arena.PlayerOneRatingBefore;
        int oppAfter    = isP1 ? (arena.PlayerTwoRatingAfter ?? oppBefore)
                               : (arena.PlayerOneRatingAfter ?? oppBefore);

        string winnerName = "Draw";
        if (arena.WinnerId.HasValue)
        {
            var winner = await _db.Users.FindAsync([arena.WinnerId.Value], cancellationToken);
            winnerName = winner?.Username ?? "Unknown";
        }

        var dto = new MatchResultDto(
            arena.WinnerId,
            winnerName,
            myBefore,
            myAfter,
            myAfter - myBefore,
            oppBefore,
            oppAfter,
            isWinner,
            isDraw,
            arena.EndedAt);

        return Result<MatchResultDto>.Success(dto, "Match result retrieved.");
    }
}
