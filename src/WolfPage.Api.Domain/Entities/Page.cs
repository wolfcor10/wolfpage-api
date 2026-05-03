using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Domain.Entities;

public class Page
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TemplateVersionId { get; set; }
    public Guid RequestId { get; set; }

    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string RoutePath { get; set; } = default!;
    public string HtmlContent { get; set; } = default!;
    public string? CssContent { get; set; }
    public string? JsContent { get; set; }
    public PageStatus Status { get; set; } = PageStatus.Generated;
    public string? PublishedUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Tenant Tenant { get; set; } = default!;
    public TemplateVersion TemplateVersion { get; set; } = default!;
    public PageGenerationRequest Request { get; set; } = default!;
    public ICollection<PageAsset> Assets { get; set; } = new List<PageAsset>();
    public ICollection<DomainBinding> DomainBindings { get; set; } = new List<DomainBinding>();
}
