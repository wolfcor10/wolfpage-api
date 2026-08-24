using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WolfPage.Api.Application.Features.Workspaces.Dtos;
using WolfPage.Api.Application.Features.Workspaces.Services;

namespace WolfPage.Api.Controllers;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}")]
[Authorize]
[Produces("application/json")]
public class WorkspaceContentController : ControllerBase
{
    private readonly IWorkspaceContentService _workspaceContentService;

    public WorkspaceContentController(IWorkspaceContentService workspaceContentService)
    {
        _workspaceContentService = workspaceContentService;
    }

    [HttpGet("profile")]
    [ProducesResponseType(typeof(WorkspaceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(Guid workspaceId, CancellationToken cancellationToken)
    {
        var profile = await _workspaceContentService.GetProfileAsync(workspaceId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("profile")]
    [ProducesResponseType(typeof(WorkspaceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertProfile(
        Guid workspaceId,
        [FromBody] UpsertWorkspaceProfileRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await _workspaceContentService.UpsertProfileAsync(workspaceId, request, cancellationToken);
        return Ok(profile);
    }

    [HttpPut("profile/logo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(typeof(WorkspaceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadProfileLogo(
        Guid workspaceId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { errors = new[] { "Selecciona una imagen." } });

        await using var content = file.OpenReadStream();
        var profile = await _workspaceContentService.UploadProfileLogoAsync(
            workspaceId,
            new CatalogItemImageUpload(content, file.ContentType, file.Length),
            cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("profile/logo")]
    public async Task<IActionResult> GetProfileLogo(Guid workspaceId, CancellationToken cancellationToken)
    {
        var image = await _workspaceContentService.GetProfileLogoAsync(workspaceId, cancellationToken);
        return image is null ? NotFound() : File(image.Content, image.ContentType, enableRangeProcessing: true);
    }

    [HttpDelete("profile/logo")]
    public async Task<IActionResult> DeleteProfileLogo(Guid workspaceId, CancellationToken cancellationToken)
    {
        var deleted = await _workspaceContentService.DeleteProfileLogoAsync(workspaceId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPut("profile/cover")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(typeof(WorkspaceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadProfileCover(
        Guid workspaceId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { errors = new[] { "Selecciona una imagen." } });

        await using var content = file.OpenReadStream();
        var profile = await _workspaceContentService.UploadProfileCoverAsync(
            workspaceId,
            new CatalogItemImageUpload(content, file.ContentType, file.Length),
            cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("profile/cover")]
    public async Task<IActionResult> GetProfileCover(Guid workspaceId, CancellationToken cancellationToken)
    {
        var image = await _workspaceContentService.GetProfileCoverAsync(workspaceId, cancellationToken);
        return image is null ? NotFound() : File(image.Content, image.ContentType, enableRangeProcessing: true);
    }

    [HttpDelete("profile/cover")]
    public async Task<IActionResult> DeleteProfileCover(Guid workspaceId, CancellationToken cancellationToken)
    {
        var deleted = await _workspaceContentService.DeleteProfileCoverAsync(workspaceId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("catalog-items")]
    [ProducesResponseType(typeof(List<WorkspaceCatalogItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalogItems(
        Guid workspaceId,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var items = await _workspaceContentService.GetCatalogItemsAsync(
            workspaceId,
            includeInactive,
            cancellationToken);

        return Ok(items);
    }

    [HttpPost("catalog-items")]
    [ProducesResponseType(typeof(WorkspaceCatalogItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCatalogItem(
        Guid workspaceId,
        [FromBody] UpsertWorkspaceCatalogItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _workspaceContentService.CreateCatalogItemAsync(workspaceId, request, cancellationToken);
        return CreatedAtAction(nameof(GetCatalogItems), new { workspaceId }, item);
    }

    [HttpPut("catalog-items/{itemId:guid}")]
    [ProducesResponseType(typeof(WorkspaceCatalogItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCatalogItem(
        Guid workspaceId,
        Guid itemId,
        [FromBody] UpsertWorkspaceCatalogItemRequest request,
        CancellationToken cancellationToken)
    {
        var item = await _workspaceContentService.UpdateCatalogItemAsync(
            workspaceId,
            itemId,
            request,
            cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("catalog-items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCatalogItem(
        Guid workspaceId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var deleted = await _workspaceContentService.DeleteCatalogItemAsync(
            workspaceId,
            itemId,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpPut("catalog-items/{itemId:guid}/image")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(typeof(WorkspaceCatalogItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> UploadCatalogItemImage(
        Guid workspaceId,
        Guid itemId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { errors = new[] { "Selecciona una imagen." } });

        await using var content = file.OpenReadStream();
        var item = await _workspaceContentService.UploadCatalogItemImageAsync(
            workspaceId,
            itemId,
            new CatalogItemImageUpload(content, file.ContentType, file.Length),
            cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("catalog-items/{itemId:guid}/image")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCatalogItemImage(
        Guid workspaceId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var image = await _workspaceContentService.GetCatalogItemImageAsync(
            workspaceId,
            itemId,
            cancellationToken);

        return image is null
            ? NotFound()
            : File(image.Content, image.ContentType, enableRangeProcessing: true);
    }

    [HttpDelete("catalog-items/{itemId:guid}/image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCatalogItemImage(
        Guid workspaceId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var deleted = await _workspaceContentService.DeleteCatalogItemImageAsync(
            workspaceId,
            itemId,
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("catalog-items/{itemId:guid}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(typeof(WorkspaceCatalogItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCatalogItemImage(
        Guid workspaceId,
        Guid itemId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { errors = new[] { "Selecciona una imagen." } });

        await using var content = file.OpenReadStream();
        var item = await _workspaceContentService.AddCatalogItemImageAsync(
            workspaceId,
            itemId,
            new CatalogItemImageUpload(content, file.ContentType, file.Length),
            cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("catalog-items/{itemId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> GetCatalogItemImage(
        Guid workspaceId,
        Guid itemId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var image = await _workspaceContentService.GetCatalogItemImageAsync(
            workspaceId, itemId, imageId, cancellationToken);
        return image is null ? NotFound() : File(image.Content, image.ContentType, enableRangeProcessing: true);
    }

    [HttpPut("catalog-items/{itemId:guid}/images/{imageId:guid}/primary")]
    public async Task<IActionResult> SetPrimaryCatalogItemImage(
        Guid workspaceId,
        Guid itemId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var item = await _workspaceContentService.SetPrimaryCatalogItemImageAsync(
            workspaceId, itemId, imageId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("catalog-items/{itemId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteCatalogItemImage(
        Guid workspaceId,
        Guid itemId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var item = await _workspaceContentService.DeleteCatalogItemImageAsync(
            workspaceId, itemId, imageId, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }
}
