namespace CodeClash.Application.Features.Matchmaking.DTOs;

/// <summary>Returned by GET /api/v1/matchmaking/status.</summary>
public sealed record QueueStatusDto(
    bool     IsInQueue,
    DateTime? QueuedAt,
    int       EloMin,
    int       EloMax,
    int       SecondsInQueue);
