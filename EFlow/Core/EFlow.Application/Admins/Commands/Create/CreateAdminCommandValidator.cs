using EFlow.Application.Common.Validators;
using FluentValidation;

namespace EFlow.Application.Admins.Commands;

public class CreateAdminCommandValidator : AbstractValidator<CreateAdminCommand>
{
    public CreateAdminCommandValidator()
    {
        RuleFor(x => x.UserName).ValidateUsername();
        RuleFor(x => x.Password).ValidatePassword();
    }
}