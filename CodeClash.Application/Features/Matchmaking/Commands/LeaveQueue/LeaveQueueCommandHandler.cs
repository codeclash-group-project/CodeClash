using CodeClash.Application.Common.Interfaces;
using CodeClash.Application.Common.Models;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Commands.LeaveQueue;

public class LeaveQueueCommandHandler : IRequestHandler<LeaveQueueCommand, Result>
{
    private readonly IMatchmakingQueueRepository _queueRepo;

    public LeaveQueueCommandHandler(IMatchmakingQueueRepository queueRepo)
    {
        _queueRepo = queueRepo;
    }

    public async Task<Result> Handle(LeaveQueueCommand request, CancellationToken cancellationToken)
    {
        var entry = await _queueRepo.GetActiveEntryForUserAsync(request.UserId, cancellationToken);
        if (entry is null)
            return Result.Failure("You are not currently in the matchmaking queue.");

        await _queueRepo.RemoveAsync(entry, cancellationToken);
        await _queueRepo.SaveChangesAsync(cancellationToken);

        return Result.Success("You have left the matchmaking queue.");
    }
}
