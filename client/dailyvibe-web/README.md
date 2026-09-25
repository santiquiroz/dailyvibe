# dailyvibe-web

Cliente Angular 21 (standalone, signals, reactive forms, sin zone.js) de DailyVibe (spec §9).

- `/login` y `/register`: un mismo componente (`AuthPage`) cuyo modo llega por `data` de la ruta.
- `/`: página protegida por `authGuard` (`CanActivateFn`) con el mensaje de hoy, la intención, "Generar", "Guardar como predeterminada" y el historial paginado.
- `authInterceptor` añade `Authorization: Bearer <token>` (token en `localStorage['dailyvibe.token']`) solo a las peticiones dirigidas a la API.
- `sessionExpiredInterceptor`: si una petición autenticada recibe 401, borra el token y vuelve a `/login`.
- `ApiService`: métodos tipados para las 6 rutas de la API.

## Cómo correrlo

Requiere Node 22+ y la API en su perfil `http` (`http://localhost:5208`, que es la URL de `API_BASE_URL` en `src/app/app.config.ts`). La API solo acepta CORS desde `http://localhost:4200` y solo en Development.

```bash
dotnet run --project ../../src/DailyVibe.Api --launch-profile http   # desde client/dailyvibe-web
npm ci
npx ng serve                                                        # http://localhost:4200
```

## Build y tests

```bash
npx ng build
npx ng test --watch=false --browsers=ChromeHeadless
```

Los tests (Karma + Jasmine) usan `HttpTestingController`, así que no necesitan la API ni LM Studio.
