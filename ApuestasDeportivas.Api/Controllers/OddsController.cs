using ApuestasDeportivas.Application.Services;
using ApuestasDeportivas.Contracts.Odds;
using ApuestasDeportivas.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApuestasDeportivas.Api.Controllers;

/// <summary>
/// Endpoints para consultar ofertas de apuestas h2h.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OddsController : ControllerBase
{
    private readonly OddsService _oddsService;

    public OddsController(OddsService oddsService)
    {
        _oddsService = oddsService;
    }

    [HttpGet("offers")]
    public async Task<ActionResult<IReadOnlyCollection<StoredOddsOfferDto>>> GetOffers([FromQuery] string? sportKey = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = string.IsNullOrWhiteSpace(sportKey)
                ? "multi"
                : sportKey;

            // Nunca consultamos la API al cargar; siempre leemos DB local.
            await _oddsService.AutoRefreshIfEnabledAsync(key, cancellationToken);
            var offers = await _oddsService.GetStoredOffersAsync(key, cancellationToken);
            return Ok(offers);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("sync-settings")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<OddsSyncSettingsDto>> GetSyncSettings(CancellationToken cancellationToken)
    {
        var settings = await _oddsService.GetSyncSettingsAsync(cancellationToken);
        return Ok(settings);
    }

    [HttpPut("sync-settings")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<OddsSyncSettingsDto>> UpdateSyncSettings(UpdateOddsSyncSettingsRequest request, CancellationToken cancellationToken)
    {
        var settings = await _oddsService.UpdateSyncSettingsAsync(
            request.AutoRefreshEnabled,
            request.RefreshIntervalSeconds,
            request.SportKey,
            request.SelectedSportKeys,
            cancellationToken);

        return Ok(settings);
    }

    [HttpGet("sports/available")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<IReadOnlyCollection<AvailableSportDto>>> GetAvailableSports(CancellationToken cancellationToken)
    {
        var sports = await _oddsService.GetAvailableSportsAsync(cancellationToken);
        return Ok(sports);
    }

    [HttpPost("refresh")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<OddsSyncSettingsDto>> RefreshNow([FromQuery] string? sportKey = null, CancellationToken cancellationToken = default)
    {
        var key = string.IsNullOrWhiteSpace(sportKey)
            ? "multi"
            : sportKey;

        try
        {
            var settings = await _oddsService.RefreshOffersAsync(key, cancellationToken);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
