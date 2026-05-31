using FluentValidation;
using WolfPage.Api.Application.Features.Auth.Dtos;

namespace WolfPage.Api.Application.Features.Auth.Validators;

public class GoogleAuthRequestValidator : AbstractValidator<GoogleAuthRequest>
{
    public GoogleAuthRequestValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty();

        RuleFor(x => x.WorkspaceName)
            .MaximumLength(150);

        RuleFor(x => x.WorkspaceEmail)
            .EmailAddress()
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.WorkspaceEmail));

        RuleFor(x => x.WorkspaceType)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.ProfileType)
            .NotEmpty()
            .MaximumLength(50);
    }
}
