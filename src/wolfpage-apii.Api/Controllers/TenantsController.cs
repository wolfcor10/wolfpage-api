using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WolfPage.Api.Application.Features.Tenants.Dtos;
using WolfPage.Api.Application.Features.Tenants.Services;

namespace WolfPage.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireAdmin")]
[Produces("application/json")]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Lista todos los tenants registrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TenantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await _tenantService.GetAllAsync(cancellationToken);
        return Ok(tenants);
    }

    /// <summary>
    /// Obtiene un tenant por su id.
    /// </summary>
    [HttpGet("{id:guid}", Name = nameof(GetTenantById))]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantById(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.GetByIdAsync(id, cancellationToken);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    /// <summary>
    /// Crea un nuevo tenant.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantDto dto,
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantService.CreateAsync(dto, cancellationToken);
        return CreatedAtRoute(nameof(GetTenantById), new { id = tenant.Id }, tenant);
    }
}
