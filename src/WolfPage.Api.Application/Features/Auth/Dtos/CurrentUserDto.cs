namespace WolfPage.Api.Application.Features.Auth.Dtos;

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string[] Roles { get; set; } = [];
}
