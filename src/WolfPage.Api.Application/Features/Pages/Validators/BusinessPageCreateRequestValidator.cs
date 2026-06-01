using FluentValidation;
using WolfPage.Api.Application.Features.Pages.Dtos;

namespace WolfPage.Api.Application.Features.Pages.Validators;

public class BusinessPageCreateRequestValidator : AbstractValidator<BusinessPageCreateRequest>
{
    public BusinessPageCreateRequestValidator()
    {
        RuleFor(x => x.BusinessName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Category)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.SelectedTemplateId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.HeroTitle)
            .NotEmpty()
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
            .WithMessage("Debes indicar al menos email, telefono o WhatsApp.");

        RuleFor(x => x.Items)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(items => items.Any(item => item.Enabled))
            .WithMessage("Debes agregar al menos un item habilitado.");

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
