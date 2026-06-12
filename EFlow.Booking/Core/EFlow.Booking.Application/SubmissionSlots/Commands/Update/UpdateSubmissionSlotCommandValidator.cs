using FluentValidation;

namespace EFlow.Booking.Application.SubmissionSlots.Commands.Update;

public class UpdateSubmissionSlotCommandValidator : AbstractValidator<UpdateSubmissionSlotCommand>
{
    public UpdateSubmissionSlotCommandValidator()
    {
        When(
            x => x.Patch.StartTime.HasValue && x.Patch.EndTime.HasValue,
            () =>
            {
                RuleFor(x => x.Patch.StartTime.Value)
                    .Must(BeUtc).WithMessage("Start time must be UTC")
                    .LessThan(x => x.Patch.EndTime.Value).WithMessage("Start time must be before end time");

                RuleFor(x => x.Patch.EndTime.Value)
                    .Must(BeUtc).WithMessage("End time must be UTC");
            });

        When(
            x => x.Patch.StartTime.HasValue && !x.Patch.EndTime.HasValue,
            () =>
            {
                RuleFor(x => x.Patch.StartTime.Value)
                    .Must(BeUtc).WithMessage("Start time must be UTC");
            });

        When(
            x => x.Patch.EndTime.HasValue && !x.Patch.StartTime.HasValue,
            () =>
            {
                RuleFor(x => x.Patch.EndTime.Value)
                    .Must(BeUtc).WithMessage("End time must be UTC");
            });

        When(
            x => x.Patch.MaxStudents.HasValue,
            () =>
            {
                RuleFor(x => x.Patch.MaxStudents.Value)
                    .GreaterThan(0).WithMessage("Maximum students must be greater than 0");
            });

        RuleFor(x => x.Patch.Location.Value)
            .MaximumLength(127).When(x => x.Patch.Location.HasValue && !string.IsNullOrEmpty(x.Patch.Location.Value))
            .WithMessage("Location must not exceed 127 characters");

        RuleFor(x => x.Patch.Comment.Value)
            .MaximumLength(1023).When(x => x.Patch.Comment.HasValue && !string.IsNullOrEmpty(x.Patch.Comment.Value))
            .WithMessage("Comment must not exceed 1023 characters");

        RuleFor(x => x.Patch.AllowedGroupIds.Value)
            .Empty().When(x => x.Patch.AllowAllGroups.HasValue && x.Patch.AllowAllGroups.Value && x.Patch.AllowedGroupIds.HasValue)
            .WithMessage("Allowed group IDs must be empty when slot allows all groups");

        RuleFor(x => x.Patch.AllowedGroupIds.Value)
            .NotEmpty().When(x => x.Patch.AllowAllGroups.HasValue && !x.Patch.AllowAllGroups.Value && x.Patch.AllowedGroupIds.HasValue)
            .WithMessage("Allowed group IDs are required when slot does not allow all groups");
    }

    private static bool BeUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc;
}
