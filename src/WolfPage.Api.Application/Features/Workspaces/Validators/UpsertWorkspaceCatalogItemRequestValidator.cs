using FluentValidation;
using WolfPage.Api.Application.Features.Workspaces.Dtos;

namespace WolfPage.Api.Application.Features.Workspaces.Validators;

public class UpsertWorkspaceCatalogItemRequestValidator : AbstractValidator<UpsertWorkspaceCatalogItemRequest>
{
    private static readonly string[] ValidTypes = ["Product", "Service"];

    public UpsertWorkspaceCatalogItemRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(type => ValidTypes.Any(valid => valid.Equals(type, StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Type debe ser Product o Service.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(800);

        RuleFor(x => x.Category).MaximumLength(100);
        RuleFor(x => x.PriceLabel).MaximumLength(80);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
        RuleFor(x => x.CtaLabel).MaximumLength(80);
        RuleFor(x => x.CtaUrl).MaximumLength(500);
    }
}
