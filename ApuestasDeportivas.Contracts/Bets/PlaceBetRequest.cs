namespace ApuestasDeportivas.Contracts.Bets;

/// <summary>
/// Payload para crear una apuesta desde el frontend.
/// </summary>
public record PlaceBetRequest(
    string EventId,
    string SportKey,
    string HomeTeam,
    string AwayTeam,
    DateTimeOffset CommenceTime,
    string BookmakerKey,
    string BookmakerTitle,
    string MarketKey,
    string SelectedOutcomeName,
    decimal SelectedOutcomePrice,
    decimal Stake);
