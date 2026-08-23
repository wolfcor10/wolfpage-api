using Azure.Identity;
using Azure.Storage.Blobs;
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
using WolfPage.Api.Infrastructure.Storage;
using WolfPage.Api.Application.Storage;

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
        services.Configure<MessagingOptions>(configuration.GetSection(MessagingOptions.SectionName));
        services.Configure<AzureServiceBusOptions>(configuration.GetSection(AzureServiceBusOptions.SectionName));
        services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var options = configuration
                .GetSection(BlobStorageOptions.SectionName)
                .Get<BlobStorageOptions>() ?? new BlobStorageOptions();
            var clientOptions = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2025_11_05);

            if (!string.IsNullOrWhiteSpace(options.ConnectionString))
                return new BlobServiceClient(options.ConnectionString, clientOptions);

            if (Uri.TryCreate(options.ServiceUri, UriKind.Absolute, out var serviceUri))
                return new BlobServiceClient(serviceUri, new DefaultAzureCredential(), clientOptions);

            throw new InvalidOperationException(
                "Configura Storage:ConnectionString para Azurite o Storage:ServiceUri para Azure Blob Storage.");
        });
        services.AddSingleton<IFileStorage, AzureBlobFileStorage>();

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
        var messagingOptions = configuration
            .GetSection(MessagingOptions.SectionName)
            .Get<MessagingOptions>() ?? new MessagingOptions();

        if (messagingOptions.Provider.Equals("AzureServiceBus", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IMessagePublisher, AzureServiceBusPublisher>();
        else
            services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }
}
