using FluentValidation;
using WolfPage.Api.Application.Features.Auth.Dtos;

namespace WolfPage.Api.Application.Features.Auth.Validators;

public class ResendEmailConfirmationRequestValidator : AbstractValidator<ResendEmailConfirmationRequest>
{
    public ResendEmailConfirmationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);
    }
}
