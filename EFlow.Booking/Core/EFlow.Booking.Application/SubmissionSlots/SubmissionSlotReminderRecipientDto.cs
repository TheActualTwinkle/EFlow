using EFlow.Common.IntegrationEvents.Booking.Models;

namespace EFlow.Booking.Application.SubmissionSlots;

public sealed record SubmissionSlotReminderRecipientDto
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }

    public required SubmissionRemindTimeModel RemindTime { get; init; }
}
