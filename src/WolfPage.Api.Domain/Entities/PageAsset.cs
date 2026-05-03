namespace WolfPage.Api.Domain.Entities;

public class PageAsset
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string AssetType { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string StoragePath { get; set; } = default!;
    public string? MimeType { get; set; }
    public DateTime CreatedAt { get; set; }

    public Page Page { get; set; } = default!;
}
