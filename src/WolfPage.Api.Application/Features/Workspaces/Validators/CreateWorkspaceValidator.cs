using FluentValidation;
using WolfPage.Api.Application.Features.Workspaces.Dtos;
using WolfPage.Api.Domain.Enums;

namespace WolfPage.Api.Application.Features.Workspaces.Validators;

public class CreateWorkspaceValidator : AbstractValidator<CreateWorkspaceDto>
{
    public CreateWorkspaceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.WorkspaceType)
            .NotEmpty()
            .MaximumLength(30)
            .Must(BeValidEnum<WorkspaceType>)
            .WithMessage($"WorkspaceType debe ser uno de: {EnumValues<WorkspaceType>()}.");

        RuleFor(x => x.ProfileType)
            .NotEmpty()
            .MaximumLength(50)
            .Must(BeValidEnum<ProfileType>)
            .WithMessage($"ProfileType debe ser uno de: {EnumValues<ProfileType>()}.");
    }

    private static bool BeValidEnum<TEnum>(string value)
        where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(value, ignoreCase: true, out _);

    private static string EnumValues<TEnum>()
        where TEnum : struct, Enum =>
        string.Join(", ", Enum.GetNames<TEnum>());
}
