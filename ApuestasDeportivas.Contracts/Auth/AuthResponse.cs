namespace ApuestasDeportivas.Contracts.Auth;

/// <summary>
/// Resultado de autenticación para el frontend.
/// </summary>
public record AuthResponse(
    string Token,
    DateTimeOffset ExpiresAt,
    UserProfileDto User,
    IReadOnlyCollection<string> Roles);
