namespace CodeClash.Application.Features.Leaderboard.DTOs;

/// <summary>Single leaderboard row returned by the global / top100 endpoints.</summary>
public sealed record LeaderboardEntryDto(
    int    Rank,
    Guid   UserId,
    string Username,
    string? ProfileImageUrl,
    int    Elo,
    int    Wins,
    int    Losses,
    double WinRate);
