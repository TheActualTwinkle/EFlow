using EFlow.Common.IntegrationEvents.Booking.Models;

namespace EFlow.Booking.Application.SubmissionSlots;

public sealed record SubmissionSlotReminderSnapshotDto
{
    public required SubmissionSlotModel SubmissionSlot { get; init; }

    public required SubmissionSlotReminderRecipientDto[] Recipients { get; init; }
}
