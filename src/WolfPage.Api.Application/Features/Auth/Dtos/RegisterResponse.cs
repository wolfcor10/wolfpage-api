namespace WolfPage.Api.Application.Features.Auth.Dtos;

public class RegisterResponse
{
    public bool RequiresEmailConfirmation { get; set; }
    public bool EmailConfirmationSent { get; set; }
    public string Message { get; set; } = default!;
    public LoginResponse? Session { get; set; }
}
