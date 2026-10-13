<p align="center"><img src="frontend/public/alkemia-logo.png" alt="Alkemia" width="160"></p>

# Cuentas Â· Creatio (Prueba tÃ©cnica Alkemia)

ResolvÃ­ la prueba tÃ©cnica con una Resolví la prueba técnica con una app web que consume la **API OData de Creatio** autenticando contra el **Identity Service** con **OAuth 2.0 client credentials**. Muestra un listado de cuentas (Accounts) con bÃºsqueda y paginaciÃ³n resueltas del lado del servidor.

- **Backend:** ASP.NET Core Web API (`.NET 9`) â€” `backend/`
- **Frontend:** React + Vite + TypeScript + CSS nativo â€” `frontend/`
- **VersiÃ³n de Creatio:** v2026.5.6
- **Instancia:** https://189637-crm-bundle.creatio.com
- **BitÃ¡cora de la integraciÃ³n:** [`BITACORA.md`](BITACORA.md) â€” ahÃ­ cuento lo que me costÃ³ y cÃ³mo lo resolvÃ­.

---

## Estructura

```
backend/CreatioAccounts.Api/
  Program.cs                  composiciÃ³n (config, DI y pipeline)
  Configuration/             opciones de Creatio + AddCreatio() (bind + validaciÃ³n + DI)
  Contracts/
    Accounts/                DTOs de cuentas (AccountItem, AccountListResponse, CreateAccountRequest)
    AccountTypes/            DTO de tipos de cuenta (AccountType)
  Integration/               transporte hacia Creatio (anÃ¡logo a src/lib/api.ts)
    CreatioODataClient.cs    request + bearer + retry 401 + parseo
    CreatioTokenService.cs   cachÃ©/renovaciÃ³n del token (OAuth client credentials)
    CreatioApiException.cs
  Services/                  lÃ³gica de dominio (filtros OData $filter/$select/$expand)
    CreatioODataService.cs
  Infrastructure/
    EnvFileLoader.cs         carga de .env
    Middleware/              manejo global de errores (UseErrorHandling())
  Controllers/               AccountsController, AccountTypesController

frontend/
  src/views/                 composiciÃ³n de pantallas (AccountsView)
  src/components/ui/         primitivas reutilizables (Button, Input, Table, â€¦)
  src/components/accounts/   componentes del dominio Cuentas (tabla, alta, filtro por tipo)
  src/hooks/                 useAccounts (fetch), useAccountTypes, useDebounce
  src/lib/                   api client (getAccounts, getAccountTypes, createAccount)
  src/types/                 un tipo/interfaz por archivo (AccountItem, TableProps, â€¦)
  src/assets/
    constants/               ACCOUNT_COLUMNS, ACCOUNT_PAGE_SIZE
    img/                     imÃ¡genes procesadas por Vite
  src/global/style.css       tema nativo (paleta del logo) y clases de UI
```

---

## CÃ³mo levantar todo desde cero

### 1. Configurar credenciales

Crear un archivo `.env` en la raÃ­z del repo (no se sube, ver `.env.example`):

```
CREATIO_INSTANCE_URL=https://189637-crm-bundle.creatio.com
CREATIO_TOKEN_URL=https://189637-crm-bundle-is.creatio.com/connect/token
CREATIO_CLIENT_ID=<client-id>
CREATIO_CLIENT_SECRET=<client-secret>
CREATIO_ODATA_BASEPATH=/0/odata
```

TambiÃ©n se pueden definir como variables de entorno reales con esos mismos nombres. El backend valida que estÃ©n completas al arrancar.

> Las credenciales corresponden a la instancia provista con el challenge y no viajan en el repo por seguridad.

### 2. Backend

Desde la raÃ­z del repo:

```
cd backend
dotnet run
```

Queda en `http://localhost:5113`. Endpoints:

- `GET /api/accounts?search=nombre&type=Cliente&page=1&pageSize=10` â†’ `{ items, total, page, pageSize }`
- `GET /api/account-types` â†’ tipos de cuenta de Creatio (para los filtros del front)
- `POST /api/accounts` con `{ "name": "...", "typeId": "opcional" }` â†’ crea la cuenta (bonus)
- `GET /health`

### 3. Frontend

Desde la raÃ­z del repo, abrÃ­ otro terminal y:

```
cd frontend
npm install
npm run dev
```

AbrÃ­ `http://localhost:5173`. Vite hace proxy de `/api` y `/health` hacia `http://localhost:5113` (no hace falta CORS en dev). Con el backend corriendo, el listado deberÃ­a cargar con las cuentas de la instancia.

---

## CÃ³mo se configurÃ³ el acceso OAuth 2.0 (client credentials)

La primera pared que tuve que atravesar fue conseguir que el usuario tÃ©cnico pudiera consumir OData; sin los permisos correctos, todos los request devolvÃ­an *"El usuario actual no tiene permisos suficientes para usar OData"*. Esto es lo que hice:

1. **System Designer** â†’ **OAuth 2.0 integrated applications** (bloque _Import and integration_).
2. **New** â†’ tipo **Server-to-server (client credentials)**.
3. Completar Nombre/URL/DescripciÃ³n y dejar activado **Create separate technical user**.
4. Creatio genera **Client Id** y **Client secret**; se guardan fuera del repositorio (`.env` + variables de entorno).
5. Asignar permisos al **usuario tÃ©cnico**: operaciones **Access to OData** y **View any data** (sin eso, cualquier request devuelve `El usuario actual no tiene permisos suficientes para usar OData`).
6. El token se pide en `POST {IdentityServiceUrl}/connect/token` con `grant_type=client_credentials` (form-urlencoded: `client_id`, `client_secret`).

