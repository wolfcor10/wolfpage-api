namespace WolfPage.Api.Application.Features.Auth.Dtos;

public class JwtTokenResult
{
    public string AccessToken { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
}
