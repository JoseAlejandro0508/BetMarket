namespace ApuestasDeportivas.Domain.Entities;

/// <summary>
/// Oferta de apuestas persistida localmente para evitar consultar la API en cada recarga.
/// </summary>
public class StoredOddsOffer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string EventId { get; set; } = string.Empty;

    public string SportKey { get; set; } = string.Empty;

    public string HomeTeam { get; set; } = string.Empty;

    public string AwayTeam { get; set; } = string.Empty;

    public DateTimeOffset CommenceTime { get; set; }

    /// <summary>
    /// JSON serializado con bookmaker y mercados para devolverlo tal cual al frontend.
    /// </summary>
    public string PayloadJson { get; set; } = string.Empty;

    public DateTimeOffset SyncedAt { get; set; } = DateTimeOffset.UtcNow;
}
