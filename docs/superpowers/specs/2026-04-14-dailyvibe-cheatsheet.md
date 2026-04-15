# DailyVibe — Cheatsheet

Acompañante del documento de diseño. Dos partes: **comandos listos para ejecutar** y **respuestas de entrevista** por tema.

---

## Parte 1 — Comandos Fase 0 (scaffolding)

Ejecutar en orden desde `C:/Users/santi/Desktop/proyectos claudio/pruebas dot net`.

### 0.1 Crear solución y estructura de carpetas

```bash
dotnet new sln -n DailyVibe
mkdir -p src tests client
```

### 0.2 Crear los 4 proyectos de backend + tests

```bash
# Domain: librería pura, sin dependencias
dotnet new classlib -n DailyVibe.Domain -o src/DailyVibe.Domain -f net10.0

# Application: CQRS, handlers, interfaces
dotnet new classlib -n DailyVibe.Application -o src/DailyVibe.Application -f net10.0

# Infrastructure: EF, JWT, HTTP clients
dotnet new classlib -n DailyVibe.Infrastructure -o src/DailyVibe.Infrastructure -f net10.0

# Api: Web API (el composition root)
dotnet new webapi -n DailyVibe.Api -o src/DailyVibe.Api -f net10.0 --use-controllers

# Tests: xUnit
dotnet new xunit -n DailyVibe.Tests -o tests/DailyVibe.Tests -f net10.0
```

### 0.3 Añadir proyectos a la solución

```bash
dotnet sln add src/DailyVibe.Domain/DailyVibe.Domain.csproj
dotnet sln add src/DailyVibe.Application/DailyVibe.Application.csproj
dotnet sln add src/DailyVibe.Infrastructure/DailyVibe.Infrastructure.csproj
dotnet sln add src/DailyVibe.Api/DailyVibe.Api.csproj
dotnet sln add tests/DailyVibe.Tests/DailyVibe.Tests.csproj
```

### 0.4 Referencias entre proyectos (respetando Clean Architecture)

```bash
# Application depende de Domain
dotnet add src/DailyVibe.Application/DailyVibe.Application.csproj reference src/DailyVibe.Domain/DailyVibe.Domain.csproj

# Infrastructure depende de Application (y transitivamente Domain)
dotnet add src/DailyVibe.Infrastructure/DailyVibe.Infrastructure.csproj reference src/DailyVibe.Application/DailyVibe.Application.csproj

# Api depende de Application e Infrastructure
dotnet add src/DailyVibe.Api/DailyVibe.Api.csproj reference src/DailyVibe.Application/DailyVibe.Application.csproj
dotnet add src/DailyVibe.Api/DailyVibe.Api.csproj reference src/DailyVibe.Infrastructure/DailyVibe.Infrastructure.csproj

# Tests referencian todo (para poder testear handlers y API)
dotnet add tests/DailyVibe.Tests/DailyVibe.Tests.csproj reference src/DailyVibe.Api/DailyVibe.Api.csproj
dotnet add tests/DailyVibe.Tests/DailyVibe.Tests.csproj reference src/DailyVibe.Application/DailyVibe.Application.csproj
dotnet add tests/DailyVibe.Tests/DailyVibe.Tests.csproj reference src/DailyVibe.Infrastructure/DailyVibe.Infrastructure.csproj
```

### 0.5 Paquetes NuGet

