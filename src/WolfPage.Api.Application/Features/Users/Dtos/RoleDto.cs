namespace WolfPage.Api.Application.Features.Users.Dtos;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
}
