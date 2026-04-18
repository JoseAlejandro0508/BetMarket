using ApuestasDeportivas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApuestasDeportivas.Application.Common.Interfaces;

/// <summary>
/// Abstracción del DbContext para desacoplar la capa Application.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Tabla de usuarios de la plataforma.
    /// </summary>
    DbSet<AppUser> Users { get; }

    /// <summary>
    /// Tabla de apuestas de usuario.
    /// </summary>
    DbSet<UserBet> UserBets { get; }

    /// <summary>
    /// Ofertas persistidas localmente.
    /// </summary>
    DbSet<StoredOddsOffer> StoredOddsOffers { get; }

    /// <summary>
    /// Configuración de sincronización automática de odds.
    /// </summary>
    DbSet<OddsSyncSettings> OddsSyncSettings { get; }

    /// <summary>
    /// Solicitudes de depósito de usuarios.
    /// </summary>
    DbSet<DepositRequest> DepositRequests { get; }

    /// <summary>
    /// Solicitudes de retiro de usuarios.
    /// </summary>
    DbSet<WithdrawalRequest> WithdrawalRequests { get; }

    /// <summary>
    /// Configuración de retiro por usuario.
    /// </summary>
    DbSet<UserWithdrawalSettings> UserWithdrawalSettings { get; }

    /// <summary>
    /// Persiste cambios en base de datos.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
