using FluentValidation;

namespace CodeClash.Application.Features.Matchmaking.Commands.SubmitSolution;

public class SubmitSolutionCommandValidator : AbstractValidator<SubmitSolutionCommand>
{
    public SubmitSolutionCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithMessage("Match ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Dto.SourceCode)
            .NotEmpty().WithMessage("Source code cannot be empty.")
            .MaximumLength(50_000).WithMessage("Source code must not exceed 50,000 characters.");
    }
}
