using ApuestasDeportivas.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ApuestasDeportivas.Application;

/// <summary>
/// Registro de servicios de la capa Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<OddsService>();
        services.AddScoped<BetsService>();
        services.AddScoped<WalletService>();

        return services;
    }
}
