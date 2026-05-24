using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WolfPage.Api.Application;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Auth.Options;
using WolfPage.Api.Application.Features.Pages.Validators;
using WolfPage.Api.Auth;
using WolfPage.Api.Infrastructure;
using WolfPage.Api.Infrastructure.Persistence;
using WolfPage.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicy = "ConfiguredCors";
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(CreatePageRequestValidator).Assembly);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey ?? string.Empty)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("admin"));
    options.AddPolicy("RequireEditor", policy => policy.RequireRole("admin", "editor"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WolfPage API",
        Version = "v1",
        Description = "API para la plataforma WolfPage. Gestiona workspaces, templates y solicitudes de generacion de paginas."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT usando el esquema Bearer."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.AllowAnyMethod()
            .AllowAnyHeader();

        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins);
    });
});

var app = builder.Build();

var runMigrationsOnStartup = app.Environment.IsDevelopment()
    || builder.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup");
var runAuthSeed = app.Environment.IsDevelopment()
    || builder.Configuration.GetValue<bool>("AuthSeed:Enabled");

if (runMigrationsOnStartup || runAuthSeed)
{
    using var scope = app.Services.CreateScope();

    if (runMigrationsOnStartup)
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    if (runAuthSeed)
    {
        var authSeeder = scope.ServiceProvider.GetRequiredService<AuthDataSeeder>();
        await authSeeder.SeedAsync();
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "WolfPage API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors(CorsPolicy);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();

app.Run();
