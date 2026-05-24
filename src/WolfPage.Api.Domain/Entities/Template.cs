namespace WolfPage.Api.Domain.Entities;

public class Template
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<TemplateVersion> Versions { get; set; } = new List<TemplateVersion>();
}
