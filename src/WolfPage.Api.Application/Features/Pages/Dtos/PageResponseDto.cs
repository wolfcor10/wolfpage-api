namespace WolfPage.Api.Application.Features.Pages.Dtos;

public class PageResponseDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid TemplateVersionId { get; set; }
    public Guid RequestId { get; set; }
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string RoutePath { get; set; } = default!;
    public string HtmlContent { get; set; } = default!;
    public string? CssContent { get; set; }
    public string? JsContent { get; set; }
    public string Status { get; set; } = default!;
    public string? PublishedUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
