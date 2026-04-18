using System.Text.Json;
using System.Text.Json.Serialization;
using ApuestasDeportivas.Application.Common.Interfaces;
using ApuestasDeportivas.Contracts.Odds;
using Microsoft.Extensions.Configuration;

namespace ApuestasDeportivas.Infrastructure.Services;

/// <summary>
/// Cliente HTTP para consumir The Odds API filtrando mercado h2h y bookmaker onexbet.
/// </summary>
public class OddsApiService : IOddsService
{
    private const string BookmakerKey = "onexbet";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OddsApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<IReadOnlyCollection<OddsOfferDto>> GetH2HOffersAsync(string sportKey, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["OddsApi:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("No se configuró OddsApi:ApiKey.");
        }

        var normalizedSport = string.IsNullOrWhiteSpace(sportKey) ? "upcoming" : sportKey.Trim();
        var endpoint = $"v4/sports/{normalizedSport}/odds/?apiKey={apiKey}&regions=eu&bookmakers={BookmakerKey}&markets=h2h&oddsFormat=decimal&dateFormat=iso";

        using var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"The Odds API devolvió {(int)response.StatusCode}: {content}");
        }

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var events = JsonSerializer.Deserialize<List<OddsApiEvent>>(content, options) ?? new List<OddsApiEvent>();

        var offers = events
            .Select(MapEvent)
            .Where(x => x is not null)
            .Cast<OddsOfferDto>()
            .ToList();

        return offers;
    }

    private static OddsOfferDto? MapEvent(OddsApiEvent apiEvent)
    {
        var bookmaker = apiEvent.Bookmakers?
            .FirstOrDefault(b => string.Equals(b.Key, BookmakerKey, StringComparison.OrdinalIgnoreCase));

        if (bookmaker is null)
        {
            return null;
        }

        var h2hMarket = bookmaker.Markets?
            .FirstOrDefault(m => string.Equals(m.Key, "h2h", StringComparison.OrdinalIgnoreCase));

        if (h2hMarket is null || h2hMarket.Outcomes is null || h2hMarket.Outcomes.Count == 0)
        {
            return null;
        }

        var mappedMarket = new MarketOddsDto(
            h2hMarket.Key ?? "h2h",
            h2hMarket.LastUpdate ?? bookmaker.LastUpdate ?? DateTimeOffset.UtcNow,
            h2hMarket.Outcomes
                .Where(o => !string.IsNullOrWhiteSpace(o.Name))
                .Select(o => new OutcomeOddsDto(o.Name!, o.Price))
                .ToList());

        return new OddsOfferDto(
            apiEvent.Id ?? string.Empty,
            apiEvent.SportKey ?? string.Empty,
            apiEvent.CommenceTime ?? DateTimeOffset.UtcNow,
            apiEvent.HomeTeam ?? string.Empty,
            apiEvent.AwayTeam ?? string.Empty,
            new BookmakerOddsDto(
                bookmaker.Key ?? BookmakerKey,
                bookmaker.Title ?? "1xBet",
                bookmaker.LastUpdate ?? DateTimeOffset.UtcNow,
                new List<MarketOddsDto> { mappedMarket }));
    }

    private sealed class OddsApiEvent
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("sport_key")]
        public string? SportKey { get; set; }

        [JsonPropertyName("commence_time")]
        public DateTimeOffset? CommenceTime { get; set; }

        [JsonPropertyName("home_team")]
        public string? HomeTeam { get; set; }

        [JsonPropertyName("away_team")]
        public string? AwayTeam { get; set; }

        [JsonPropertyName("bookmakers")]
        public List<OddsApiBookmaker>? Bookmakers { get; set; }
    }

    private sealed class OddsApiBookmaker
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("last_update")]
        public DateTimeOffset? LastUpdate { get; set; }

        [JsonPropertyName("markets")]
        public List<OddsApiMarket>? Markets { get; set; }
    }

    private sealed class OddsApiMarket
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("last_update")]
        public DateTimeOffset? LastUpdate { get; set; }

        [JsonPropertyName("outcomes")]
        public List<OddsApiOutcome>? Outcomes { get; set; }
    }

    private sealed class OddsApiOutcome
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }
}
