namespace WolfPage.Api.Application.Auth;

public sealed record GoogleUserInfo(
    string ProviderUserId,
    string Email,
    bool EmailVerified,
    string FullName);
