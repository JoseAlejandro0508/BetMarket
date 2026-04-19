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
    private static readonly string[] CoreSports = ["basketball_nba", "baseball_mlb", "soccer_epl"];

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
        var normalizedSport = string.IsNullOrWhiteSpace(sportKey) ? "multi" : sportKey.Trim();
        var query = _dbContext.StoredOddsOffers.AsQueryable();

        // En modo multi devolvemos todo el snapshot guardado.
        if (!IsMultiMode(normalizedSport))
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
        IReadOnlyCollection<string>? selectedSportKeys,
        CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        settings.AutoRefreshEnabled = enabled;
        settings.RefreshIntervalSeconds = Math.Clamp(intervalSeconds, 15, 3600);
        settings.CurrentSportKey = string.IsNullOrWhiteSpace(sportKey) ? "multi" : sportKey.Trim();

        if (selectedSportKeys is not null)
        {
            var normalized = selectedSportKeys
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            settings.SelectedSportKeysJson = JsonSerializer.Serialize(normalized);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapSettings(settings);
    }

    public async Task<OddsSyncSettingsDto> RefreshOffersAsync(string sportKey, CancellationToken cancellationToken = default)
    {
        var normalizedSport = string.IsNullOrWhiteSpace(sportKey) ? "multi" : sportKey.Trim();
        string fetchWarning = string.Empty;

        IReadOnlyCollection<OddsOfferDto> apiOffers;
        if (IsMultiMode(normalizedSport))
        {
            var fetchResult = await FetchConfiguredSportsOffersAsync(cancellationToken);
            apiOffers = fetchResult.Offers;
            fetchWarning = fetchResult.Warning;
        }
        else
        {
            apiOffers = await _oddsApiService.GetH2HOffersAsync(normalizedSport, cancellationToken);
        }

        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        settings.CurrentSportKey = IsMultiMode(normalizedSport) ? "multi" : normalizedSport;

        // Si rebotamos por rate limit y no trajimos nada, conservamos snapshot previo.
        if (apiOffers.Count == 0 && !string.IsNullOrWhiteSpace(fetchWarning))
        {
            settings.LastError = fetchWarning;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapSettings(settings);
        }

        var existing = IsMultiMode(normalizedSport)
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
        settings.LastError = fetchWarning;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapSettings(settings);
    }

    public async Task AutoRefreshIfEnabledAsync(string sportKey, CancellationToken cancellationToken = default)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        if (!settings.AutoRefreshEnabled) return;

        var normalizedSport = string.IsNullOrWhiteSpace(sportKey) ? "multi" : sportKey.Trim();
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
        return string.IsNullOrWhiteSpace(settings.CurrentSportKey) ? "multi" : settings.CurrentSportKey;
    }

    public async Task<IReadOnlyCollection<AvailableSportDto>> GetAvailableSportsAsync(CancellationToken cancellationToken = default)
    {
        return await _oddsApiService.GetAvailableSportsAsync(cancellationToken);
    }

    private async Task<OddsSyncSettings> GetOrCreateSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await _dbContext.OddsSyncSettings.FirstOrDefaultAsync(x => x.Id == 1, cancellationToken);
        if (settings is not null) return settings;

        settings = new OddsSyncSettings
        {
            Id = 1,
            AutoRefreshEnabled = false,
            CurrentSportKey = "multi",
            RefreshIntervalSeconds = _configuration.GetValue<int?>("OddsSync:DefaultIntervalSeconds") ?? 60,
            SelectedSportKeysJson = JsonSerializer.Serialize(CoreSports)
        };
        _dbContext.OddsSyncSettings.Add(settings);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return settings;
    }

    private async Task<(IReadOnlyCollection<OddsOfferDto> Offers, string Warning)> FetchConfiguredSportsOffersAsync(CancellationToken cancellationToken)
    {
        var settings = await GetOrCreateSettingsAsync(cancellationToken);
        var configured = ParseSelectedSportKeys(settings.SelectedSportKeysJson);
        var sportKeys = configured.Count == 0 ? CoreSports.ToList() : configured;

        var allOffers = new List<OddsOfferDto>();
        var warnings = new List<string>();

        foreach (var sportKey in sportKeys.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var sportOffers = await _oddsApiService.GetH2HOffersAsync(sportKey, cancellationToken);
                allOffers.AddRange(sportOffers);

                // Suavizamos ráfaga para reducir probabilidad de EXCEEDED_FREQ_LIMIT.
                await Task.Delay(180, cancellationToken);
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("EXCEEDED_FREQ_LIMIT", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("too frequent", StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add("Rate limit alcanzado durante sync. Se guardó un snapshot parcial y se conserva el anterior si no hubo resultados.");
                break;
            }
            catch (InvalidOperationException ex) when (
                ex.Message.Contains("INVALID_MARKET_COMBO", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Invalid parameter combination", StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add($"Sport '{sportKey}' omitido por combinación de mercado inválida (INVALID_MARKET_COMBO).");
                continue;
            }
            catch (Exception ex)
            {
                warnings.Add($"Sport '{sportKey}' falló: {ex.Message}");
                continue;
            }
        }

        var merged = allOffers
            .GroupBy(x => x.EventId)
            .Select(g => g.First())
            .ToList();

        var warning = string.Join(" | ", warnings.Distinct());
        return (merged, warning);
    }

    private static List<string> ParseSelectedSportKeys(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static bool IsMultiMode(string sportKey)
    {
        return string.Equals(sportKey, "multi", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sportKey, "all", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sportKey, "upcoming", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sportKey, "core", StringComparison.OrdinalIgnoreCase);
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
        => new(
            settings.AutoRefreshEnabled,
            settings.CurrentSportKey,
            settings.RefreshIntervalSeconds,
            settings.LastRefreshAt,
            settings.LastError,
            ParseSelectedSportKeys(settings.SelectedSportKeysJson));
}
