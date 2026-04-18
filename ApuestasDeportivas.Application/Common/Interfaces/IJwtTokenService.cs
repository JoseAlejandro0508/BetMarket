using ApuestasDeportivas.Domain.Entities;

namespace ApuestasDeportivas.Application.Common.Interfaces;

/// <summary>
/// Contrato para emitir tokens JWT autenticados.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Crea un token JWT con claims del usuario y sus roles.
    /// </summary>
    string GenerateToken(AppUser user, IList<string> roles);
}
