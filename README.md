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

Editar `src/wolfpage-apii.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\MSSQLSERVER01;Database=WolfWareDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "WolfPage.Api",
    "Audience": "WolfPage.Portal",
    "SecretKey": "wolfpage-dev-secret-key-change-me-please-1234567890",
    "AccessTokenMinutes": 60
  },
  "AuthSeed": {
    "WorkspaceName": "WolfPage Demo",
    "WorkspaceEmail": "admin@wolfpage.local",
    "AdminEmail": "admin@wolfpage.local",
    "AdminPassword": "Admin123!",
    "AdminFullName": "WolfPage Admin"
  }
}
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
