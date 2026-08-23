namespace WolfPage.Api.Domain.Entities;

public class PageItem
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public Guid? SourceCatalogItemId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public bool Enabled { get; set; } = true;
    public int SortOrder { get; set; }

    public Page Page { get; set; } = default!;
}
