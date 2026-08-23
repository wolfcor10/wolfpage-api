using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Domain.Entities;

public class Page
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid? TemplateVersionId { get; set; }
    public Guid? RequestId { get; set; }
    public string SelectedTemplateId { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string RoutePath { get; set; } = default!;
    public string? HtmlContent { get; set; }
    public string? CssContent { get; set; }
    public string? JsContent { get; set; }
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
    public string? ContentSnapshotJson { get; set; }
    public string? GeneratedFilePath { get; set; }
    public PageStatus Status { get; set; } = PageStatus.Generated;
    public string? PublishedUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = default!;
    public TemplateVersion? TemplateVersion { get; set; }
    public ICollection<PageGenerationRequest> GenerationRequests { get; set; } = new List<PageGenerationRequest>();
    public ICollection<PageItem> Items { get; set; } = new List<PageItem>();
    public ICollection<PageAsset> Assets { get; set; } = new List<PageAsset>();
    public ICollection<DomainBinding> DomainBindings { get; set; } = new List<DomainBinding>();
}
