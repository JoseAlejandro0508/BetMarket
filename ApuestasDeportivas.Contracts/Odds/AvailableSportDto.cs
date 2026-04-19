namespace ApuestasDeportivas.Contracts.Odds;

/// <summary>
/// Deporte disponible en The Odds API para selección en panel admin.
/// </summary>
public record AvailableSportDto(
    string Key,
    string Group,
    string Title,
    string Description);
