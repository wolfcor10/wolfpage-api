using WolfPage.Api.Application.Features.Templates.Dtos;

namespace WolfPage.Api.Application.Features.Templates.Services;

public interface ITemplateService
{
    Task<List<TemplateDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TemplateDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
