using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Leaderboard.DTOs;
using MediatR;

namespace CodeClash.Application.Features.Leaderboard.Queries.GetTop100Leaderboard;

public record GetTop100LeaderboardQuery : IRequest<Result<List<LeaderboardEntryDto>>>;
