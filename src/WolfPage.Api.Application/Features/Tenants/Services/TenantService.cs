using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Features.Tenants.Dtos;
using WolfPage.Api.Application.Persistence;
using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Application.Features.Tenants.Services;

public class TenantService : ITenantService
{
    private readonly IAppDbContext _dbContext;

    public TenantService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TenantDto> CreateAsync(CreateTenantDto dto, CancellationToken cancellationToken = default)
    {
        var emailTaken = await _dbContext.Tenants
            .AnyAsync(x => x.Email == dto.Email, cancellationToken);

        if (emailTaken)
            throw new InvalidOperationException($"Ya existe un tenant con el email '{dto.Email}'.");

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(tenant);
    }

    public async Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return tenant is null ? null : Map(tenant);
    }

    public async Task<List<TenantDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
            .AsNoTracking()
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                Email = t.Email,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private static TenantDto Map(Tenant tenant) => new()
    {
        Id = tenant.Id,
        Name = tenant.Name,
        Email = tenant.Email,
        IsActive = tenant.IsActive,
        CreatedAt = tenant.CreatedAt
    };
}
