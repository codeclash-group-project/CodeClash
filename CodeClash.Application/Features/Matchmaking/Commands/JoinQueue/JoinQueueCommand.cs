using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Matchmaking.DTOs;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Commands.JoinQueue;

public record JoinQueueCommand(
    Guid                 UserId,
    int                  Elo,
    JoinQueueRequestDto  Dto
) : IRequest<Result<Guid>>;
