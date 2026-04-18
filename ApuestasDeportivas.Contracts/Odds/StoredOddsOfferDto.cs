namespace ApuestasDeportivas.Contracts.Odds;

/// <summary>
/// Oferta persistida que el frontend consume desde DB local.
/// </summary>
public record StoredOddsOfferDto(
    string EventId,
    string SportKey,
    DateTimeOffset CommenceTime,
    string HomeTeam,
    string AwayTeam,
    BookmakerOddsDto Bookmaker,
    DateTimeOffset SyncedAt);
