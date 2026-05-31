using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using WolfPage.Api.Application.Auth;
using WolfPage.Api.Application.Features.Auth.Options;

namespace WolfPage.Api.Infrastructure.Auth;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleAuthOptions _options;

    public GoogleTokenValidator(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId))
            throw new InvalidOperationException("Google authentication is not configured.");

        var payload = await GoogleJsonWebSignature.ValidateAsync(
            idToken,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _options.ClientId }
            });

        return new GoogleUserInfo(
            payload.Subject,
            payload.Email,
            payload.EmailVerified,
            payload.Name ?? payload.Email);
    }
}
