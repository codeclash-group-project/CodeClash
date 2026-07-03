using CodeClash.API.Common;
using CodeClash.API.Extensions;
using CodeClash.Application.Features.Matchmaking.Commands.SubmitSolution;
using CodeClash.Application.Features.Matchmaking.DTOs;
using CodeClash.Application.Features.Matchmaking.Queries.GetCurrentMatch;
using CodeClash.Application.Features.Matchmaking.Queries.GetMatchArena;
using CodeClash.Application.Features.Matchmaking.Queries.GetMatchResult;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeClash.API.Controllers;

[ApiController]
[Route("api/v1/matches")]
[Authorize]
[Produces("application/json")]
public class MatchController : ControllerBase
{
    private readonly IMediator _mediator;

    public MatchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/v1/matches/current
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Returns the caller's active or countdown match, or null if not in a match.</summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<MatchArenaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentMatch(CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetCurrentMatchQuery(userId), ct);
        return Ok(ApiResponse<MatchArenaDto?>.Ok(result.Data, result.Message));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/v1/matches/{matchId}
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Returns full match arena details for a participant.</summary>
    [HttpGet("{matchId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MatchArenaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMatchArena(
        Guid matchId, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetMatchArenaQuery(matchId, userId), ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<MatchArenaDto>.Fail(result.Errors, result.Message));

        return Ok(ApiResponse<MatchArenaDto>.Ok(result.Data, result.Message));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/v1/matches/{matchId}/submit
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Submit a solution during a live match.</summary>
    [HttpPost("{matchId:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<SubmissionResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SubmitSolution(
        Guid matchId,
        [FromBody] SubmitSolutionRequestDto dto,
        CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new SubmitSolutionCommand(matchId, userId, dto), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<SubmissionResultDto>.Fail(result.Errors, result.Message));

        return Ok(ApiResponse<SubmissionResultDto>.Ok(result.Data, result.Message));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/v1/matches/{matchId}/result
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Returns the final match result with ELO changes (caller-personalised view).</summary>
    [HttpGet("{matchId:guid}/result")]
    [ProducesResponseType(typeof(ApiResponse<MatchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMatchResult(
        Guid matchId, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetMatchResultQuery(matchId, userId), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<MatchResultDto>.Fail(result.Errors, result.Message));

        return Ok(ApiResponse<MatchResultDto>.Ok(result.Data, result.Message));
    }
}
