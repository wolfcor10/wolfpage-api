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
}
