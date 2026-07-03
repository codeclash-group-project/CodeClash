using CodeClash.Domain.Enums;

namespace CodeClash.Application.Features.Matchmaking.DTOs;

/// <summary>Request body for POST /api/v1/matches/{id}/submit.</summary>
public sealed record SubmitSolutionRequestDto(
    ProgrammingLanguage Language,
    string              SourceCode);
