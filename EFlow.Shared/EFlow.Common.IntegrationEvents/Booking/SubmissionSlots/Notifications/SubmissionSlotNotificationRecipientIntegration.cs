using MemoryPack;

namespace EFlow.Booking.IntegrationEvents.SubmissionSlots.Notifications;

[MemoryPackable]
public partial record SubmissionSlotNotificationRecipientIntegration
{
    public required Guid UserId { get; init; }

    public string? Email { get; init; }

    public required ReminderScheduleIntegration[] ReminderSchedules { get; init; }

    public BookingNotificationModeIntegration? BookingNotificationMode { get; init; }
}
