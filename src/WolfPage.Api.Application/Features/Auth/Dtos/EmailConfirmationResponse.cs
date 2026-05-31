namespace WolfPage.Api.Application.Features.Auth.Dtos;

public class EmailConfirmationResponse
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = default!;
}
