namespace ApuestasDeportivas.Contracts.Bets;

/// <summary>
/// Representación de apuesta para vistas de usuario y admin.
/// </summary>
public record UserBetDto(
    Guid Id,
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
    decimal Stake,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt,
    string UserId,
    string UserDisplayName,
    string UserEmail);
