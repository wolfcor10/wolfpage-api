namespace WolfPage.Api.Application.Features.Workspaces.Dtos;

public class UpsertWorkspaceCatalogItemRequest
{
    public string Type { get; set; } = "Product";
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Category { get; set; }
    public string? PriceLabel { get; set; }
    public string? ImageUrl { get; set; }
    public string? CtaLabel { get; set; }
    public string? CtaUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
