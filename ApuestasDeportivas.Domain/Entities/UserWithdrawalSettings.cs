using ApuestasDeportivas.Domain.Enums;

namespace ApuestasDeportivas.Domain.Entities;

/// <summary>
/// Configuración de retiro del usuario.
/// </summary>
public class UserWithdrawalSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public WithdrawalMethod Method { get; set; } = WithdrawalMethod.Cup;
    public string Account { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public AppUser? User { get; set; }
}
