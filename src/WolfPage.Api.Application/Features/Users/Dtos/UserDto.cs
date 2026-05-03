namespace WolfPage.Api.Application.Features.Users.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string[] Roles { get; set; } = [];
}
