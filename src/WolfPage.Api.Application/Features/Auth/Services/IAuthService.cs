using WolfPage.Api.Application.Features.Auth.Dtos;

namespace WolfPage.Api.Application.Features.Auth.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<EmailConfirmationResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
    Task<EmailConfirmationResponse> ResendEmailConfirmationAsync(ResendEmailConfirmationRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginWithGoogleAsync(GoogleAuthRequest request, CancellationToken cancellationToken = default);
    Task<CurrentUserDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