```bash
# Application
dotnet add src/DailyVibe.Application package MediatR
dotnet add src/DailyVibe.Application package FluentValidation
dotnet add src/DailyVibe.Application package FluentValidation.DependencyInjectionExtensions

# Infrastructure
dotnet add src/DailyVibe.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/DailyVibe.Infrastructure package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/DailyVibe.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/DailyVibe.Infrastructure package BCrypt.Net-Next
dotnet add src/DailyVibe.Infrastructure package System.IdentityModel.Tokens.Jwt
dotnet add src/DailyVibe.Infrastructure package Microsoft.Extensions.Http

# Api
dotnet add src/DailyVibe.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/DailyVibe.Api package Microsoft.EntityFrameworkCore.Design
dotnet add src/DailyVibe.Api package Swashbuckle.AspNetCore
dotnet add src/DailyVibe.Api package MediatR
dotnet add src/DailyVibe.Api package FluentValidation.AspNetCore

# Tests
dotnet add tests/DailyVibe.Tests package Moq
dotnet add tests/DailyVibe.Tests package FluentAssertions
dotnet add tests/DailyVibe.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/DailyVibe.Tests package Microsoft.EntityFrameworkCore.InMemory

# Tool global de EF CLI (una sola vez por máquina)
dotnet tool install --global dotnet-ef
```

### 0.6 Verificar que compila

```bash
dotnet build
```

Si ves `Build succeeded` con 0 errores, la Fase 0 terminó.

### 0.7 Angular scaffolding (al inicio de Fase 6)

```bash
cd client
npx -y @angular/cli@21 new dailyvibe-web --standalone --routing --style=css --ssr=false --skip-git
cd dailyvibe-web
npx ng serve
```

---

## Parte 2 — Respuestas de entrevista por tema

Respuestas cortas y defendibles. Léelas mañana antes de la entrevista.

### ¿Qué arquitectura usas?

> Clean Architecture con 4 proyectos: Domain, Application, Infrastructure y Api. La regla es que las dependencias apuntan hacia adentro: Domain no depende de nadie, Application solo de Domain, Infrastructure implementa las interfaces que define Application, y Api es el composition root. Es la misma idea que Onion o Hexagonal, con nombres distintos. El beneficio práctico: puedo testear los handlers sin base de datos ni HTTP reales porque dependen de interfaces.

### ¿Qué es CQRS y por qué MediatR?

> CQRS separa operaciones de escritura (Commands) de lectura (Queries). Cada una tiene su modelo, su handler, su validación. MediatR es un mediador en proceso: el controller manda un `Command` al `IMediator` y este encuentra el handler correspondiente. Beneficios: controllers delgados, cada handler hace una sola cosa, fácil de testear. Uso un pipeline behavior para validación automática con FluentValidation.

### ¿Cuándo NO usarías CQRS?

> En CRUDs simples sin lógica de negocio, porque añades ceremonia sin beneficio. Tiene sentido cuando tienes validaciones, reglas, o cuando lecturas y escrituras tienen formas distintas. No lo confundo con "Event Sourcing" ni con bases de datos separadas para read/write — eso es CQRS extremo.

### ¿Cómo funciona JWT en tu proyecto?

> En login valido credenciales con BCrypt, si son correctas genero un JWT firmado con HS256 usando un secret de configuración. El token lleva el `sub` (UserId) y el `email` como claims. En la API configuro `AddAuthentication().AddJwtBearer(...)` con los parámetros de validación (issuer, audience, lifetime, signing key). Los endpoints protegidos llevan `[Authorize]` y accedo al usuario con `User.FindFirst(ClaimTypes.NameIdentifier)`.

### Access token vs refresh token

> Access token es de corta vida (1h en mi caso) y lleva la identidad. Refresh token es de larga vida, opaco, guardado en BD, y sirve para pedir un nuevo access sin re-login. No implementé refresh en esta demo para ahorrar tiempo, pero el diseño sería: tabla `RefreshTokens` con `UserId`, `Token`, `ExpiresAt`, `RevokedAt`; endpoint `/auth/refresh` que lo rota (emite nuevo access + nuevo refresh y revoca el anterior).

### Tracking vs AsNoTracking

> Por defecto EF Core trackea entidades leídas para detectar cambios en `SaveChanges`. Eso tiene coste de memoria y CPU. Si solo voy a leer (ej. endpoint GET de historial) uso `AsNoTracking()` — las entidades son "sueltas", no hay overhead. Regla mía: queries de solo lectura siempre `AsNoTracking`; cuando voy a modificar, tracking normal.

