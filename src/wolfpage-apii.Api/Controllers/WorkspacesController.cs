using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WolfPage.Api.Application.Features.Workspaces.Dtos;
using WolfPage.Api.Application.Features.Workspaces.Services;

namespace WolfPage.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspacesController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<WorkspaceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var workspaces = await _workspaceService.GetAllAsync(cancellationToken);
        return Ok(workspaces);
    }

    [HttpGet("{id:guid}", Name = nameof(GetWorkspaceById))]
    [ProducesResponseType(typeof(WorkspaceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkspaceById(Guid id, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceService.GetByIdAsync(id, cancellationToken);
        return workspace is null ? NotFound() : Ok(workspace);
    }

    [HttpPost]
    [ProducesResponseType(typeof(WorkspaceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWorkspaceDto dto,
        CancellationToken cancellationToken)
    {
        var workspace = await _workspaceService.CreateAsync(dto, cancellationToken);
        return CreatedAtRoute(nameof(GetWorkspaceById), new { id = workspace.Id }, workspace);
    }
}
