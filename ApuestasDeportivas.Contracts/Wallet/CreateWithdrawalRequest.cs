namespace ApuestasDeportivas.Contracts.Wallet;

/// <summary>
/// Payload para crear solicitud de retiro.
/// </summary>
public record CreateWithdrawalRequest(
    decimal Amount);
