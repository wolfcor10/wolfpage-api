using FluentValidation;
using WolfPage.Api.Application.Features.Pages.Dtos;

namespace WolfPage.Api.Application.Features.Pages.Validators;

public class CreatePageRequestValidator : AbstractValidator<CreatePageRequestDto>
{
    public CreatePageRequestValidator()
    {
        RuleFor(x => x.WorkspaceId).NotEmpty();
        RuleFor(x => x.TemplateVersionId).NotEmpty();

        RuleFor(x => x.PageName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(150)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("El slug debe ser kebab-case (minúsculas, números y guiones).");
    }
}
