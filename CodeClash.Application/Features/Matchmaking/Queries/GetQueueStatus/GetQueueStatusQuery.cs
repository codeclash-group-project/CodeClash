using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Matchmaking.DTOs;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Queries.GetQueueStatus;

public record GetQueueStatusQuery(Guid UserId) : IRequest<Result<QueueStatusDto>>;
