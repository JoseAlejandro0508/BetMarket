namespace ApuestasDeportivas.Contracts.Odds;

/// <summary>
/// Mercado de odds, en este caso h2h.
/// </summary>
public record MarketOddsDto(
    string Key,
    DateTimeOffset LastUpdate,
    IReadOnlyCollection<OutcomeOddsDto> Outcomes);
