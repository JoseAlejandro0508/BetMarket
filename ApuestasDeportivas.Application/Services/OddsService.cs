using System.Text.Json;
using ApuestasDeportivas.Application.Common.Interfaces;
using ApuestasDeportivas.Contracts.Odds;
using ApuestasDeportivas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ApuestasDeportivas.Application.Services;

/// <summary>
/// Maneja lectura persistida de ofertas y sincronización manual/automática.
/// </summary>
public class OddsService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IOddsService _oddsApiService;
    private readonly IConfiguration _configuration;

    public OddsService(IApplicationDbContext dbContext, IOddsService oddsApiService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _oddsApiService = oddsApiService;
        _configuration = configuration;
    }

    public async Task<IReadOnlyCollection<StoredOddsOfferDto>> GetStoredOffersAsync(string sportKey, CancellationToken cancellationToken = default)
    {
        var normalizedSport = string.IsNullOrWhiteSpace(sportKey) ? "upcoming" : sportKey.Trim();
        var query = _dbContext.StoredOddsOffers.AsQueryable();

        // Si el sport key actual es "upcoming", devolvemos todo el snapshot guardado
        // (cada evento mantiene su sportKey real para mostrar el deporte en UI).
        if (!string.Equals(normalizedSport, "upcoming", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.SportKey == normalizedSport);
        }

        var offers = await query.ToListAsync(cancellationToken);

        return offers
            .OrderByDescending(x => x.SyncedAt)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<OddsSyncSettingsDto> GetSyncSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        return MapSettings(settings);
    }

    public async Task<OddsSyncSettingsDto> UpdateSyncSettingsAsync(
        bool enabled,
        int intervalSeconds,
        string? sportKey,
        CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        settings.AutoRefreshEnabled = enabled;
        settings.RefreshIntervalSeconds = Math.Clamp(intervalSeconds, 15, 3600);
        settings.CurrentSportKey = string.IsNullOrWhiteSpace(sportKey) ? "upcoming" : sportKey.Trim();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapSettings(settings);
    }

    public async Task<OddsSyncSettingsDto> RefreshOffersAsync(string sportKey, CancellationToken cancellationToken = default)
    {
        var normalizedSport = string.IsNullOrWhiteSpace(sportKey) ? "upcoming" : sportKey.Trim();
        var apiOffers = await _oddsApiService.GetH2HOffersAsync(normalizedSport, cancellationToken);
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        settings.CurrentSportKey = normalizedSport;

        var existing = string.Equals(normalizedSport, "upcoming", StringComparison.OrdinalIgnoreCase)
            ? await _dbContext.StoredOddsOffers.ToListAsync(cancellationToken)
            : await _dbContext.StoredOddsOffers.Where(x => x.SportKey == normalizedSport).ToListAsync(cancellationToken);

        _dbContext.StoredOddsOffers.RemoveRange(existing);

        foreach (var offer in apiOffers)
        {
            _dbContext.StoredOddsOffers.Add(new StoredOddsOffer
            {
                EventId = offer.EventId,
                SportKey = offer.SportKey,
                HomeTeam = offer.HomeTeam,
                AwayTeam = offer.AwayTeam,
                CommenceTime = offer.CommenceTime,
                PayloadJson = JsonSerializer.Serialize(offer),
                SyncedAt = DateTimeOffset.UtcNow
            });
        }

        settings.LastRefreshAt = DateTimeOffset.UtcNow;
        settings.LastError = string.Empty;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapSettings(settings);
    }

    public async Task AutoRefreshIfEnabledAsync(string sportKey, CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        if (!settings.AutoRefreshEnabled) return;

        var normalizedSport = string.IsNullOrWhiteSpace(sportKey) ? "upcoming" : sportKey.Trim();
        settings.CurrentSportKey = normalizedSport;
        if (settings.LastRefreshAt is not null)
        {
            var elapsed = DateTimeOffset.UtcNow - settings.LastRefreshAt.Value;
            if (elapsed.TotalSeconds < settings.RefreshIntervalSeconds)
            {
                return;
            }
        }

        try
        {
            await RefreshOffersAsync(normalizedSport, cancellationToken);
        }
        catch (Exception ex)
        {
            settings.LastError = ex.Message;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<string> GetCurrentSportKeyAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(settings.CurrentSportKey) ? "upcoming" : settings.CurrentSportKey;
    }

    private async Task<OddsSyncSettings> GetOrCreateSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await _dbContext.OddsSyncSettings.FirstOrDefaultAsync(x => x.Id == 1, cancellationToken);
        if (settings is not null) return settings;

        settings = new OddsSyncSettings
        {
            Id = 1,
            AutoRefreshEnabled = false,
            CurrentSportKey = "upcoming",
            RefreshIntervalSeconds = _configuration.GetValue<int?>("OddsSync:DefaultIntervalSeconds") ?? 60
        };
        _dbContext.OddsSyncSettings.Add(settings);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return settings;
    }

    private static StoredOddsOfferDto MapToDto(StoredOddsOffer offer)
    {
        var bookmaker = JsonSerializer.Deserialize<OddsOfferDto>(offer.PayloadJson);
        var book = bookmaker?.Bookmaker ?? new BookmakerOddsDto("onexbet", "1xBet", offer.SyncedAt, Array.Empty<MarketOddsDto>());

        return new StoredOddsOfferDto(
            offer.EventId,
            offer.SportKey,
            offer.CommenceTime,
            offer.HomeTeam,
            offer.AwayTeam,
            book,
            offer.SyncedAt);
    }

    private static OddsSyncSettingsDto MapSettings(OddsSyncSettings settings)
        => new(settings.AutoRefreshEnabled, settings.CurrentSportKey, settings.RefreshIntervalSeconds, settings.LastRefreshAt, settings.LastError);
}
