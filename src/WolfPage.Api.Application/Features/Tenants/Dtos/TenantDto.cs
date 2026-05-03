namespace WolfPage.Api.Application.Features.Tenants.Dtos;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTenantDto
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}
