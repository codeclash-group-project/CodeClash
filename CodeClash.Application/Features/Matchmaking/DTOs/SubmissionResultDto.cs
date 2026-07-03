namespace CodeClash.Application.Features.Matchmaking.DTOs;

/// <summary>Returned by POST /api/v1/matches/{id}/submit.</summary>
public sealed record SubmissionResultDto(
    Guid   SubmissionId,
    bool   IsAccepted,
    int    TestCasesPassed,
    int    TotalTestCases,
    long   ExecutionTimeMs,
    long   MemoryUsedMb,
    string Message);
