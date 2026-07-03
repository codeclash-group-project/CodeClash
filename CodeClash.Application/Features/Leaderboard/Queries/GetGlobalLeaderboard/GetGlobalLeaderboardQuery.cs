using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Leaderboard.DTOs;
using MediatR;

namespace CodeClash.Application.Features.Leaderboard.Queries.GetGlobalLeaderboard;

public record GetGlobalLeaderboardQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PaginatedList<LeaderboardEntryDto>>>;
