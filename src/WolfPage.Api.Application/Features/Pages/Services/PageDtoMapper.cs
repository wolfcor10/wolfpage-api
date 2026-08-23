using WolfPage.Api.Application.Features.Pages.Dtos;
using WolfPage.Api.Domain.Entities;

namespace WolfPage.Api.Application.Features.Pages.Services;

internal static class PageDtoMapper
{
    public static PageResponseDto Map(Page page) => new()
    {
        Id = page.Id,
        WorkspaceId = page.WorkspaceId,
        TemplateVersionId = page.TemplateVersionId,
        RequestId = page.RequestId,
        SelectedTemplateId = page.SelectedTemplateId,
        Title = page.Title,
        Slug = page.Slug,
        RoutePath = page.RoutePath,
        HtmlContent = page.HtmlContent,
        CssContent = page.CssContent,
        JsContent = page.JsContent,
        BusinessName = page.BusinessName,
        BusinessCategory = page.BusinessCategory,
        BusinessDescription = page.BusinessDescription,
        LogoUrl = page.LogoUrl,
        HeroTitle = page.HeroTitle,
        HeroSubtitle = page.HeroSubtitle,
        HeroImageUrl = page.HeroImageUrl,
        HasStoredHeroImage = !string.IsNullOrWhiteSpace(page.HeroImageStoragePath),
        Phone = page.Phone,
        Email = page.Email,
        Address = page.Address,
        WhatsApp = page.WhatsApp,
        OpeningHours = page.OpeningHours,
        SocialLinksJson = page.SocialLinksJson,
        GeneratedFilePath = page.GeneratedFilePath,
        Status = page.Status.ToString(),
        PublishedUrl = page.PublishedUrl,
        CreatedAt = page.CreatedAt,
        UpdatedAt = page.UpdatedAt,
        Items = page.Items
            .OrderBy(x => x.SortOrder)
            .Select(item => new PageItemDto
            {
                Id = item.Id,
                SourceCatalogItemId = item.SourceCatalogItemId,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                ImageUrl = item.ImageUrl,
                Category = item.Category,
                Enabled = item.Enabled,
                SortOrder = item.SortOrder
            })
            .ToList()
    };
}
