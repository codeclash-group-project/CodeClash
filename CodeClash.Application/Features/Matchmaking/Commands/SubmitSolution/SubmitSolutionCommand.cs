using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Matchmaking.DTOs;
using CodeClash.Domain.Enums;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Commands.SubmitSolution;

public record SubmitSolutionCommand(
    Guid                    MatchId,
    Guid                    UserId,
    SubmitSolutionRequestDto Dto
) : IRequest<Result<SubmissionResultDto>>;
