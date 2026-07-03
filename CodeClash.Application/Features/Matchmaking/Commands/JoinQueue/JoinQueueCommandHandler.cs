using CodeClash.Application.Common.Interfaces;
using CodeClash.Application.Common.Models;
using CodeClash.Domain.Entities;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Commands.JoinQueue;

public class JoinQueueCommandHandler : IRequestHandler<JoinQueueCommand, Result<Guid>>
{
    private readonly IMatchmakingQueueRepository _queueRepo;

    public JoinQueueCommandHandler(IMatchmakingQueueRepository queueRepo)
    {
        _queueRepo = queueRepo;
    }

    public async Task<Result<Guid>> Handle(JoinQueueCommand request, CancellationToken cancellationToken)
    {
        // Prevent duplicate queue entries
        var existing = await _queueRepo.GetActiveEntryForUserAsync(request.UserId, cancellationToken);
        if (existing is not null)
            return Result<Guid>.Failure("You are already in the matchmaking queue.");

        var entry = MatchmakingQueue.Create(
            request.UserId,
            request.Elo,
            request.Dto.Difficulty,
            request.Dto.Category,
            request.Dto.PreferredLanguage,
            request.Dto.DurationMinutes);

        await _queueRepo.AddAsync(entry, cancellationToken);
        await _queueRepo.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(entry.Id, "You have joined the matchmaking queue.");
    }
}
