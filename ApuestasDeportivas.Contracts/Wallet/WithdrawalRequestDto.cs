namespace ApuestasDeportivas.Contracts.Wallet;

/// <summary>
/// Solicitud de retiro para vistas de usuario.
/// </summary>
public record WithdrawalRequestDto(
    Guid Id,
    decimal Amount,
    string Method,
    string Account,
    string Status,
    DateTimeOffset CreatedAt);
