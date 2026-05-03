namespace WolfPage.Api.Application.Features.Templates.Dtos;

public class TemplateDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TemplateVersionDto> Versions { get; set; } = new();
}

public class TemplateVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string? Engine { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
