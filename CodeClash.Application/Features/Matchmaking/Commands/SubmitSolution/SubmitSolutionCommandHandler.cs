using CodeClash.Application.Common.Interfaces;
using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Matchmaking.DTOs;
using CodeClash.Domain.Entities;
using CodeClash.Domain.Enums;
using MediatR;

namespace CodeClash.Application.Features.Matchmaking.Commands.SubmitSolution;

/// <summary>
/// Handles a player's code submission during a live match.
///
/// Winner Priority (per spec):
///  1. First Accepted Solution (IsAccepted = true, earliest SubmittedAt)
///  2. Most Test Cases Passed
///  3. Lowest Execution Time
///  4. Lowest Memory Usage
///  5. Draw
/// </summary>
public class SubmitSolutionCommandHandler : IRequestHandler<SubmitSolutionCommand, Result<SubmissionResultDto>>
{
    private readonly IMatchArenaRepository _arenaRepo;
    private readonly IEloRatingService     _elo;
    private readonly IMatchHubService      _hub;

    public SubmitSolutionCommandHandler(
        IMatchArenaRepository arenaRepo,
        IEloRatingService     elo,
        IMatchHubService      hub)
    {
        _arenaRepo = arenaRepo;
        _elo       = elo;
        _hub       = hub;
    }

    public async Task<Result<SubmissionResultDto>> Handle(
        SubmitSolutionCommand request,
        CancellationToken     cancellationToken)
    {
        var arena = await _arenaRepo.GetByIdAsync(request.MatchId, cancellationToken);
        if (arena is null)
            return Result<SubmissionResultDto>.Failure("Match not found.");

        if (!arena.InvolvesUser(request.UserId))
            return Result<SubmissionResultDto>.Failure("You are not a participant in this match.");

        if (arena.Status != MatchStatus.InProgress)
            return Result<SubmissionResultDto>.Failure($"Match is not in progress (status: {arena.Status}).");

        // ── Stub: Replace with real judge integration ─────────────────────────
        // In production, call Judge0/Piston here and await the results.
        var totalTestCases  = 10;   // placeholder — loaded from Problem.TestCases.Count
        var testCasesPassed = totalTestCases; // stub: assume all pass
        var executionTimeMs = 120L;
        var memoryUsedMb    = 32L;
        var isAccepted      = true;
        // ─────────────────────────────────────────────────────────────────────

        var submission = MatchSubmission.Create(
            request.MatchId,
            request.UserId,
            request.Dto.Language,
            request.Dto.SourceCode,
            testCasesPassed,
            totalTestCases,
            executionTimeMs,
            memoryUsedMb,
            isAccepted);

        await _arenaRepo.AddSubmissionAsync(submission, cancellationToken);

        // Notify opponent
        var opponentId = arena.GetOpponentId(request.UserId);
        await _hub.NotifyOpponentSubmittedAsync(request.MatchId, opponentId, cancellationToken);

        // ── Check if match can be resolved now ────────────────────────────────
        var allSubmissions = await _arenaRepo.GetSubmissionsForMatchAsync(request.MatchId, cancellationToken);
        var p1Subs = allSubmissions.Where(s => s.UserId == arena.PlayerOneId).ToList();
        var p2Subs = allSubmissions.Where(s => s.UserId == arena.PlayerTwoId).ToList();

        var matchCompleted = await TryResolveMatchAsync(
            arena, p1Subs, p2Subs, cancellationToken);

        var dto = new SubmissionResultDto(
            submission.Id,
            isAccepted,
            testCasesPassed,
            totalTestCases,
            executionTimeMs,
            memoryUsedMb,
            isAccepted ? "All test cases passed!" : $"{testCasesPassed}/{totalTestCases} test cases passed.");

        return Result<SubmissionResultDto>.Success(dto, "Solution submitted successfully.");
    }

    // ── Winner determination ──────────────────────────────────────────────────

