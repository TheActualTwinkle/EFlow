using FluentValidation;

namespace EFlow.Booking.Application.SubmissionSlots.Commands.Update;

public class UpdateSubmissionSlotCommandValidator : AbstractValidator<UpdateSubmissionSlotCommand>
{
    public UpdateSubmissionSlotCommandValidator()
    {
        When(
            x => x.SubjectId is not null,
            () =>
            {
                RuleFor(x => x.SubjectId)
                    .NotEmpty().WithMessage("Subject ID cannot be empty");
            });

        When(
            x => x.StartTime is not null && x.EndTime is not null,
            () =>
            {
                RuleFor(x => x.StartTime)
                    .LessThan(x => x.EndTime).WithMessage("Start time must be before end time");
            });

        When(
            x => x.MaxStudents is not null,
            () =>
            {
                RuleFor(x => x.MaxStudents)
                    .GreaterThan(0).WithMessage("Maximum students must be greater than 0");
            });

        RuleFor(x => x.Location)
            .MaximumLength(127).When(x => !string.IsNullOrEmpty(x.Location))
            .WithMessage("Location must not exceed 127 characters");

        RuleFor(x => x.Comment)
            .MaximumLength(1023).When(x => !string.IsNullOrEmpty(x.Comment))
            .WithMessage("Comment must not exceed 1023 characters");
    }
}
