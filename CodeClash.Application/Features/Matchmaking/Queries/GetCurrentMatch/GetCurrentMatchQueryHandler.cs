using CodeClash.Application.Common.Interfaces;
using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Matchmaking.DTOs;
using CodeClash.Application.Features.Matchmaking.Queries.GetMatchArena;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Queries.GetCurrentMatch;

public class GetCurrentMatchQueryHandler : IRequestHandler<GetCurrentMatchQuery, Result<MatchArenaDto?>>
{
    private readonly IMatchArenaRepository _arenaRepo;
    private readonly IMediator             _mediator;

    public GetCurrentMatchQueryHandler(
        IMatchArenaRepository arenaRepo,
        IMediator             mediator)
    {
        _arenaRepo = arenaRepo;
        _mediator  = mediator;
    }

    public async Task<Result<MatchArenaDto?>> Handle(
        GetCurrentMatchQuery request,
        CancellationToken    cancellationToken)
    {
        var arena = await _arenaRepo.GetActiveMatchForUserAsync(request.UserId, cancellationToken);
        if (arena is null)
            return Result<MatchArenaDto?>.Success(null, "No active match found.");

        var detail = await _mediator.Send(
            new GetMatchArenaQuery(arena.Id, request.UserId), cancellationToken);

        return Result<MatchArenaDto?>.Success(detail.Data, "Current match retrieved.");
    }
}
