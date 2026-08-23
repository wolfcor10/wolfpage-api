namespace WolfPage.Api.Application.Features.Pages.Dtos;

public class PageGenerationSnapshotDto
{
    public Guid PageId { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid? TemplateVersionId { get; set; }
    public string SelectedTemplateId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string BusinessName { get; set; } = default!;
    public string? BusinessCategory { get; set; }
    public string BusinessDescription { get; set; } = default!;
    public string? LogoUrl { get; set; }
    public string HeroTitle { get; set; } = default!;
    public string? HeroSubtitle { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? HeroImageStoragePath { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? WhatsApp { get; set; }
    public string? OpeningHours { get; set; }
    public string? SocialLinksJson { get; set; }
    public List<PageGenerationSnapshotItemDto> Items { get; set; } = [];
}

public class PageGenerationSnapshotItemDto
{
    public Guid? SourceCatalogItemId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public bool Enabled { get; set; }
    public int SortOrder { get; set; }
}
