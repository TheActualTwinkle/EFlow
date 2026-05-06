using EFlow.Booking.Domain.Notifications;

namespace EFlow.Booking.Contracts.SubmissionSlots;

public sealed record SubmissionSlotReminderRecipientView
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public required SubmissionRemindTime RemindTime { get; init; }
}
