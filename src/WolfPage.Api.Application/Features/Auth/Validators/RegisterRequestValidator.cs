using FluentValidation;
using WolfPage.Api.Application.Features.Auth.Dtos;

namespace WolfPage.Api.Application.Features.Auth.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(200);

        RuleFor(x => x.WorkspaceName)
            .NotEmpty()
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
