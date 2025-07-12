using FluentValidation;

namespace EFlow.Application.Subjects.Commands.Update;

public class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectCommandValidator()
    {
        When(
            x => x.Name is not null,
            () =>
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Subject name cannot be empty")
                    .MaximumLength(127).WithMessage("Subject name must not exceed 127 characters");
            });

        When(
            x => x.TeacherId is not null,
            () =>
            {
                RuleFor(x => x.TeacherId)
                    .NotEmpty().WithMessage("Teacher ID cannot be empty");
            });
    }
}