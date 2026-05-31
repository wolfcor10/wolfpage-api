# wolfpage-api

REST API de la plataforma WolfPage. Gestiona workspaces, templates, usuarios, paginas generadas y solicitudes de generacion. Las solicitudes se persisten en SQL Server y se publican via RabbitMQ al servicio `wolfpage-generator-worker`.

## Stack

- .NET 10 / ASP.NET Core Web API
- EF Core 9.0.4 + SQL Server
- RabbitMQ.Client 6.8.1
- FluentValidation 11.9.2
- Swashbuckle.AspNetCore 6.6.2 (Swagger UI)

## Estructura

```text
wolfpage-api/
|-- src/
|   |-- WolfPage.Api.Domain/          -> Entidades + enums
|   |-- WolfPage.Api.Application/     -> DTOs, interfaces, servicios, validators
|   |-- WolfPage.Api.Infrastructure/  -> AppDbContext, RabbitMQ, auth, DI
|   `-- wolfpage-apii.Api/            -> Controllers, middleware, Program.cs
`-- wolfpage-api.slnx
```

Dependencias:

```text
Domain <- Application <- Infrastructure <- Api
```

## Modelo actual

- `Workspace` es el espacio administrable dentro de WolfPage.
- Un usuario puede pertenecer a varios workspaces mediante `workspace_member`.
- El rol vive en la membresia del workspace, no globalmente en el usuario.
- El JWT identifica al usuario; el workspace activo viaja en el header `X-Workspace-Id`.
- Cada `Page` representa una landing independiente y puede tener su propio `domain_binding`.
- `workspace_type` y `profile_type` se guardan como enums convertidos a string.

## Flujo principal

```text
Cliente
  |
  v
[wolfpage-api] -- POST /api/pages/generate
  |
  |-- crea PageGenerationRequest(Pending)
  |-- publica { RequestId, CorrelationId } en RabbitMQ (cola: site.generate)
  `-- retorna 202 Accepted
                         |
                         v
              [wolfpage-generator-worker]
                         |
                         |-- busca request por id
                         |-- renderiza template
                         |-- crea Page(Generated)
                         `-- actualiza request a Completed / Failed
```

## Endpoints

| Metodo | Ruta                       | Descripcion                                      |
| ------ | -------------------------- | ------------------------------------------------ |
| POST   | `/api/auth/login`          | Autentica usuario interno y emite JWT            |
| POST   | `/api/auth/register`       | Crea usuario, workspace inicial y envia confirmacion |
| POST   | `/api/auth/confirm-email`  | Confirma el correo con token enviado por email   |
| POST   | `/api/auth/resend-confirmation` | Reenvia el enlace de confirmacion          |
| POST   | `/api/auth/google`         | Autentica o registra con Google Identity         |
| GET    | `/api/auth/me`             | Devuelve usuario, workspace activo y membresias  |
| GET    | `/api/workspaces`          | Lista workspaces del usuario autenticado         |
| POST   | `/api/workspaces`          | Crea workspace y asigna admin al creador         |
| GET    | `/api/templates`           | Lista templates activos                          |
| GET    | `/api/templates/{id}`      | Obtiene un template con sus versiones            |
| GET    | `/api/users`               | Lista usuarios del workspace activo              |
| POST   | `/api/users`               | Agrega usuario al workspace activo               |
| GET    | `/api/users/roles`         | Lista roles disponibles                          |
| GET    | `/api/pages`               | Lista paginas del workspace activo               |
| POST   | `/api/pages/generate`      | Crea solicitud y publica mensaje al Worker       |
| GET    | `/api/pages/requests/{id}` | Consulta el estado de una solicitud              |
| GET    | `/api/pages/{id}`          | Obtiene una pagina generada                      |

## Configuracion

`appsettings.json` no contiene connection strings ni secretos de produccion. Para desarrollo local se usa `src/wolfpage-apii.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\MSSQLSERVER01;Database=WolfPageDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "WolfPage.Api",
    "Audience": "WolfPage.Portal",
    "SecretKey": "<dev-secret>",
    "AccessTokenMinutes": 60
  }
}
```

Para Azure App Service, configurar `DefaultConnection` desde **Environment variables > Connection strings** con tipo `SQLAzure`:

```text
Server=tcp:<server>.database.windows.net,1433;Database=<database>;Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Tambien configurar como App Settings los valores sensibles que no deben ir a git, por ejemplo:

```text
Jwt__SecretKey=<production-secret>
RabbitMq__HostName=<host>
RabbitMq__UserName=<user>
RabbitMq__Password=<password>
Cors__AllowedOrigins__0=https://<static-web-app>.azurestaticapps.net
Portal__BaseUrl=https://<static-web-app>.azurestaticapps.net
Email__Provider=AzureCommunicationServices
Email__FromAddress=donotreply@<managed-domain>.azurecomm.net
AzureCommunicationServices__ConnectionString=<acs-connection-string>
GoogleAuth__ClientId=<google-oauth-client-id>
```

Para exigir confirmacion de correo antes del login con password, activar:

```text
Auth__RequireConfirmedEmail=true
```

Para inicializar una base nueva en Azure, activar temporalmente:

```text
Database__RunMigrationsOnStartup=true
AuthSeed__Enabled=true
AuthSeed__WorkspaceName=WolfPage Demo
AuthSeed__WorkspaceEmail=admin@wolfpage.local
AuthSeed__AdminEmail=admin@wolfpage.local
AuthSeed__AdminFullName=WolfPage Admin
```

Despues del primer arranque exitoso, dejar `Database__RunMigrationsOnStartup=false`. `AuthSeed__Enabled` puede quedar activo porque el seed es idempotente, aunque en produccion es mas limpio apagarlo luego de crear el admin inicial.

El portal Angular usa `src/environments/environment.development.ts` para `ng serve` local y `src/environments/environment.ts` para el build productivo. Si cambia el dominio del App Service, actualizar `apiBaseUrl` en el environment productivo del portal.

Para desarrollo local, los secretos se guardan fuera del repo con `dotnet user-secrets`:

```bash
dotnet user-secrets set "Jwt:SecretKey" "<dev-secret>" --project src/wolfpage-apii.Api
dotnet user-secrets set "AuthSeed:AdminPassword" "<dev-admin-password>" --project src/wolfpage-apii.Api
dotnet user-secrets set "RabbitMq:UserName" "<dev-rabbit-user>" --project src/wolfpage-apii.Api
dotnet user-secrets set "RabbitMq:Password" "<dev-rabbit-password>" --project src/wolfpage-apii.Api
dotnet user-secrets set "Email:Provider" "AzureCommunicationServices" --project src/wolfpage-apii.Api
dotnet user-secrets set "Email:FromAddress" "donotreply@<managed-domain>.azurecomm.net" --project src/wolfpage-apii.Api
dotnet user-secrets set "AzureCommunicationServices:ConnectionString" "<acs-connection-string>" --project src/wolfpage-apii.Api
dotnet user-secrets set "GoogleAuth:ClientId" "<google-oauth-client-id>" --project src/wolfpage-apii.Api
```

## Ejecutar

Aplicar migraciones:

```bash
dotnet ef database update --project src/WolfPage.Api.Infrastructure --startup-project src/wolfpage-apii.Api
```

Iniciar API:

```bash
cd src/wolfpage-apii.Api
dotnet run
```

Swagger:

- HTTP: `http://localhost:5141/swagger`
- HTTPS: `https://localhost:7050/swagger`
