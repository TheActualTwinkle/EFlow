using FluentValidation;

namespace EFlow.Booking.Application.Groups.Commands;

public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator() =>
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Group name is required")
            .MaximumLength(127).WithMessage("Group name must not exceed 127 characters");
}