using CodeClash.Application.Common.Interfaces;
using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Matchmaking.DTOs;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Queries.GetQueueStatus;

public class GetQueueStatusQueryHandler : IRequestHandler<GetQueueStatusQuery, Result<QueueStatusDto>>
{
    private readonly IMatchmakingQueueRepository _queueRepo;

    public GetQueueStatusQueryHandler(IMatchmakingQueueRepository queueRepo)
    {
        _queueRepo = queueRepo;
    }

    public async Task<Result<QueueStatusDto>> Handle(
        GetQueueStatusQuery request,
        CancellationToken   cancellationToken)
    {
        var entry = await _queueRepo.GetActiveEntryForUserAsync(request.UserId, cancellationToken);

        if (entry is null)
        {
            return Result<QueueStatusDto>.Success(
                new QueueStatusDto(false, null, 0, 0, 0),
                "Not in queue.");
        }

        var (min, max) = entry.GetEloSearchRange();
        var seconds    = (int)(DateTime.UtcNow - entry.QueuedAt).TotalSeconds;

        return Result<QueueStatusDto>.Success(
            new QueueStatusDto(true, entry.QueuedAt, min, max, seconds),
            "Queue status retrieved.");
    }
}
