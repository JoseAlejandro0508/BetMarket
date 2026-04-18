namespace ApuestasDeportivas.Contracts.Wallet;

/// <summary>
/// Configuración de método/cuenta de retiro del usuario.
/// </summary>
public record UserWithdrawalSettingsDto(
    string Method,
    string Account,
    DateTimeOffset UpdatedAt);
