namespace WolfPage.Api.Application.Features.Workspaces.Dtos;

public class WorkspaceCatalogItemDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Type { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Category { get; set; }
    public string? PriceLabel { get; set; }
    public string? ImageUrl { get; set; }
    public bool HasStoredImage { get; set; }
    public List<CatalogItemImageDto> Images { get; set; } = [];
    public string? CtaLabel { get; set; }
    public string? CtaUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CatalogItemImageDto
{
    public Guid Id { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
