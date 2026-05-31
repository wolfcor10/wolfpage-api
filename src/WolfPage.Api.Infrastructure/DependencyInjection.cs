using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Email;
using WolfPage.Api.Application.Features.Auth.Options;
using WolfPage.Api.Application.Messaging;
using WolfPage.Api.Infrastructure.Auth;
using WolfPage.Api.Infrastructure.Email;
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

        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<PortalOptions>(configuration.GetSection(PortalOptions.SectionName));
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<AzureCommunicationServicesOptions>(
            configuration.GetSection(AzureCommunicationServicesOptions.SectionName));

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<AuthDataSeeder>();

        var emailOptions = configuration
            .GetSection(EmailOptions.SectionName)
            .Get<EmailOptions>() ?? new EmailOptions();

        if (emailOptions.Provider.Equals("AzureCommunicationServices", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IEmailSender, AzureCommunicationEmailSender>();
        else
            services.AddSingleton<IEmailSender, LogEmailSender>();

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }
}
