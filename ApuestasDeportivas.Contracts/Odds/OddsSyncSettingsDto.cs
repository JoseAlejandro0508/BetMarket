namespace ApuestasDeportivas.Contracts.Odds;

/// <summary>
/// Configuración admin de sincronización de odds.
/// </summary>
public record OddsSyncSettingsDto(
    bool AutoRefreshEnabled,
    string CurrentSportKey,
    int RefreshIntervalSeconds,
    DateTimeOffset? LastRefreshAt,
    string LastError);