    private async Task<bool> TryResolveMatchAsync(
        MatchArena            arena,
        List<MatchSubmission> p1Subs,
        List<MatchSubmission> p2Subs,
        CancellationToken     ct)
    {
        // We need at least one submission from each player to evaluate
        if (!p1Subs.Any() || !p2Subs.Any()) return false;

        var p1Best = GetBestSubmission(p1Subs);
        var p2Best = GetBestSubmission(p2Subs);

        Guid? winnerId = DetermineWinner(arena, p1Best, p2Best);

        // Compute ELO
        int p1NewRating, p2NewRating;
        if (winnerId is null)
        {
            (p1NewRating, p2NewRating) = _elo.CalculateDraw(
                arena.PlayerOneRatingBefore, arena.PlayerTwoRatingBefore);
        }
        else if (winnerId == arena.PlayerOneId)
        {
            (p1NewRating, p2NewRating) = _elo.Calculate(
                arena.PlayerOneRatingBefore, arena.PlayerTwoRatingBefore);
        }
        else
        {
            (p2NewRating, p1NewRating) = _elo.Calculate(
                arena.PlayerTwoRatingBefore, arena.PlayerOneRatingBefore);
        }

        arena.Complete(winnerId, p1NewRating, p2NewRating);

        // Store match history (two rows — one per player)
        var p1Result = winnerId is null ? MatchResult.Draw
                     : winnerId == arena.PlayerOneId ? MatchResult.Win : MatchResult.Loss;
        var p2Result = winnerId is null ? MatchResult.Draw
                     : winnerId == arena.PlayerTwoId ? MatchResult.Win : MatchResult.Loss;

        var h1 = MatchHistory.Create(arena.Id, arena.PlayerOneId, arena.PlayerTwoId,
                     arena.ProblemId, p1Result, arena.PlayerOneRatingBefore, p1NewRating);
        var h2 = MatchHistory.Create(arena.Id, arena.PlayerTwoId, arena.PlayerOneId,
                     arena.ProblemId, p2Result, arena.PlayerTwoRatingBefore, p2NewRating);

        await _arenaRepo.AddHistoryAsync(h1, ct);
        await _arenaRepo.AddHistoryAsync(h2, ct);

        // Signal match completed
        string winnerName = winnerId is null ? "Draw" : "Winner determined";
        await _hub.NotifyMatchCompletedAsync(arena.Id, new MatchCompletedPayload(
            winnerId,
            winnerName,
            p1NewRating,
            p2NewRating), ct);

        return true;
    }

    private static MatchSubmission GetBestSubmission(List<MatchSubmission> subs)
    {
        // Priority: IsAccepted first, then most test cases, then earliest time, then least memory
        return subs
            .OrderByDescending(s => s.IsAccepted)
            .ThenByDescending(s => s.TestCasesPassed)
            .ThenBy(s => s.ExecutionTimeMs)
            .ThenBy(s => s.MemoryUsedMb)
            .First();
    }

    private static Guid? DetermineWinner(
        MatchArena       arena,
        MatchSubmission  p1Best,
        MatchSubmission  p2Best)
    {
        // Rule 1: First accepted solution
        if (p1Best.IsAccepted && !p2Best.IsAccepted) return arena.PlayerOneId;
        if (p2Best.IsAccepted && !p1Best.IsAccepted) return arena.PlayerTwoId;
        if (p1Best.IsAccepted && p2Best.IsAccepted)
        {
            if (p1Best.SubmittedAt < p2Best.SubmittedAt) return arena.PlayerOneId;
            if (p2Best.SubmittedAt < p1Best.SubmittedAt) return arena.PlayerTwoId;
        }

        // Rule 2: Most test cases passed
        if (p1Best.TestCasesPassed > p2Best.TestCasesPassed) return arena.PlayerOneId;
        if (p2Best.TestCasesPassed > p1Best.TestCasesPassed) return arena.PlayerTwoId;

        // Rule 3: Lowest execution time
        if (p1Best.ExecutionTimeMs < p2Best.ExecutionTimeMs) return arena.PlayerOneId;
        if (p2Best.ExecutionTimeMs < p1Best.ExecutionTimeMs) return arena.PlayerTwoId;

        // Rule 4: Lowest memory
        if (p1Best.MemoryUsedMb < p2Best.MemoryUsedMb) return arena.PlayerOneId;
        if (p2Best.MemoryUsedMb < p1Best.MemoryUsedMb) return arena.PlayerTwoId;

        // Rule 5: Draw
        return null;
    }
}
