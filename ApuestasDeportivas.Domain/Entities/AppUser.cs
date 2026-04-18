using Microsoft.AspNetCore.Identity;

namespace ApuestasDeportivas.Domain.Entities;

/// <summary>
/// Usuario de la plataforma. Hereda de IdentityUser para reutilizar autenticación.
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>
    /// Nombre para mostrar del usuario dentro del front.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Balance disponible para apostar.
    /// </summary>
    public decimal Balance { get; set; } = 100m;

    /// <summary>
    /// Colección de apuestas realizadas por el usuario.
    /// </summary>
    public ICollection<UserBet> Bets { get; set; } = new List<UserBet>();

    /// <summary>
    /// Depósitos solicitados por el usuario.
    /// </summary>
    public ICollection<DepositRequest> Deposits { get; set; } = new List<DepositRequest>();

    /// <summary>
    /// Retiros solicitados por el usuario.
    /// </summary>
    public ICollection<WithdrawalRequest> Withdrawals { get; set; } = new List<WithdrawalRequest>();

    /// <summary>
    /// Preferencias de método/cuenta para retiro.
    /// </summary>
    public UserWithdrawalSettings? WithdrawalSettings { get; set; }
}
