using ApuestasDeportivas.Application.Common.Interfaces;
using ApuestasDeportivas.Contracts.Wallet;
using ApuestasDeportivas.Domain.Entities;
using ApuestasDeportivas.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ApuestasDeportivas.Application.Services;

/// <summary>
/// Caso de uso para depósitos, retiros y settings de retiro.
/// </summary>
public class WalletService
{
    private readonly IApplicationDbContext _dbContext;

    public WalletService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WalletSummaryDto> GetSummaryAsync(string userId, CancellationToken cancellationToken = default)
    {
        var deposits = await _dbContext.DepositRequests
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var withdrawals = await _dbContext.WithdrawalRequests
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var totalDeposited = deposits
            .Where(x => x.Status == TransactionStatus.Approved)
            .Sum(x => x.Amount);

        var pendingDeposits = deposits
            .Where(x => x.Status == TransactionStatus.Pending)
            .Sum(x => x.Amount);

        var totalWithdrawn = withdrawals
            .Where(x => x.Status == TransactionStatus.Approved)
            .Sum(x => x.Amount);

        var pendingWithdrawals = withdrawals
            .Where(x => x.Status == TransactionStatus.Pending)
            .Sum(x => x.Amount);

        return new WalletSummaryDto(totalDeposited, pendingDeposits, totalWithdrawn, pendingWithdrawals);
    }

    public async Task<IReadOnlyCollection<DepositRequestDto>> GetDepositsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var deposits = await _dbContext.DepositRequests
            .Where(x => x.UserId == userId)
            .Select(x => new DepositRequestDto(x.Id, x.Amount, x.TransactionId, x.Status.ToString(), x.CreatedAt))
            .ToListAsync(cancellationToken);

        return deposits
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public async Task<DepositRequestDto> CreateDepositAsync(string userId, CreateDepositRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("El monto de depósito debe ser mayor que 0.");
        }

        if (string.IsNullOrWhiteSpace(request.TransactionId))
        {
            throw new InvalidOperationException("El id de transacción es requerido.");
        }

        var deposit = new DepositRequest
        {
            UserId = userId,
            Amount = request.Amount,
            TransactionId = request.TransactionId.Trim(),
            Status = TransactionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.DepositRequests.Add(deposit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DepositRequestDto(deposit.Id, deposit.Amount, deposit.TransactionId, deposit.Status.ToString(), deposit.CreatedAt);
    }

    public async Task<IReadOnlyCollection<WithdrawalRequestDto>> GetWithdrawalsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var withdrawals = await _dbContext.WithdrawalRequests
            .Where(x => x.UserId == userId)
            .Select(x => new WithdrawalRequestDto(x.Id, x.Amount, x.Method.ToString(), x.Account, x.Status.ToString(), x.CreatedAt))
            .ToListAsync(cancellationToken);

        return withdrawals
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public async Task<WithdrawalRequestDto> CreateWithdrawalAsync(string userId, CreateWithdrawalRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("El monto de retiro debe ser mayor que 0.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                   ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (user.Balance < request.Amount)
        {
            throw new InvalidOperationException("Balance insuficiente para retiro.");
        }

        var settings = await _dbContext.UserWithdrawalSettings
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Configura primero tu método/cuenta de retiro en settings.");

        user.Balance -= request.Amount;

        var withdrawal = new WithdrawalRequest
        {
            UserId = userId,
            Amount = request.Amount,
            Method = settings.Method,
            Account = settings.Account,
            Status = TransactionStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.WithdrawalRequests.Add(withdrawal);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new WithdrawalRequestDto(
            withdrawal.Id,
            withdrawal.Amount,
            withdrawal.Method.ToString(),
            withdrawal.Account,
            withdrawal.Status.ToString(),
            withdrawal.CreatedAt);
    }

    public async Task<UserWithdrawalSettingsDto> GetWithdrawalSettingsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var settings = await _dbContext.UserWithdrawalSettings
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (settings is null)
        {
            return new UserWithdrawalSettingsDto(WithdrawalMethod.Cup.ToString(), string.Empty, DateTimeOffset.MinValue);
        }

        return new UserWithdrawalSettingsDto(settings.Method.ToString(), settings.Account, settings.UpdatedAt);
    }

    public async Task<UserWithdrawalSettingsDto> UpdateWithdrawalSettingsAsync(
        string userId,
        UpdateWithdrawalSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Account))
        {
            throw new InvalidOperationException("La cuenta de retiro es requerida.");
        }

        if (!Enum.TryParse<WithdrawalMethod>(request.Method, true, out var parsedMethod))
        {
            throw new InvalidOperationException("Método de retiro inválido. Usa CUP, MLC o QvaPay.");
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                   ?? throw new InvalidOperationException("Usuario no encontrado.");

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            user.DisplayName = request.DisplayName.Trim();
        }

        var settings = await _dbContext.UserWithdrawalSettings
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (settings is null)
        {
            settings = new UserWithdrawalSettings
            {
                UserId = userId,
                Method = parsedMethod,
                Account = request.Account.Trim(),
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.UserWithdrawalSettings.Add(settings);
        }
        else
        {
            settings.Method = parsedMethod;
            settings.Account = request.Account.Trim();
            settings.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UserWithdrawalSettingsDto(settings.Method.ToString(), settings.Account, settings.UpdatedAt);
    }
}
