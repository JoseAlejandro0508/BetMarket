namespace ApuestasDeportivas.Domain.Entities;

/// <summary>
/// Configuración de sincronización automática de ofertas de apuestas.
/// </summary>
public class OddsSyncSettings
{
    private const string DefaultSports = "[\"basketball_nba\",\"baseball_mlb\",\"soccer_epl\"]";

    public int Id { get; set; } = 1;

    public bool AutoRefreshEnabled { get; set; }

    public string CurrentSportKey { get; set; } = "multi";

    public int RefreshIntervalSeconds { get; set; } = 60;

    public DateTimeOffset? LastRefreshAt { get; set; }

    public string LastError { get; set; } = string.Empty;

    /// <summary>
    /// Lista serializada de sport keys seleccionados por admin para sync multi.
    /// </summary>
    public string SelectedSportKeysJson { get; set; } = DefaultSports;
}
