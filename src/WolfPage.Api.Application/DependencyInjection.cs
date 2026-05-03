using Microsoft.Extensions.DependencyInjection;
using WolfPage.Api.Application.Features.Auth.Services;
using WolfPage.Api.Application.Features.Pages.Services;
using WolfPage.Api.Application.Features.Templates.Services;
using WolfPage.Api.Application.Features.Tenants.Services;
using WolfPage.Api.Application.Features.Users.Services;

namespace WolfPage.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPageRequestService, PageRequestService>();
        services.AddScoped<IPageQueryService, PageQueryService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
