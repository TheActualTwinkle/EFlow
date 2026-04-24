using EFlow.Booking.IntegrationEvents.SubmissionSlots.Notifications;

namespace EFlow.Booking.Application.SubmissionSlots;

public sealed record SubmissionSlotReminderRecipientDto
{
    public required Guid UserId { get; init; }

    public string? Email { get; init; }

    public required ReminderScheduleIntegration[] ReminderSchedules { get; init; }
}
