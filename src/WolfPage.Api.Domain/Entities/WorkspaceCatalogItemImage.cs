namespace WolfPage.Api.Domain.Entities;

public class WorkspaceCatalogItemImage
{
    public Guid Id { get; set; }
    public Guid CatalogItemId { get; set; }
    public string StoragePath { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public WorkspaceCatalogItem CatalogItem { get; set; } = default!;
}
