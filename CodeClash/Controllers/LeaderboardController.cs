using CodeClash.API.Common;
using CodeClash.Application.Common.Models;
using CodeClash.Application.Features.Leaderboard.DTOs;
using CodeClash.Application.Features.Leaderboard.Queries.GetGlobalLeaderboard;
using CodeClash.Application.Features.Leaderboard.Queries.GetTop100Leaderboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CodeClash.API.Controllers;

[ApiController]
[Route("api/v1/leaderboard")]
[Produces("application/json")]
public class LeaderboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaderboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/v1/leaderboard/global
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Returns the global leaderboard sorted by ELO descending, paginated.
    /// Public endpoint — no authentication required.
    /// </summary>
    [HttpGet("global")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<LeaderboardEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGlobalLeaderboard(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetGlobalLeaderboardQuery(page, pageSize), ct);
        return Ok(ApiResponse<PaginatedList<LeaderboardEntryDto>>.Ok(result.Data, result.Message));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/v1/leaderboard/top100
    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Returns the top 100 players sorted by ELO descending.
    /// Public endpoint — no authentication required.
    /// </summary>
    [HttpGet("top100")]
    [ProducesResponseType(typeof(ApiResponse<List<LeaderboardEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTop100Leaderboard(CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTop100LeaderboardQuery(), ct);
        return Ok(ApiResponse<List<LeaderboardEntryDto>>.Ok(result.Data, result.Message));
    }
}
