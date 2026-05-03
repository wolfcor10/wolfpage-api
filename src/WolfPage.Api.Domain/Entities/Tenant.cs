namespace WolfPage.Api.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<PageGenerationRequest> PageGenerationRequests { get; set; } = new List<PageGenerationRequest>();
    public ICollection<Page> Pages { get; set; } = new List<Page>();
}
