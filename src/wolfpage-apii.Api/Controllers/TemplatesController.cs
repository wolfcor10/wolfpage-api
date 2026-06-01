using Microsoft.AspNetCore.Mvc;
using WolfPage.Api.Application.Features.Templates.Dtos;
using WolfPage.Api.Application.Features.Templates.Services;

namespace WolfPage.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplatesController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    /// <summary>
    /// Lista los templates disponibles para generacion.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var templates = await _templateService.GetAllAsync(cancellationToken);
        return Ok(templates);
    }

    /// <summary>
    /// Obtiene un template por su id.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var template = await _templateService.GetByIdAsync(id, cancellationToken);
        return template is null ? NotFound() : Ok(template);
    }
}
