using FluentValidation;
using WolfPage.Api.Application.Features.Users.Dtos;

namespace WolfPage.Api.Application.Features.Users.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(200);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleForEach(x => x.Roles)
            .NotEmpty()
            .MaximumLength(50);
    }
}
