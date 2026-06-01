namespace WolfPage.Api.Application.Features.Templates.Dtos;

public class TemplateDto
{
    public string Id { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool Enabled { get; set; }
    public List<TemplateVersionDto> Versions { get; set; } = new();
}

public class TemplateVersionDto
{
    public string Id { get; set; } = default!;
    public int VersionNumber { get; set; }
    public string? Engine { get; set; }
    public bool IsPublished { get; set; }
}
