using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Notifications;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.IntegrationEvents.SubmissionSlots.Notifications;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.Common.Mappers;

internal static class SubmissionSlotNotificationIntegrationMapper
{
    internal static async Task<SubmissionSlotNotificationRecipientIntegration[]> MapRecipientsAsync(
        SubmissionSlot slot,
        UserManager<Identity> userManager)
    {
        var recipients = slot.GetNotificationRecipients();
        var result = new List<SubmissionSlotNotificationRecipientIntegration>(recipients.Count);

        foreach (var recipient in recipients)
        {
            var user = await userManager.FindByIdAsync(recipient.UserId.ToString());

            result.Add(
                new SubmissionSlotNotificationRecipientIntegration
                {
                    UserId = recipient.UserId,
                    Email = user?.Email,
                    ReminderSchedules = recipient.ReminderSchedules
                        .Select(MapReminderSchedule)
                        .ToArray(),
                    BookingNotificationMode = recipient.BookingNotificationMode is null
                        ? null
                        : MapBookingNotificationMode(recipient.BookingNotificationMode.Value)
                });
        }

        return result.ToArray();
    }

    private static ReminderScheduleIntegration MapReminderSchedule(ReminderSchedule reminderSchedule) =>
        (ReminderScheduleIntegration)(int)reminderSchedule;

    private static BookingNotificationModeIntegration MapBookingNotificationMode(BookingNotificationMode bookingNotificationMode) =>
        (BookingNotificationModeIntegration)(int)bookingNotificationMode;
}
