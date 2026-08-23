namespace WolfPage.Api.Infrastructure.Options;

public class BlobStorageOptions
{
    public const string SectionName = "Storage";

    public string? ConnectionString { get; set; }
    public string? ServiceUri { get; set; }
    public string ContainerName { get; set; } = "wolfpage-media";
}
