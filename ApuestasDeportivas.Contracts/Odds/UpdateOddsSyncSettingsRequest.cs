namespace ApuestasDeportivas.Contracts.Odds;

/// <summary>
/// Solicitud admin para actualizar comportamiento de sincronización de odds.
/// </summary>
public record UpdateOddsSyncSettingsRequest(
    bool AutoRefreshEnabled,
    int RefreshIntervalSeconds,
    string SportKey,
    IReadOnlyCollection<string>? SelectedSportKeys = null);
