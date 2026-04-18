namespace ApuestasDeportivas.Contracts.Wallet;

/// <summary>
/// Solicitud de depósito para vistas de usuario.
/// </summary>
public record DepositRequestDto(
    Guid Id,
    decimal Amount,
    string TransactionId,
    string Status,
    DateTimeOffset CreatedAt);
