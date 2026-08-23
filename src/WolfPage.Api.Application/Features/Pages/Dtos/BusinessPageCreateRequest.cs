namespace WolfPage.Api.Application.Features.Pages.Dtos;

public class BusinessPageCreateRequest
{
    public Guid? WorkspaceId { get; set; }
    public string? Slug { get; set; }
    public bool UseWorkspaceProfile { get; set; } = true;
    public string? BusinessName { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? HeroTitle { get; set; }
    public string? HeroSubtitle { get; set; }
    public string? HeroImageUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? WhatsApp { get; set; }
    public string? OpeningHours { get; set; }
    public SocialLinksDto SocialLinks { get; set; } = new();
    public string SelectedTemplateId { get; set; } = "general-business";
    public List<Guid> CatalogItemIds { get; set; } = [];
    public List<BusinessItemDto> Items { get; set; } = [];
}

public class BusinessItemDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    public bool Enabled { get; set; } = true;
}

public class SocialLinksDto
{
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? Tiktok { get; set; }
    public string? Linkedin { get; set; }
    public string? Website { get; set; }
}
