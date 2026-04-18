namespace ApuestasDeportivas.Contracts.Auth;

/// <summary>
/// Datos requeridos para registrar un usuario nuevo.
/// </summary>
public record RegisterRequest(string DisplayName, string Email, string Password);
