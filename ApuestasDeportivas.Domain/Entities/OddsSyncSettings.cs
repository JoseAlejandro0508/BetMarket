namespace ApuestasDeportivas.Domain.Entities;

/// <summary>
/// Configuración de sincronización automática de ofertas de apuestas.
/// </summary>
public class OddsSyncSettings
{
    public int Id { get; set; } = 1;

    public bool AutoRefreshEnabled { get; set; }

    public string CurrentSportKey { get; set; } = "upcoming";

    public int RefreshIntervalSeconds { get; set; } = 60;

    public DateTimeOffset? LastRefreshAt { get; set; }

    public string LastError { get; set; } = string.Empty;
}
