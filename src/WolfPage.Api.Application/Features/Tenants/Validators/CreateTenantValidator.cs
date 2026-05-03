using FluentValidation;
using WolfPage.Api.Application.Features.Tenants.Dtos;

namespace WolfPage.Api.Application.Features.Tenants.Validators;

public class CreateTenantValidator : AbstractValidator<CreateTenantDto>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);
    }
}
