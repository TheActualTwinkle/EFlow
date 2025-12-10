using EFlow.Booking.Application.Common.Validators;
using FluentValidation;

namespace EFlow.Booking.Application.Admins.Commands;

public class CreateAdminCommandValidator : AbstractValidator<CreateAdminCommand>
{
    public CreateAdminCommandValidator()
    {
        RuleFor(x => x.UserName).ValidateUsername();
        RuleFor(x => x.Password).ValidatePassword();
    }
}