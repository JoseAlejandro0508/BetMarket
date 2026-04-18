# ApuestasDeportivas

Proyecto fullstack de casa de apuestas deportivas con:

- **Frontend:** React + Vite
- **Backend:** ASP.NET Core 10 (Controladores)
- **Auth:** ASP.NET Identity + JWT
- **DB:** SQLite3 + Entity Framework Core
- **Odds provider:** [The Odds API](https://the-odds-api.com/) en mercado `h2h`
- **Bookmaker forzado:** `onexbet` (1xBet)

## Diseño visual (Stitch)

Se generó un proyecto de diseño visual en Stitch inspirado en 1xBet.

- **Stitch Project:** `projects/11309137477063861572`
- **Nombre:** `ApuestasDeportivas 1xBet Style`

## Requisitos funcionales implementados

- Registro e inicio de sesión de usuarios.
- Cada usuario tiene balance inicial de **100.00**.
- Listado de ofertas h2h desde The Odds API con bookmaker `onexbet`.
- Creación de apuestas por parte del usuario (selección + monto).
- Historial de apuestas del usuario.
- Panel admin para ver apuestas pendientes y marcarlas como **Ganada** o **Perdida**.
- Si admin marca una apuesta como ganada, se acredita `stake * cuota` al balance del usuario.

## Estructura

- `ApuestasDeportivas.Api` → API ASP.NET (controladores)
- `ApuestasDeportivas.Application` → casos de uso y servicios
- `ApuestasDeportivas.Domain` → entidades y reglas base
- `ApuestasDeportivas.Infrastructure` → EF Core, Identity, servicios externos
- `ApuestasDeportivas.Contracts` → DTOs compartidos
- `ApuestasDeportivas.Web` → frontend React

## Configuración

### 1) Backend (`ApuestasDeportivas.Api/appsettings.Development.json`)

Configura tu API key de The Odds API:

```json
"OddsApi": {
  "ApiKey": "PON_AQUI_TU_API_KEY_DE_THE_ODDS_API"
}
```

También puedes cambiar:

- `Jwt:Secret`
- `Seed:AdminEmail`
- `Seed:AdminPassword`

## Ejecución local

### Backend

```bash
dotnet run --project "ApuestasDeportivas.Api/ApuestasDeportivas.Api.csproj"
```

### Frontend

```bash
npm install
npm run dev
```

Desde:

```bash
ApuestasDeportivas.Web
```

## Credenciales admin por defecto

- Email: `admin@apuestas.local`
- Password: `Admin123!`

> Cambia estas credenciales en configuración para un entorno real.

## Notas técnicas

- La API usa `EnsureCreated()` para crear SQLite automáticamente al arrancar.
- Frontend apunta por defecto a `https://localhost:7255/api` (ajústalo en `src/api.ts` si cambia el puerto).
- Se comenta código en clases principales para facilitar mantenimiento futuro.
