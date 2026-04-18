namespace ApuestasDeportivas.Contracts.Odds;

/// <summary>
/// Oferta de apuesta h2h que se mostrará en el front.
/// </summary>
public record OddsOfferDto(
    string EventId,
    string SportKey,
    DateTimeOffset CommenceTime,
    string HomeTeam,
    string AwayTeam,
    BookmakerOddsDto Bookmaker);
