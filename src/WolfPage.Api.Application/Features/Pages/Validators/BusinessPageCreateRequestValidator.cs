using FluentValidation;
using WolfPage.Api.Application.Features.Pages.Dtos;

namespace WolfPage.Api.Application.Features.Pages.Validators;

public class BusinessPageCreateRequestValidator : AbstractValidator<BusinessPageCreateRequest>
{
    public BusinessPageCreateRequestValidator()
    {
        RuleFor(x => x.BusinessName)
            .MaximumLength(150)
            .NotEmpty()
            .When(x => !x.UseWorkspaceProfile);

        RuleFor(x => x.Category)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .NotEmpty()
            .When(x => !x.UseWorkspaceProfile);

        RuleFor(x => x.SelectedTemplateId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.HeroTitle)
            .MaximumLength(200);

        RuleFor(x => x.HeroSubtitle)
            .MaximumLength(500);

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Slug)
            .MaximumLength(150)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .When(x => !string.IsNullOrWhiteSpace(x.Slug))
            .WithMessage("El slug debe ser kebab-case (minusculas, numeros y guiones).");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email)
                || !string.IsNullOrWhiteSpace(x.Phone)
                || !string.IsNullOrWhiteSpace(x.WhatsApp))
            .When(x => !x.UseWorkspaceProfile)
            .WithMessage("Debes indicar al menos email, telefono o WhatsApp.");

        RuleFor(x => x)
            .Must(x => x.CatalogItemIds.Any() || x.Items.Any(item => item.Enabled))
            .WithMessage("Debes agregar al menos un item habilitado o seleccionar un item del catalogo.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(150);

            item.RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(800);

            item.RuleFor(x => x.Price)
                .MaximumLength(80);

            item.RuleFor(x => x.ImageUrl)
                .MaximumLength(500);

            item.RuleFor(x => x.Category)
                .MaximumLength(100);
        });
    }
}
