using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Matchmaking.DTOs;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Queries.GetMatchResult;

public record GetMatchResultQuery(Guid MatchId, Guid RequestingUserId) : IRequest<Result<MatchResultDto>>;
