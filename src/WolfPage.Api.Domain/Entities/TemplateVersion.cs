namespace WolfPage.Api.Domain.Entities;

public class TemplateVersion
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string? Engine { get; set; }
    public string HtmlTemplate { get; set; } = default!;
    public string? CssTemplate { get; set; }
    public string? JsTemplate { get; set; }
    public string? SchemaJson { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }

    public Template Template { get; set; } = default!;
    public ICollection<PageGenerationRequest> PageGenerationRequests { get; set; } = new List<PageGenerationRequest>();
    public ICollection<Page> Pages { get; set; } = new List<Page>();
}
