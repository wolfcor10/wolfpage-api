using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Domain.Entities;

public class PageGenerationRequest
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TemplateVersionId { get; set; }
    public string CorrelationId { get; set; } = default!;
    public string PageName { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? ContentJson { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public Tenant Tenant { get; set; } = default!;
    public TemplateVersion TemplateVersion { get; set; } = default!;
    public Page? Page { get; set; }
}
