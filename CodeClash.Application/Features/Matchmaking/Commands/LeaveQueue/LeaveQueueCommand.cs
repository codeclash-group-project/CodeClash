using CodeClash.Application.Common.Models;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Commands.LeaveQueue;

public record LeaveQueueCommand(Guid UserId) : IRequest<Result>;
