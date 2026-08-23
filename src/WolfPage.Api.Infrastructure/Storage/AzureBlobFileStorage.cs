using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using WolfPage.Api.Application.Storage;
using WolfPage.Api.Infrastructure.Options;

namespace WolfPage.Api.Infrastructure.Storage;

public sealed class AzureBlobFileStorage : IFileStorage
{
    private readonly BlobContainerClient _container;

    public AzureBlobFileStorage(BlobServiceClient blobServiceClient, IOptions<BlobStorageOptions> options)
    {
        _container = blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
    }

    public async Task UploadAsync(
        string storagePath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await _container.CreateIfNotExistsAsync(
            PublicAccessType.None,
            cancellationToken: cancellationToken);

        var blob = _container.GetBlobClient(storagePath);
        await blob.UploadAsync(
            content,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            },
            cancellationToken);
    }

    public async Task<StoredFileContent?> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container
                .GetBlobClient(storagePath)
                .DownloadStreamingAsync(cancellationToken: cancellationToken);

            return new StoredFileContent(
                response.Value.Content,
                response.Value.Details.ContentType ?? "application/octet-stream");
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return null;
        }
    }

    public async Task DeleteIfExistsAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        await _container
            .GetBlobClient(storagePath)
            .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }
}
