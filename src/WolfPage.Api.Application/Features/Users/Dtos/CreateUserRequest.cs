namespace WolfPage.Api.Application.Features.Users.Dtos;

public class CreateUserRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string[] Roles { get; set; } = ["viewer"];
}
