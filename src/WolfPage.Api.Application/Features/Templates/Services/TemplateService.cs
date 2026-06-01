using WolfPage.Api.Application.Features.Templates.Dtos;

namespace WolfPage.Api.Application.Features.Templates.Services;

public class TemplateService : ITemplateService
{
    public const string GeneralBusinessTemplateId = "general-business";

    public Task<List<TemplateDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<TemplateDto> { CreateGeneralBusinessTemplate() });
    }

    public Task<TemplateDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var template = string.Equals(id, GeneralBusinessTemplateId, StringComparison.OrdinalIgnoreCase)
            ? CreateGeneralBusinessTemplate()
            : null;

        return Task.FromResult(template);
    }

    private static TemplateDto CreateGeneralBusinessTemplate() =>
        new()
        {
            Id = GeneralBusinessTemplateId,
            Code = GeneralBusinessTemplateId,
            Name = "General Business Template",
            Description = "Template general para negocios locales, productos, servicios y paginas corporativas.",
            Category = "business",
            Enabled = true,
            Versions =
            {
                new TemplateVersionDto
                {
                    Id = GeneralBusinessTemplateId,
                    VersionNumber = 1,
                    Engine = "local-file",
                    IsPublished = true
                }
            }
        };
}
