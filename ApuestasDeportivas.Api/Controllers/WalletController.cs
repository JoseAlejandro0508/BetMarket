using System.Security.Claims;
using ApuestasDeportivas.Application.Services;
using ApuestasDeportivas.Contracts.Wallet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApuestasDeportivas.Api.Controllers;

/// <summary>
/// Endpoints para depósitos, retiros y settings de retiro.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly WalletService _walletService;

    public WalletController(WalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<WalletSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var summary = await _walletService.GetSummaryAsync(userId, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("deposits")]
    public async Task<ActionResult<IReadOnlyCollection<DepositRequestDto>>> GetDeposits(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var deposits = await _walletService.GetDepositsAsync(userId, cancellationToken);
        return Ok(deposits);
    }

    [HttpPost("deposits")]
    public async Task<ActionResult<DepositRequestDto>> CreateDeposit(CreateDepositRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        try
        {
            var created = await _walletService.CreateDepositAsync(userId, request, cancellationToken);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("withdrawals")]
    public async Task<ActionResult<IReadOnlyCollection<WithdrawalRequestDto>>> GetWithdrawals(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var withdrawals = await _walletService.GetWithdrawalsAsync(userId, cancellationToken);
        return Ok(withdrawals);
    }

    [HttpPost("withdrawals")]
    public async Task<ActionResult<WithdrawalRequestDto>> CreateWithdrawal(CreateWithdrawalRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        try
        {
            var created = await _walletService.CreateWithdrawalAsync(userId, request, cancellationToken);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("withdrawal-settings")]
    public async Task<ActionResult<UserWithdrawalSettingsDto>> GetWithdrawalSettings(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var settings = await _walletService.GetWithdrawalSettingsAsync(userId, cancellationToken);
        return Ok(settings);
    }

    [HttpPut("withdrawal-settings")]
    public async Task<ActionResult<UserWithdrawalSettingsDto>> UpdateWithdrawalSettings(
        UpdateWithdrawalSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        try
        {
            var settings = await _walletService.UpdateWithdrawalSettingsAsync(userId, request, cancellationToken);
            return Ok(settings);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
