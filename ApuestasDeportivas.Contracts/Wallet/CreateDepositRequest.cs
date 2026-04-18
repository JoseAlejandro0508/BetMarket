namespace ApuestasDeportivas.Contracts.Wallet;

/// <summary>
/// Payload para crear solicitud de depósito.
/// </summary>
public record CreateDepositRequest(
    decimal Amount,
    string TransactionId);
