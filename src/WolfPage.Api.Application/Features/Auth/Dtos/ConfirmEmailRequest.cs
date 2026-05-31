namespace WolfPage.Api.Application.Features.Auth.Dtos;

public class ConfirmEmailRequest
{
    public string Token { get; set; } = default!;
}
