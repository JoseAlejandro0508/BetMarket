using System.Security.Claims;
using ApuestasDeportivas.Application.Services;
using ApuestasDeportivas.Contracts.Bets;
using ApuestasDeportivas.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApuestasDeportivas.Api.Controllers;

/// <summary>
/// Endpoints para crear apuestas, ver historial y resolver pendientes (admin).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BetsController : ControllerBase
{
    private readonly BetsService _betsService;

    public BetsController(BetsService betsService)
    {
        _betsService = betsService;
    }

    [HttpPost]
    public async Task<ActionResult<UserBetDto>> PlaceBet(PlaceBetRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            var bet = await _betsService.PlaceBetAsync(userId, request, cancellationToken);
            return Ok(bet);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyCollection<UserBetDto>>> MyBets(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var bets = await _betsService.GetMyBetsAsync(userId, cancellationToken);
        return Ok(bets);
    }

    [HttpGet("pending")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<IReadOnlyCollection<UserBetDto>>> PendingBets(CancellationToken cancellationToken)
    {
        var pending = await _betsService.GetPendingBetsAsync(cancellationToken);
        return Ok(pending);
    }

    [HttpPatch("{betId:guid}/resolve")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<UserBetDto>> ResolveBet(Guid betId, ResolveBetRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var resolved = await _betsService.ResolveBetAsync(betId, request.IsWon, cancellationToken);
            return Ok(resolved);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
