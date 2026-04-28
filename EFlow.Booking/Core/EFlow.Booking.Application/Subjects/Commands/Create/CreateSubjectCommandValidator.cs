using FluentValidation;

namespace EFlow.Booking.Application.Subjects.Commands;

public class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subject name is required")
            .MaximumLength(127).WithMessage("Subject name must not exceed 127 characters");

        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Teacher ID is required");
    }
}