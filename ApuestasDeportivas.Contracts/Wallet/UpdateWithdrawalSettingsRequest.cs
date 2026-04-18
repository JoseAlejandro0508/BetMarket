namespace ApuestasDeportivas.Contracts.Wallet;

/// <summary>
/// Actualización de settings de retiro del usuario.
/// </summary>
public record UpdateWithdrawalSettingsRequest(
    string Method,
    string Account,
    string? DisplayName);
