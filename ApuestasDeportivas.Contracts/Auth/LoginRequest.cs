namespace ApuestasDeportivas.Contracts.Auth;

/// <summary>
/// Datos requeridos para iniciar sesión.
/// </summary>
public record LoginRequest(string Email, string Password);
