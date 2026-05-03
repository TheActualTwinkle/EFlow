using FluentValidation;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public class UpdateNotificationSettingsCommandValidator : AbstractValidator<UpdateNotificationSettingsCommand>
{
    public UpdateNotificationSettingsCommandValidator()
    {
        RuleFor(x => x.SlotId)
            .NotEmpty().WithMessage("Submission slot ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.SubmissionRemindTimes)
            .NotEmpty().WithMessage("Reminder schedules must not be empty");

        RuleFor(x => x.SubmissionRemindTimes)
            .Must(schedules => schedules.Distinct().Count() == schedules.Count)
            .WithMessage("Reminder schedules must not contain duplicates");

        RuleForEach(x => x.SubmissionRemindTimes)
            .IsInEnum()
            .WithMessage("Reminder schedule is invalid");

        RuleFor(x => x.BookingNotificationMode)
            .IsInEnum()
            .When(x => x.BookingNotificationMode.HasValue)
            .WithMessage("Booking notification mode is invalid");
    }
}
