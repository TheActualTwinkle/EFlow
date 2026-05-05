using FluentValidation;

namespace EFlow.Booking.Application.Groups.Commands.Update;

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator() =>
        When(
            x => x.Patch.Name.HasValue,
            () =>
            {
                RuleFor(x => x.Patch.Name.Value)
                    .NotEmpty().WithMessage("Group name cannot be empty")
                    .MaximumLength(127).WithMessage("Group name must not exceed 127 characters");
            });
}
