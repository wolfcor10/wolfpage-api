using Microsoft.Extensions.DependencyInjection;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Auth.Services;
using WolfPage.Api.Application.Features.Pages.Services;
using WolfPage.Api.Application.Features.Templates.Services;
using WolfPage.Api.Application.Features.Users.Services;
using WolfPage.Api.Application.Features.Workspaces.Services;

namespace WolfPage.Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPageRequestService, PageRequestService>();
        services.AddScoped<IPageQueryService, PageQueryService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddScoped<IWorkspaceContentService, WorkspaceContentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWorkspaceAccessService, WorkspaceAccessService>();

        return services;
    }
}
