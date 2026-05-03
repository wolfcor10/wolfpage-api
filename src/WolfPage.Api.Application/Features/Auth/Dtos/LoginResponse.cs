namespace WolfPage.Api.Application.Features.Auth.Dtos;

public class LoginResponse
{
    public string AccessToken { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public CurrentUserDto User { get; set; } = default!;
}
