using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Matchmaking.DTOs;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Queries.GetMatchArena;

public record GetMatchArenaQuery(Guid MatchId, Guid RequestingUserId) : IRequest<Result<MatchArenaDto>>;
