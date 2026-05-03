# wolfpage-api

REST API de la plataforma WolfPage. Gestiona tenants, templates y solicitudes de generación de páginas. Las solicitudes son persistidas en SQL Server y publicadas vía RabbitMQ al servicio `wolfpage-generator-worker` para su procesamiento asíncrono.

## Stack

- .NET 10 / ASP.NET Core Web API
- EF Core 9.0.4 + SQL Server
- RabbitMQ.Client 6.8.1
- FluentValidation 11.9.2
- Swashbuckle.AspNetCore 6.6.2 (Swagger UI)

## Estructura de la solución

```
wolfpage-api/
├── src/
│   ├── WolfPage.Api.Domain/          → Entidades + Enums (sin dependencias)
│   ├── WolfPage.Api.Application/     → DTOs, interfaces, servicios, validators
│   ├── WolfPage.Api.Infrastructure/  → AppDbContext, RabbitMqPublisher, DI
│   └── wolfpage-apii.Api/            → Controllers, Middleware, Program.cs
└── wolfpage-api.slnx
```

### Dependencias entre capas

```
Domain ←── Application ←── Infrastructure ←── Api
```

## Arquitectura del flujo

```
Cliente
   │
   ▼
[wolfpage-api] ─── POST /api/pages/generate
   │
   ├─► crea PageGenerationRequest(Pending) en SQL Server
   ├─► publica { RequestId, CorrelationId } en RabbitMQ (cola: site.generate)
   └─► retorna 202 Accepted con { requestId, correlationId, status: "Pending" }
                                         │
                                         ▼
                              [wolfpage-generator-worker]
                                         │
                                         ├─► busca request por id
                                         ├─► actualiza → Processing
                                         ├─► renderiza template
                                         ├─► crea Page(Generated)
                                         └─► actualiza → Completed / Failed

Cliente ─── GET /api/pages/requests/{id} ─── consulta el estado
Cliente ─── GET /api/pages/{id} ──────────── obtiene la página generada
```

> El API y el Worker comparten la misma base de datos SQL Server. Desde este MVP, las migraciones EF Core viven en el API (`WolfPage.Api.Infrastructure`) y el Worker consume el schema existente.

## Endpoints

| Método | Ruta                            | Descripción                                       |
| ------ | ------------------------------- | ------------------------------------------------- |
| GET    | `/api/tenants`                  | Lista todos los tenants                           |
| GET    | `/api/tenants/{id}`             | Obtiene un tenant por id                          |
| POST   | `/api/tenants`                  | Crea un nuevo tenant                              |
| GET    | `/api/templates`                | Lista templates activos con versiones publicadas  |
| GET    | `/api/templates/{id}`           | Obtiene un template y todas sus versiones         |
| POST   | `/api/auth/login`               | Autentica usuario interno y emite JWT             |
| GET    | `/api/auth/me`                  | Devuelve el usuario autenticado                   |
| GET    | `/api/users`                    | Lista usuarios del tenant autenticado             |
| POST   | `/api/users`                    | Crea usuario interno (admin)                      |
| GET    | `/api/users/roles`              | Lista roles disponibles                           |
| GET    | `/api/pages`                    | Lista paginas generadas del tenant autenticado    |
| POST   | `/api/pages/generate`           | Crea solicitud y publica mensaje al Worker        |
| GET    | `/api/pages/requests/{id}`      | Consulta el estado de una solicitud               |
| GET    | `/api/pages/{id}`               | Obtiene una página generada                       |

## Configuración

Editar `src/wolfpage-apii.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\MSSQLSERVER01;Database=WolfPageDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "WolfPage.Api",
    "Audience": "WolfPage.Portal",
    "SecretKey": "wolfpage-dev-secret-key-change-me-please-1234567890",
    "AccessTokenMinutes": 60
  },
  "AuthSeed": {
    "TenantName": "WolfPage Demo",
    "TenantEmail": "admin@wolfpage.local",
    "AdminEmail": "admin@wolfpage.local",
    "AdminPassword": "Admin123!",
    "AdminFullName": "WolfPage Admin"
  },
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "wolfadmin",
    "Password": "wolfadmin",
    "VirtualHost": "/wolfpage"
  }
}
```

## Requisitos previos

1. **SQL Server** corriendo. La BD `WolfPageDb` se crea/aplica con las migraciones del API:
   ```bash
   dotnet ef database update --project src/WolfPage.Api.Infrastructure --startup-project src/wolfpage-apii.Api
   ```

2. **RabbitMQ** corriendo en `localhost:5672` con vhost `/wolfpage` y credenciales `wolfadmin/wolfadmin`.

3. **wolfpage-generator-worker** ejecutándose para consumir los mensajes publicados.

## Ejecutar

```bash
cd src/wolfpage-apii.Api
dotnet run
```

Por defecto abre Swagger UI en:
- HTTP:  http://localhost:5141/swagger
- HTTPS: https://localhost:7050/swagger

## Testing rápido con archivo `.http`

Ver `src/wolfpage-apii.Api/wolfpage-api.http` (compatible con la extensión REST Client de VSCode y con JetBrains Rider).
