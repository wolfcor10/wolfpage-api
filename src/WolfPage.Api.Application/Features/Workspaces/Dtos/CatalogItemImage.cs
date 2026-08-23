namespace WolfPage.Api.Application.Features.Workspaces.Dtos;

public sealed record CatalogItemImageUpload(
    Stream Content,
    string ContentType,
    long Length);

public sealed record CatalogItemImageContent(
    Stream Content,
    string ContentType);
