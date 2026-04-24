using System.Collections.Concurrent;
using EFlow.Booking.IntegrationEvents.SubmissionSlots.Notifications;
using EFlow.Notifications.Services.NotificationServices.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFlow.Notifications.Services.NotificationServices;

public sealed class BookingReminderBackgroundService(
    BookingReminderSnapshotClient bookingReminderSnapshotClient,
    IEmailNotificationService notificationService,
    IOptions<BookingReminderOptions> options,
    ILogger<BookingReminderBackgroundService> logger)
    : BackgroundService
{
    private static readonly (ReminderScheduleIntegration Flag, TimeSpan BeforeStart)[] ReminderOffsets =
    [
        (ReminderScheduleIntegration.TwoWeeks, TimeSpan.FromDays(14)),
        (ReminderScheduleIntegration.OneWeek, TimeSpan.FromDays(7)),
        (ReminderScheduleIntegration.TwoDays, TimeSpan.FromDays(2)),
        (ReminderScheduleIntegration.FourHours, TimeSpan.FromHours(4))
    ];

    private readonly ConcurrentDictionary<string, byte> _sentReminderKeys = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = options.Value.PollInterval;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to process booking reminders");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessRemindersAsync(CancellationToken cancellationToken)
    {
        var slots = await bookingReminderSnapshotClient.GetReminderSnapshotAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var activeReminderKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var slot in slots)
        {
            if (slot.SlotStartTime <= now)
                continue;

            foreach (var recipient in slot.Recipients)
            {
                foreach (var (flag, beforeStart) in ReminderOffsets)
                {
                    if (!recipient.ReminderSchedules.Contains(flag))
                        continue;

                    var key = BuildReminderKey(slot.SlotId, recipient.UserId, flag, slot.SlotStartTime);
                    activeReminderKeys.Add(key);

                    if (slot.SlotStartTime - beforeStart > now)
                        continue;

                    if (!_sentReminderKeys.TryAdd(key, 0))
                        continue;

                    try
                    {
                        await notificationService.SendAsync(
                            new NotificationMessage
                            {
                                RecipientUserId = recipient.UserId,
                                RecipientEmail = recipient.Email,
                                Subject = "Reminder: upcoming defense",
                                Body = $"Defense slot {slot.SlotId} starts at {slot.SlotStartTime:O}. Reminder: {flag} before start."
                            },
                            cancellationToken);
                    }
                    catch
                    {
                        _sentReminderKeys.TryRemove(key, out _);
                        throw;
                    }
                }
            }
        }

        CleanupSentReminderKeys(activeReminderKeys);
    }

    private void CleanupSentReminderKeys(HashSet<string> activeReminderKeys)
    {
        foreach (var existingKey in _sentReminderKeys.Keys)
            if (!activeReminderKeys.Contains(existingKey))
                _sentReminderKeys.TryRemove(existingKey, out _);
    }

    private static string BuildReminderKey(Guid slotId, Guid userId, ReminderScheduleIntegration flag, DateTime slotStartTime) =>
        $"{slotId}:{userId}:{flag}:{slotStartTime.Ticks}";
}
