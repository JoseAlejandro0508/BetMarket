namespace ApuestasDeportivas.Contracts.Auth;

/// <summary>
/// Perfil básico del usuario autenticado.
/// </summary>
public record UserProfileDto(
    string Id,
    string DisplayName,
    string Email,
    decimal Balance);
