namespace CodeClash.Application.Features.Matchmaking.DTOs;

/// <summary>Result details returned by GET /api/v1/matches/{id}/result.</summary>
public sealed record MatchResultDto(
    Guid?  WinnerId,
    string WinnerUsername,
    int    MyRatingBefore,
    int    MyRatingAfter,
    int    MyRatingChange,
    int    OpponentRatingBefore,
    int    OpponentRatingAfter,
    bool   IsWinner,
    bool   IsDraw,
    DateTime? EndedAt);
