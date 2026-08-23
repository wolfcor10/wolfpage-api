namespace WolfPage.Api.Application.Storage;

public interface IFileStorage
{
    Task UploadAsync(
        string storagePath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<StoredFileContent?> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    Task DeleteIfExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}

public sealed record StoredFileContent(Stream Content, string ContentType);
