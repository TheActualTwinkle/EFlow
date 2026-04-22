using EFlow.Booking.Domain.Notifications;
using FluentValidation;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public class UpdateNotificationSettingsCommandValidator : AbstractValidator<UpdateNotificationSettingsCommand>
{
    private const ReminderSchedule AllReminderScheduleValues =
        ReminderSchedule.TwoWeeks |
        ReminderSchedule.OneWeek |
        ReminderSchedule.TwoDays |
        ReminderSchedule.FourHours;

    public UpdateNotificationSettingsCommandValidator()
    {
        RuleFor(x => x.SlotId)
            .NotEmpty().WithMessage("Submission slot ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.ReminderSchedule)
            .Must(value => (value & ~AllReminderScheduleValues) == 0)
            .WithMessage("Reminder schedule is invalid");

        RuleFor(x => x.BookingNotificationMode)
            .IsInEnum()
            .When(x => x.BookingNotificationMode.HasValue)
            .WithMessage("Booking notification mode is invalid");
    }
}
