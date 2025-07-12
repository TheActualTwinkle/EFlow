using FluentValidation;

namespace EFlow.Application.SubmissionSlots.Commands.Update;

public class UpdateSubmissionSlotCommandValidator : AbstractValidator<UpdateSubmissionSlotCommand>
{
    // TODO: добавить сюда бизнес валидацию о времени, брать время из базы и проверять что оно не пересекается с другими слотами и что время старта раньше чем уже записанное время конца и т.п.
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
    }
}