using CodeClash.Domain.Enums;

namespace CodeClash.Application.Features.Matchmaking.DTOs;

/// <summary>Request body for POST /api/v1/matchmaking/join.</summary>
public sealed record JoinQueueRequestDto(
    Difficulty          Difficulty,
    ProblemCategory     Category,
    ProgrammingLanguage PreferredLanguage,
    int                 DurationMinutes);
