using ApuestasDeportivas.Contracts.Odds;

namespace ApuestasDeportivas.Application.Common.Interfaces;

/// <summary>
/// Contrato para consultar ofertas de apuestas en The Odds API.
/// </summary>
public interface IOddsService
{
    /// <summary>
    /// Obtiene una lista de ofertas h2h filtradas por bookmaker onexbet.
    /// </summary>
    Task<IReadOnlyCollection<OddsOfferDto>> GetH2HOffersAsync(string sportKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene todos los deportes disponibles desde The Odds API.
    /// </summary>
    Task<IReadOnlyCollection<AvailableSportDto>> GetAvailableSportsAsync(CancellationToken cancellationToken = default);
}
