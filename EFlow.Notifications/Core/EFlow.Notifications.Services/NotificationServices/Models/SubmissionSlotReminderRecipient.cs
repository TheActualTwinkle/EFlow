using EFlow.Booking.IntegrationEvents.SubmissionSlots.Notifications;

namespace EFlow.Notifications.Services.NotificationServices.Models;

public sealed record SubmissionSlotReminderRecipient
{
    public required Guid UserId { get; init; }

    public string? Email { get; init; }

    public required ReminderScheduleIntegration[] ReminderSchedules { get; init; }
}
