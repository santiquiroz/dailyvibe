# DailyVibe — Diseño

**Fecha:** 2026-04-14
**Contexto:** Proyecto de aprendizaje intensivo (≈6 h) para preparar una entrevista técnica .NET + Angular. El proyecto debe ejercitar, a nivel mínimo pero real, los temas que un tech lead usaría para sondear competencia: CQRS + MediatR, JWT, EF Core, Clean Architecture, tests (unit + integration) y Angular con interceptor/guard.

## 1. Resumen

API en .NET + frontend en Angular que entrega un "mensaje del día" personalizable (motivacional, reflexivo, humorístico, estoico, técnico…). El mensaje se genera llamando a un modelo local servido por **LM Studio** en `http://localhost:1234` (endpoint OpenAI-compatible `/v1/chat/completions`). Los usuarios se autentican con JWT y pueden ver su historial.

## 2. Arquitectura

Clean Architecture simplificada en 4 proyectos + tests + cliente Angular:

```
DailyVibe/
├── src/
│   ├── DailyVibe.Domain/          entidades, sin dependencias
│   ├── DailyVibe.Application/     CQRS handlers, DTOs, interfaces, validators
│   ├── DailyVibe.Infrastructure/  EF DbContext, JwtService, LmStudioClient
│   └── DailyVibe.Api/             controllers, Program.cs, middleware
├── tests/
│   └── DailyVibe.Tests/           xUnit + Moq + WebApplicationFactory
└── client/
    └── dailyvibe-web/             Angular standalone
```

**Regla de dependencias:** Domain ← Application ← Infrastructure, y Api compone todo (composition root).

## 3. Modelo de dominio

- **User**: `Id (Guid)`, `Email`, `PasswordHash`, `DefaultIntent` (string, ej. "motivacional y estoico, en español, 2 frases máx"), `CreatedAt`.
- **DailyMessage**: `Id (Guid)`, `UserId (FK)`, `Content`, `Intent`, `CreatedAt`.
- Relación 1:N `User → DailyMessages`, configurada con `IEntityTypeConfiguration<T>`.

## 4. Endpoints

| Método | Ruta | Auth | Propósito |
|---|---|---|---|
| POST | `/api/auth/register` | no | Registro + devuelve JWT |
| POST | `/api/auth/login` | no | Login + devuelve JWT |
| GET | `/api/messages/today` | sí | Devuelve mensaje de hoy; si no existe, lo genera |
| POST | `/api/messages/generate` | sí | Genera uno nuevo on-demand con intención custom |
| GET | `/api/messages/history?page&size` | sí | Historial paginado |
| PUT | `/api/preferences` | sí | Actualiza `DefaultIntent` del usuario |

## 5. CQRS (MediatR)

- **Commands:** `RegisterUserCommand`, `LoginCommand`, `GenerateMessageCommand`, `UpdatePreferencesCommand`.
- **Queries:** `GetTodayMessageQuery`, `GetMessageHistoryQuery`.
- **Pipeline behavior** de validación con FluentValidation — para demostrar comprensión de MediatR a nivel medio.
- `GetTodayMessageQuery` puede disparar `GenerateMessageCommand` vía `IMediator` si no existe mensaje del día (ejemplo realista).

## 6. Seguridad

- Password hash con **BCrypt.Net-Next** (simple y suficiente para la demo).
- JWT firmado con HS256, secret en `appsettings.Development.json`. Access token 60 min. Sin refresh token en la versión base (opcional si hay tiempo).
- `[Authorize]` en controllers protegidos.
- CORS abierto a `http://localhost:4200` en dev.

## 7. Integración con LM Studio

- Interfaz `ILmStudioClient` en `Application`.
- Implementación `LmStudioHttpClient` en `Infrastructure` usando `HttpClient` tipado (`AddHttpClient<ILmStudioClient, LmStudioHttpClient>()`), apuntando a `http://localhost:1234/v1/chat/completions`.
- Modelo por defecto: `google/gemma-4-26b-a4b` (configurable en `appsettings`).
- Prompt: combina la `intent` del usuario con un system prompt fijo que pide 1–2 frases concisas.

## 8. Tests

- **Unit (xUnit + Moq):**
  1. `GenerateMessageCommandHandler` — mockea `ILmStudioClient` y repositorio, verifica que persiste lo que devuelve el modelo.
  2. `LoginCommandHandler` — verifica que con password incorrecto lanza excepción de auth.
- **Integration (WebApplicationFactory + SQLite en memoria):**
  1. Flow: register → login → generate → history. Verifica 200s y cantidades.

## 9. Angular (standalone)

- Standalone components, signals, reactive forms, HttpClient.
- **Páginas:** `login`, `register`, `home` (protegida, con textarea de intención, botón "Generar" y lista de historial).
- **Interceptor** que adjunta `Authorization: Bearer <token>` desde `localStorage`.
- **Guard** (`CanActivateFn`) que redirige a `/login` si no hay token.
- **Service** `ApiService` con métodos tipados.
- Estilos mínimos con CSS plano (sin Tailwind para ahorrar tiempo de config).

## 10. Stack / paquetes

**Backend:**
- .NET 10 (SDK 10.0.202 ya instalado)
- EF Core + **SQLite** (`Microsoft.EntityFrameworkCore.Sqlite`)
- MediatR
- FluentValidation 12 + `FluentValidation.DependencyInjectionExtensions` (validación vía el `ValidationBehavior` de MediatR; `FluentValidation.AspNetCore` no se usa)
- BCrypt.Net-Next
- Microsoft.AspNetCore.Authentication.JwtBearer
- Swashbuckle.AspNetCore (Swagger con auth)
- xUnit, Moq, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing

**Frontend:**
- Angular CLI 21.2.7 (vía `npx`)
- Angular standalone + signals + HttpClient

## 11. Cronograma (6 h)

| Fase | Tiempo | Qué se construye |
|---|---|---|
| 0 | 0:00–0:20 | Scaffolding solución + proyectos + paquetes |
| 1 | 0:20–1:00 | Domain + Infrastructure (DbContext, migración, EF config) |
| 2 | 1:00–1:45 | JwtService + LmStudioClient + DI |
| 3 | 1:45–2:45 | Application: handlers CQRS + validators + pipeline |
| 4 | 2:45–3:15 | Api: controllers, middleware de errores, Swagger |
| 5 | 3:15–3:45 | Tests (2 unit + 1 integration) |
| 6 | 3:45–5:30 | Angular: routing, login, guard, interceptor, home |
| 7 | 5:30–6:00 | E2E, fix bugs, repasar conceptos para entrevista |

## 12. Fuera de alcance (YAGNI explícito)

- Refresh tokens (se menciona en entrevista pero no se implementa salvo que sobre tiempo).
- Roles/autorización por policies.
- Docker, CI/CD.
- Tailwind o librerías de UI.
- Logging estructurado (Serilog) — se deja el default de ASP.NET.
- Cache de respuestas del LLM.

## 13. Criterios de "hecho"

1. `dotnet run` levanta la API en `https://localhost:xxxx` con Swagger visible.
2. Puedo registrarme, loguearme y llamar `/api/messages/today` con el JWT — devuelve texto generado por LM Studio.
3. El historial paginado funciona.
4. `dotnet test` pasa los 3 tests.
5. `npx ng serve` levanta en `http://localhost:4200`; login funciona; la página protegida muestra el mensaje del día y el historial.
6. Puedo explicar en voz alta, frente a cada pieza: qué es, por qué está ahí, qué patrón aplica y cómo responderlo en entrevista.
