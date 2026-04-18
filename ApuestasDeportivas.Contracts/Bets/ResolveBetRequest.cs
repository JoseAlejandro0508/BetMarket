namespace ApuestasDeportivas.Contracts.Bets;

/// <summary>
/// Solicitud del admin para resolver una apuesta pendiente.
/// </summary>
public record ResolveBetRequest(bool IsWon);
