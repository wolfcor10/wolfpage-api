namespace WolfPage.Api.Application.Features.Pages.Dtos;

public class CreatePageRequestDto
{
    public Guid WorkspaceId { get; set; }
    public Guid? TemplateVersionId { get; set; }
    public string SelectedTemplateId { get; set; } = "general-business";
    public string PageName { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public Dictionary<string, object> Content { get; set; } = new();
}
