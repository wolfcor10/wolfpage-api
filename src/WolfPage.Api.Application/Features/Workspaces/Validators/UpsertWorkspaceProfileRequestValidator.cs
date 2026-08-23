using FluentValidation;
using WolfPage.Api.Application.Features.Workspaces.Dtos;

namespace WolfPage.Api.Application.Features.Workspaces.Validators;

public class UpsertWorkspaceProfileRequestValidator : AbstractValidator<UpsertWorkspaceProfileRequest>
{
    public UpsertWorkspaceProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.LegalName).MaximumLength(200);
        RuleFor(x => x.Category).MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.LogoUrl).MaximumLength(500);
        RuleFor(x => x.CoverImageUrl).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(80);
        RuleFor(x => x.WhatsApp).MaximumLength(80);
        RuleFor(x => x.Address).MaximumLength(300);
        RuleFor(x => x.OpeningHours).MaximumLength(300);
        RuleFor(x => x.WebsiteUrl).MaximumLength(500);
        RuleFor(x => x.FacebookUrl).MaximumLength(500);
        RuleFor(x => x.InstagramUrl).MaximumLength(500);
        RuleFor(x => x.TiktokUrl).MaximumLength(500);
        RuleFor(x => x.LinkedinUrl).MaximumLength(500);
        RuleFor(x => x.PrimaryColor).MaximumLength(20);
        RuleFor(x => x.SecondaryColor).MaximumLength(20);
        RuleFor(x => x.AccentColor).MaximumLength(20);

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email)
                || !string.IsNullOrWhiteSpace(x.Phone)
                || !string.IsNullOrWhiteSpace(x.WhatsApp))
            .WithMessage("Debes indicar al menos email, telefono o WhatsApp.");
    }
}
