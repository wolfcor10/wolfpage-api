using Microsoft.EntityFrameworkCore;
using WolfPage.Api.Application.Features.Templates.Dtos;
using WolfPage.Api.Application.Persistence;

namespace WolfPage.Api.Application.Features.Templates.Services;

public class TemplateService : ITemplateService
{
    private readonly IAppDbContext _dbContext;

    public TemplateService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TemplateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Templates
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(t => new TemplateDto
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                Category = t.Category,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                Versions = t.Versions
                    .Where(v => v.IsPublished)
                    .Select(v => new TemplateVersionDto
                    {
                        Id = v.Id,
                        VersionNumber = v.VersionNumber,
                        Engine = v.Engine,
                        IsPublished = v.IsPublished,
                        CreatedAt = v.CreatedAt
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TemplateDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Templates
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(t => new TemplateDto
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                Category = t.Category,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                Versions = t.Versions
                    .Select(v => new TemplateVersionDto
                    {
                        Id = v.Id,
                        VersionNumber = v.VersionNumber,
                        Engine = v.Engine,
                        IsPublished = v.IsPublished,
                        CreatedAt = v.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