### Migraciones de EF Core

> `dotnet ef migrations add <Nombre>` genera las clases `Up/Down` comparando el modelo actual con el snapshot. `dotnet ef database update` las aplica. En Program.cs llamo `context.Database.Migrate()` al arrancar para auto-aplicar en dev. En producción se suele correr el update como paso de despliegue, no al arrancar.

### ¿Por qué SQLite aquí?

> Por rapidez de setup para la demo. En un proyecto real usaría SQL Server o PostgreSQL. Gracias a que EF abstrae el provider, cambiar es modificar el `UseSqlite(...)` por `UseSqlServer(...)` y las migraciones.

### Validaciones

> FluentValidation para reglas complejas y reutilizables. Cada Command tiene su Validator (`AbstractValidator<T>`). Lo conecto a MediatR con un pipeline behavior que valida antes de ejecutar el handler y lanza `ValidationException` si falla. Un middleware global traduce esa excepción a `400 Bad Request` con el detalle. Así los handlers asumen entrada válida.

### Tests — qué tipos hiciste

> Dos unitarios con xUnit + Moq: uno del handler de generar mensaje (mockeando `ILmStudioClient` y el repositorio, verifico que persiste lo que devuelve el modelo) y uno de login (password incorrecto lanza auth exception). Uno de integración con `WebApplicationFactory` que levanta la API en memoria con SQLite InMemory, hace register → login → generate → history y valida 200s. La pirámide de tests: muchos unitarios rápidos, pocos de integración que prueban el cableado real.

### Angular — lo clave

> App standalone (sin NgModules), routing con rutas lazy, `HttpClient` con `provideHttpClient(withInterceptors(...))`. Tengo un **interceptor** funcional que inyecta `Authorization: Bearer <token>` si hay token en localStorage. Un **guard** (`CanActivateFn`) protege rutas: si no hay token, redirige a `/login`. Los formularios de login/register usan Reactive Forms. Uso signals para el estado local de componentes.

### ¿Por qué standalone y no NgModules?

> Desde Angular 14–17 los standalone components son el default. Menos boilerplate, imports explícitos por componente, mejor tree-shaking, routing lazy más simple. NgModules siguen funcionando pero son legacy.

### Manejo de errores global

> Middleware en el pipeline de ASP.NET (`UseExceptionHandler`) que captura, mapea excepciones conocidas a status codes (`ValidationException → 400`, `UnauthorizedAccessException → 401`, `NotFoundException → 404`) y devuelve un ProblemDetails. Así controllers y handlers no se llenan de try/catch.

### ¿Cómo integras LM Studio?

> LM Studio expone una API compatible con OpenAI en `localhost:1234/v1/chat/completions`. Defino `ILmStudioClient` en Application y la implementación `LmStudioHttpClient` en Infrastructure usando `HttpClient` tipado (`AddHttpClient<ILmStudioClient, LmStudioHttpClient>()`). El cliente construye el payload con un system prompt fijo + el `intent` del usuario, parsea la respuesta y devuelve el texto. Configurable: base URL, modelo y timeout.

### Dependency Injection en .NET — lo básico

> Tres lifetimes: `Singleton` (una sola instancia para toda la app), `Scoped` (una por request — default para DbContext), `Transient` (una cada vez que se resuelve). DbContext es Scoped porque no es thread-safe. HttpClient va con `AddHttpClient` para que el factory maneje el pooling y evite socket exhaustion.

---

## Parte 3 — Orden mental mañana antes de entrevista

1. Leer este cheatsheet (15 min).
2. Abrir VS Code con el proyecto y pasear por las capas: mostrar que entiendes cada carpeta.
3. Tener **Swagger abierto** en otra pestaña listo para demostrar si te lo piden.
4. Tener Angular corriendo por si piden verlo.
5. Recordar: **hablar del *por qué* más que del *cómo***. Un tech lead quiere ver criterio, no sintaxis.
