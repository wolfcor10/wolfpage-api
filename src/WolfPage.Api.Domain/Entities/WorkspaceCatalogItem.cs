using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Domain.Entities;

public class WorkspaceCatalogItem
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public CatalogItemType Type { get; set; } = CatalogItemType.Product;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Category { get; set; }
    public string? PriceLabel { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageStoragePath { get; set; }
    public string? CtaLabel { get; set; }
    public string? CtaUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = default!;
    public ICollection<WorkspaceCatalogItemImage> Images { get; set; } = new List<WorkspaceCatalogItemImage>();
}
