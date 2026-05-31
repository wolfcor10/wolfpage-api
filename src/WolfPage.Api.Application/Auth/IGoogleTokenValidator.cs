namespace WolfPage.Api.Application.Auth;

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}
