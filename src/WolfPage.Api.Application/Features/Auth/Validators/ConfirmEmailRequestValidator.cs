using FluentValidation;
using WolfPage.Api.Application.Features.Auth.Dtos;

namespace WolfPage.Api.Application.Features.Auth.Validators;

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(500);
    }
}
