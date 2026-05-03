using FluentValidation;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public class CreateSubmissionSlotCommandValidator : AbstractValidator<CreateSubmissionSlotCommand>
{
    public CreateSubmissionSlotCommandValidator()
    {
        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Subject ID is required");

        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("Teacher ID is required");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required")
            .Must(BeUtc).WithMessage("Start time must be UTC")
            .LessThan(x => x.EndTime).WithMessage("Start time must be before end time");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required")
            .Must(BeUtc).WithMessage("End time must be UTC")
            .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time");

        RuleFor(x => x.MaxStudents)
            .GreaterThan(0).WithMessage("Maximum students must be greater than 0");

        RuleFor(x => x.Location)
            .MaximumLength(127).When(x => !string.IsNullOrEmpty(x.Location))
            .WithMessage("Location must not exceed 127 characters");

        RuleFor(x => x.Comment)
            .MaximumLength(1023).When(x => !string.IsNullOrEmpty(x.Comment))
            .WithMessage("Comment must not exceed 1023 characters");

        RuleFor(x => x.AllowedGroupIds)
            .Empty().When(x => x.AllowAllGroups)
            .WithMessage("Allowed group IDs must be empty when slot allows all groups");

        RuleFor(x => x.AllowedGroupIds)
            .NotEmpty().When(x => !x.AllowAllGroups)
            .WithMessage("Allowed group IDs are required when slot does not allow all groups");
    }

    private static bool BeUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc;
}
