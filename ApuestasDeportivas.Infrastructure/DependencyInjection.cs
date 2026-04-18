using ApuestasDeportivas.Application.Common.Interfaces;
using ApuestasDeportivas.Domain.Constants;
using ApuestasDeportivas.Domain.Entities;
using ApuestasDeportivas.Infrastructure.Persistence;
using ApuestasDeportivas.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;

namespace ApuestasDeportivas.Infrastructure;

/// <summary>
/// Registro centralizado de dependencias de infraestructura.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                              ?? "Data Source=apuestasdeportivas.db";

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddHttpClient<IOddsService, OddsApiService>(client =>
        {
            client.BaseAddress = new Uri("https://api.the-odds-api.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
            // Forzamos HTTP/1.1 para evitar problemas de negociación en algunos entornos.
            client.DefaultRequestVersion = HttpVersion.Version11;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            // Usamos configuración de proxy del sistema operativo (si existe).
            var systemProxy = WebRequest.GetSystemWebProxy();

            return new HttpClientHandler
            {
                Proxy = systemProxy,
                UseProxy = systemProxy is not null,
                DefaultProxyCredentials = CredentialCache.DefaultCredentials
            };
        });

        return services;
    }

    /// <summary>
    /// Crea roles y un admin inicial para poder gestionar apuestas pendientes.
    /// </summary>
    public static async Task SeedIdentityAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        if (!await roleManager.RoleExistsAsync(AppRoles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));
        }

        if (!await roleManager.RoleExistsAsync(AppRoles.User))
        {
            await roleManager.CreateAsync(new IdentityRole(AppRoles.User));
        }

        var adminEmail = config["Seed:AdminEmail"] ?? "admin@apuestas.local";
        var adminPassword = config["Seed:AdminPassword"] ?? "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Administrador",
                Balance = 0m,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"No se pudo crear admin inicial: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.User))
        {
            await userManager.AddToRoleAsync(admin, AppRoles.User);
        }
    }
}
