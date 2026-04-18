using ApuestasDeportivas.Domain.Enums;

namespace ApuestasDeportivas.Domain.Entities;

/// <summary>
/// Apuesta realizada por un usuario para un evento y outcome específico.
/// </summary>
public class UserBet
{
    /// <summary>
    /// Identificador interno de la apuesta.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Id del evento en The Odds API.
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Clave del deporte (ej: soccer_spain_la_liga).
    /// </summary>
    public string SportKey { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del equipo local.
    /// </summary>
    public string HomeTeam { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del equipo visitante.
    /// </summary>
    public string AwayTeam { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de comienzo del evento.
    /// </summary>
    public DateTimeOffset CommenceTime { get; set; }

    /// <summary>
    /// Bookmaker seleccionado para tomar la cuota (onexbet).
    /// </summary>
    public string BookmakerKey { get; set; } = "onexbet";

    /// <summary>
    /// Nombre legible del bookmaker.
    /// </summary>
    public string BookmakerTitle { get; set; } = "1xBet";

    /// <summary>
    /// Mercado de la apuesta (h2h).
    /// </summary>
    public string MarketKey { get; set; } = "h2h";

    /// <summary>
    /// Selección elegida por el usuario (home/away/draw).
    /// </summary>
    public string SelectedOutcomeName { get; set; } = string.Empty;

    /// <summary>
    /// Cuota decimal capturada al momento de apostar.
    /// </summary>
    public decimal SelectedOutcomePrice { get; set; }

    /// <summary>
    /// Monto apostado.
    /// </summary>
    public decimal Stake { get; set; }

    /// <summary>
    /// Estado actual de la apuesta.
    /// </summary>
    public BetStatus Status { get; set; } = BetStatus.Pending;

    /// <summary>
    /// Fecha de creación de la apuesta.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Fecha en que el admin resolvió la apuesta.
    /// </summary>
    public DateTimeOffset? ResolvedAt { get; set; }

    /// <summary>
    /// Id del usuario dueño de la apuesta.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navegación al usuario dueño.
    /// </summary>
    public AppUser? User { get; set; }
}
