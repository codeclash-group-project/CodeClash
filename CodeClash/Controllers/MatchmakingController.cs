using CodeClash.API.Common;
using CodeClash.API.Extensions;
using CodeClash.Application.Features.Matchmaking.Commands.JoinQueue;
using CodeClash.Application.Features.Matchmaking.Commands.LeaveQueue;
using CodeClash.Application.Features.Matchmaking.DTOs;
using CodeClash.Application.Features.Matchmaking.Queries.GetQueueStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeClash.API.Controllers;

[ApiController]
[Route("api/v1/matchmaking")]
[Authorize]
[Produces("application/json")]
public class MatchmakingController : ControllerBase
{
    private readonly IMediator _mediator;

    public MatchmakingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/v1/matchmaking/join
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Join the ranked matchmaking queue with selected preferences.
    /// Returns a queue entry ID. Listen to SignalR "MatchFound" event for match notification.
    /// </summary>
    [HttpPost("join")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> JoinQueue(
        [FromBody] JoinQueueRequestDto dto,
        CancellationToken ct = default)
    {
        var userId = User.GetUserId();

        // ELO is stored in the JWT claim "elo"; default to 1000 if not present
        var eloClaim = User.FindFirst("elo")?.Value;
        int elo      = int.TryParse(eloClaim, out var parsed) ? parsed : 1000;

        var result = await _mediator.Send(new JoinQueueCommand(userId, elo, dto), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<Guid>.Fail(result.Errors, result.Message));

        return Ok(ApiResponse<Guid>.Ok(result.Data, result.Message));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE /api/v1/matchmaking/leave
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Leave the matchmaking queue.</summary>
    [HttpDelete("leave")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LeaveQueue(CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new LeaveQueueCommand(userId), ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Errors, result.Message));

        return Ok(ApiResponse<object>.Ok(null, result.Message));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/v1/matchmaking/status
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Get current queue status including ELO search range and time in queue.</summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<QueueStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQueueStatus(CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetQueueStatusQuery(userId), ct);
        return Ok(ApiResponse<QueueStatusDto>.Ok(result.Data, result.Message));
    }
}
