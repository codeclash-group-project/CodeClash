using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Matchmaking.DTOs;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Queries.GetCurrentMatch;

public record GetCurrentMatchQuery(Guid UserId) : IRequest<Result<MatchArenaDto?>>;
