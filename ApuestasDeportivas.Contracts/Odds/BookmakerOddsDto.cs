namespace ApuestasDeportivas.Contracts.Odds;

/// <summary>
/// Estructura de bookmaker y su mercado h2h.
/// </summary>
public record BookmakerOddsDto(
    string Key,
    string Title,
    DateTimeOffset LastUpdate,
    IReadOnlyCollection<MarketOddsDto> Markets);
