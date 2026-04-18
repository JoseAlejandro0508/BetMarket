namespace ApuestasDeportivas.Contracts.Wallet;

/// <summary>
/// Resumen de montos para panel de wallet.
/// </summary>
public record WalletSummaryDto(
    decimal TotalDeposited,
    decimal PendingDeposits,
    decimal TotalWithdrawn,
    decimal PendingWithdrawals);
