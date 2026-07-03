using FluentValidation;

namespace CodeClash.Application.Features.Matchmaking.Commands.JoinQueue;

public class JoinQueueCommandValidator : AbstractValidator<JoinQueueCommand>
{
    private static readonly int[] ValidDurations = [15, 30, 45, 60];

    public JoinQueueCommandValidator()
    {
        RuleFor(x => x.Dto.DurationMinutes)
            .Must(d => ValidDurations.Contains(d))
            .WithMessage("Duration must be 15, 30, 45, or 60 minutes.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Elo)
            .GreaterThan(0)
            .WithMessage("ELO must be a positive value.");
    }
}
