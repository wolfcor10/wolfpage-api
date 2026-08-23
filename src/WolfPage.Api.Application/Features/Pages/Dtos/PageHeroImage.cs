namespace WolfPage.Api.Application.Features.Pages.Dtos;

public sealed record PageHeroImageUpload(
    Stream Content,
    string ContentType,
    long Length);

public sealed record PageHeroImageContent(
    Stream Content,
    string ContentType);