> La URL del Identity Service la da el system setting **`OAuth20IdentityServerUrl`** (_Authorization server Url for OAuth 2.0 integrations_).

Fuentes consultadas:

- Creatio Academy â€” [Set up client credentials grant](https://academy.creatio.com/docs/8.x/no-code-customization/base-integrations/oauth-2-0/set-up-the-oauth-2-0-authorization-instruction)
- Creatio Academy â€” [Authorize external requests (client credentials grant)](https://academy.creatio.com/guides/dev/development-on-creatio-platform/8.2/integrations-and-api/authentication/oauth-2-0-authorization/identity-service-overview)
- OAuth 2.0 â€” [Client types](https://oauth.net/2/client-types/) (por quÃ© el "Public client" tenÃ­a que quedar apagado)
- [Plecto â†’ Creatio integration guide](https://docs.plecto.com/kb/guide/en/creatio-IpChCf4mSq/Steps/4070710) (permisos del usuario tÃ©cnico para OData)

---

## Decisiones tÃ©cnicas y por quÃ©

- **OAuth solo del lado del backend.** El frontend nunca ve el `client_secret`; el backend es el Ãºnico orquestador de la integraciÃ³n. Eso mantiene las credenciales fuera del browser y simplifica la app web.
- **CachÃ© y renovaciÃ³n del token.** `CreatioTokenService` cachea el `access_token` en memoria con `SemaphoreSlim` (evita pedir uno nuevo en cada request y evita "stampede" cuando expiran). Renueva antes de la expiraciÃ³n (grace de 60s sobre el `expires_in`) y, ante un `401`, invalida el cachÃ© y vuelve a pedirlo una Ãºnica vez.
- **PaginaciÃ³n y bÃºsqueda 100 % en servidor.** Se traduce `page`/`pageSize` a `$top`/`$skip`, la bÃºsqueda a `$filter=contains(Name,'...')`, y se pide `$count=true` para el total real. El frontend solo corta visualmente la pÃ¡gina que recibe.
- **Solo los campos mostrados.** `$select=Id,Name,TypeId,Type` en lugar del registro completo.
- **Lookup resuelto legible.** `$expand=Type` devuelve el tipo de cuenta (p. ej. "Cliente", "Contratista") sin exponer el GUID a cara o cruz.
- **Filtro por tipo de cuenta.** `type` se traduce a `$filter=Type/Name eq '...'`. El `$filter=TypeId eq guid'â€¦'` resulta que no lo acepta la versiÃ³n: `TypeId` viaja como `Edm.Guid` y el parser no reconoce el literal `guid'â€¦'`; filtrar por la propiedad navegacional `Type/Name` sÃ­ resuelve.
- **Transporte separado de la lÃ³gica.** `Integration/CreatioODataClient` hace HTTP/bearer/retry 401/parseo; `Services/CreatioODataService` solo arma queries y DTOs. Quedaron listas para mockear y testear por separado.
- **Errores visibles.** El backend mapea fallos de Creatio a `{ error: { code, message } }` con middleware global; el frontend los muestra en un banner (listado y alta).
- **Config por `.env`/variables de entorno** con un loader propio liviano (sin dependencias extra) y validaciÃ³n al arranque.
- **CORS no necesario en dev** gracias al proxy de Vite; igualmente se habilita en Development por comodidad.

## QuÃ© dejÃ© afuera y quÃ© harÃ­a con mÃ¡s tiempo

- No hay tests automatizados (los servicios estÃ¡n detrÃ¡s de interfaces `ICreatioTokenService` / `ICreatioODataService`, listas para mockear).
- No hay validaciÃ³n de duplicados ni normalizaciÃ³n de nombre al crear cuentas.
- Con mÃ¡s tiempo: `authorization code + PKCE` para usuarios humanos, logs estructurados, contenedor Docker para backend y frontend, CI, reintento con `jitter` para 429/5xx y un health-check dedicado del Identity Service.

## Capturas

![Listado de cuentas](capturas/accounts-list.png)

![BÃºsqueda y filtro por tipo](capturas/accounts-search.png)

![Formulario de alta de cuenta](capturas/company.png)

## Deploys en vivo

- Frontend (Vercel): https://creatio-accounts.vercel.app/
- Backend (Render): https://creatio-accounts.onrender.com/health

Nota: el back puede tardar ~30-60s en el primer arranque (free tier).



| Variable | DescripciÃ³n |
| --- | --- |
| `CREATIO_INSTANCE_URL` | URL base del CRM (`https://<instancia>.creatio.com`). |
| `CREATIO_TOKEN_URL` | Endpoint del token (`POST <url>/connect/token`). |
| `CREATIO_CLIENT_ID` | Client id de la integraciÃ³n OAuth. |
| `CREATIO_CLIENT_SECRET` | Client secret de la integraciÃ³n OAuth. |
| `CREATIO_ODATA_BASEPATH` | Base del servicio OData (default `/0/odata`). |
| `VITE_API_BASE_URL` (opcional) | Base de la API si el frontend no usa el proxy (default `/api`). |

