using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Auth.Options;
using WolfPage.Api.Application.Messaging;
using WolfPage.Api.Infrastructure.Auth;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Infrastructure.Messaging;
using WolfPage.Api.Infrastructure.Options;
using WolfPage.Api.Infrastructure.Persistence;

namespace WolfPage.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<AuthDataSeeder>();

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }
}
