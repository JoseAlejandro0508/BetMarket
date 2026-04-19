using System.Text;
using ApuestasDeportivas.Application;
using ApuestasDeportivas.Application.Common.Interfaces;
using ApuestasDeportivas.Domain.Constants;
using ApuestasDeportivas.Infrastructure;
using ApuestasDeportivas.Infrastructure.Persistence;
using ApuestasDeportivas.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Registramos servicios de presentación (controladores) y OpenAPI.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Registramos servicios de capas internas.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configuración de CORS para permitir al frontend React consumir la API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configuración de JWT Bearer para autenticar peticiones protegidas.
var secret = builder.Configuration["Jwt:Secret"] ?? "CAMBIAR_ESTE_SECRETO_POR_UNO_MUY_SEGURO_1234567890";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "ApuestasDeportivas.Api";
var audience = builder.Configuration["Jwt:Audience"] ?? "ApuestasDeportivas.Web";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(AppRoles.Admin));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Forzamos creación de base de datos en arranque (sin migraciones manuales).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    // Compatibilidad incremental para SQLite en entorno dev sin migraciones:
    // si la DB ya existía, EnsureCreated no agrega tablas/columnas nuevas.
    await dbContext.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS StoredOddsOffers (
    Id TEXT NOT NULL PRIMARY KEY,
    EventId TEXT NOT NULL,
    SportKey TEXT NOT NULL,
    HomeTeam TEXT NOT NULL,
    AwayTeam TEXT NOT NULL,
    CommenceTime TEXT NOT NULL,
    PayloadJson TEXT NOT NULL,
    SyncedAt TEXT NOT NULL
);
");

    await dbContext.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS OddsSyncSettings (
    Id INTEGER NOT NULL PRIMARY KEY,
    AutoRefreshEnabled INTEGER NOT NULL,
    RefreshIntervalSeconds INTEGER NOT NULL,
    LastRefreshAt TEXT NULL,
    LastError TEXT NOT NULL DEFAULT ''
);
");

    await dbContext.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS DepositRequests (
    Id TEXT NOT NULL PRIMARY KEY,
    UserId TEXT NOT NULL,
    Amount TEXT NOT NULL,
    TransactionId TEXT NOT NULL,
    Status INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    CONSTRAINT FK_DepositRequests_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
");

    await dbContext.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS WithdrawalRequests (
    Id TEXT NOT NULL PRIMARY KEY,
    UserId TEXT NOT NULL,
    Amount TEXT NOT NULL,
    Method INTEGER NOT NULL,
    Account TEXT NOT NULL,
    Status INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    CONSTRAINT FK_WithdrawalRequests_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
");

    await dbContext.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS UserWithdrawalSettings (
    Id TEXT NOT NULL PRIMARY KEY,
    UserId TEXT NOT NULL,
    Method INTEGER NOT NULL,
    Account TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    CONSTRAINT FK_UserWithdrawalSettings_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
");

    await dbContext.Database.ExecuteSqlRawAsync(@"
CREATE UNIQUE INDEX IF NOT EXISTS IX_UserWithdrawalSettings_UserId
ON UserWithdrawalSettings(UserId);
");

    // Evitamos ejecutar ALTER TABLE si la columna ya existe para no contaminar logs con errores esperados.
    await using var connection = dbContext.Database.GetDbConnection();
    if (connection.State != System.Data.ConnectionState.Open)
    {
        await connection.OpenAsync();
    }

    var columnExists = false;
    await using (var command = connection.CreateCommand())
    {
        command.CommandText = "PRAGMA table_info('OddsSyncSettings');";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var columnName = reader.GetString(1);
            if (string.Equals(columnName, "CurrentSportKey", StringComparison.OrdinalIgnoreCase))
            {
                columnExists = true;
                break;
            }
        }
    }

    if (!columnExists)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            "ALTER TABLE OddsSyncSettings ADD COLUMN CurrentSportKey TEXT NOT NULL DEFAULT 'multi';");
    }

    var selectedKeysColumnExists = false;
    await using (var command = connection.CreateCommand())
    {
        command.CommandText = "PRAGMA table_info('OddsSyncSettings');";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var columnName = reader.GetString(1);
            if (string.Equals(columnName, "SelectedSportKeysJson", StringComparison.OrdinalIgnoreCase))
            {
                selectedKeysColumnExists = true;
                break;
            }
        }
    }

    if (!selectedKeysColumnExists)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            "ALTER TABLE OddsSyncSettings ADD COLUMN SelectedSportKeysJson TEXT NOT NULL DEFAULT '[]';");
    }
}

// Seed de roles y admin inicial para operar panel administrativo.
await ApuestasDeportivas.Infrastructure.DependencyInjection.SeedIdentityAsync(app.Services);

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
