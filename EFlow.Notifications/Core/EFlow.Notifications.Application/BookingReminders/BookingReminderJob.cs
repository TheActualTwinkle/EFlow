using EFlow.Common.IntegrationEvents.Booking.Models;
using EFlow.Common.Infrastructure;
using EFlow.Notifications.Application.Email.Interfaces;
using EFlow.Notifications.Application.Email.Models;
using EFlow.Notifications.Messaging.Booking.Interfaces;
using EFlow.Notifications.Messaging.Booking.Settings;
using EFlow.Notifications.Templates.Notifications.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFlow.Notifications.Application.BookingReminders;

public sealed class BookingReminderJob(
    IBookingClient bookingClient,
    IBookingNotificationTemplateService templateService,
    IEmailNotificationService emailNotificationService,
    ISystemClock systemClock,
    IOptions<BookingReminderSettings> settings,
    ILogger<BookingReminderJob> logger)
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var snapshots = await bookingClient.GetReminderSnapshotAsync(cancellationToken);
        var nowUtc = systemClock.UtcNow;
        var notifySince = nowUtc - settings.Value.PollInterval;
        var sentCount = 0;

        foreach (var snapshot in snapshots)
        {
            foreach (var recipient in snapshot.Recipients)
            {
                var remindAt = GetReminderTimeUtc(snapshot.SubmissionSlot.StartTime, recipient.RemindTime);

                if (remindAt > nowUtc || remindAt <= notifySince)
                    continue;

                var (subject, body) = await templateService.CreateReminderAsync(
                    snapshot.SubmissionSlot,
                    recipient.RemindTime,
                    cancellationToken);

                await emailNotificationService.SendAsync(
                    new NotificationMessage
                    {
                        Subject = subject,
                        Body = body,
                        RecipientEmail = recipient.Email
                    },
                    cancellationToken);

                sentCount++;
            }
        }

        logger.LogInformation("Processed booking reminders. Sent {ReminderCount} emails.", sentCount);
    }

    private static DateTime GetReminderTimeUtc(DateTime slotStartTime, SubmissionRemindTimeModel remindTime) =>
        remindTime switch
        {
            SubmissionRemindTimeModel.TwoWeeks => slotStartTime.AddDays(-14),
            SubmissionRemindTimeModel.OneWeek => slotStartTime.AddDays(-7),
            SubmissionRemindTimeModel.TwoDays => slotStartTime.AddDays(-2),
            SubmissionRemindTimeModel.FourHours => slotStartTime.AddHours(-4),
            _ => throw new ArgumentOutOfRangeException(nameof(remindTime), remindTime, null)
        };
}
