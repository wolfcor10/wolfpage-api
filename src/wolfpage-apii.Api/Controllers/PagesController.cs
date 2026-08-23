using Microsoft.AspNetCore.Mvc;
using WolfPage.Api.Application.Features.Pages.Dtos;
using WolfPage.Api.Application.Features.Pages.Services;

namespace WolfPage.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PagesController : ControllerBase
{
    private readonly IPageRequestService _requestService;
    private readonly IPageQueryService _queryService;

    public PagesController(IPageRequestService requestService, IPageQueryService queryService)
    {
        _requestService = requestService;
        _queryService = queryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PageResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var pages = await _queryService.GetAllAsync(cancellationToken);
        return Ok(pages);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PageResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] BusinessPageCreateRequest request,
        CancellationToken cancellationToken)
    {
        var page = await _requestService.CreateDraftAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPage), new { id = page.Id }, page);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] BusinessPageCreateRequest request,
        CancellationToken cancellationToken)
    {
        var page = await _requestService.UpdateDraftAsync(id, request, cancellationToken);
        return page is null ? NotFound() : Ok(page);
    }

    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(PageRequestResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        var page = await _queryService.GetByIdAsync(id, cancellationToken);
        if (page is null)
            return NotFound();

        var response = await _requestService.PublishAsync(id, cancellationToken);
        return AcceptedAtAction(nameof(GetRequest), new { id = response.RequestId }, response);
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(PageRequestResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate(
        [FromBody] CreatePageRequestDto dto,
        CancellationToken cancellationToken)
    {
        var response = await _requestService.CreateAsync(dto, cancellationToken);
        return AcceptedAtAction(nameof(GetRequest), new { id = response.RequestId }, response);
    }

    [HttpGet("requests/{id:guid}", Name = nameof(GetRequest))]
    [ProducesResponseType(typeof(PageRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRequest(Guid id, CancellationToken cancellationToken)
    {
        var request = await _requestService.GetByIdAsync(id, cancellationToken);
        return request is null ? NotFound() : Ok(request);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPage(Guid id, CancellationToken cancellationToken)
    {
        var page = await _queryService.GetByIdAsync(id, cancellationToken);
        return page is null ? NotFound() : Ok(page);
    }

    [HttpPut("{id:guid}/hero-image")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(typeof(PageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> UploadHeroImage(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { errors = new[] { "Selecciona una imagen de portada." } });

        await using var content = file.OpenReadStream();
        var page = await _requestService.UploadHeroImageAsync(
            id,
            new PageHeroImageUpload(content, file.ContentType, file.Length),
            cancellationToken);

        return page is null ? NotFound() : Ok(page);
    }

    [HttpGet("{id:guid}/hero-image")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHeroImage(Guid id, CancellationToken cancellationToken)
    {
        var image = await _requestService.GetHeroImageAsync(id, cancellationToken);
        return image is null
            ? NotFound()
            : File(image.Content, image.ContentType, enableRangeProcessing: true);
    }

    [HttpDelete("{id:guid}/hero-image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHeroImage(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _requestService.DeleteHeroImageAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
