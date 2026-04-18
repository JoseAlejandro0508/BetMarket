namespace ApuestasDeportivas.Domain.Enums;

/// <summary>
/// Estado de una apuesta dentro del ciclo de vida del negocio.
/// </summary>
public enum BetStatus
{
    Pending = 0,
    Won = 1,
    Lost = 2
}
