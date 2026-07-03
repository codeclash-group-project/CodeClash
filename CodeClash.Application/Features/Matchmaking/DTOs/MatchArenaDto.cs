using CodeClash.Domain.Enums;

namespace CodeClash.Application.Features.Matchmaking.DTOs;

/// <summary>Full match arena details returned by GET /api/v1/matches/{id} and /current.</summary>
public sealed record MatchArenaDto(
    Guid         Id,
    MatchStatus  Status,
    OpponentDto  Opponent,
    ProblemBriefDto Problem,
    int          DurationMinutes,
    DateTime?    StartedAt,
    int          TimeRemainingSeconds,
    int          MyRatingBefore,
    int          OpponentRatingBefore);

public sealed record OpponentDto(
    Guid   Id,
    string Username,
    string? ProfileImageUrl,
    int    Elo);

public sealed record ProblemBriefDto(
    Guid   Id,
    string Title,
    string Slug,
    string Difficulty,
    string Category,
    string StatementMarkdown);
