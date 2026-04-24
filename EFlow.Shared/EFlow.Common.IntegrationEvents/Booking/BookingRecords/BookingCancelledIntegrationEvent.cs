using EFlow.Common.Markers;
using MemoryPack;
using EFlow.Booking.IntegrationEvents.SubmissionSlots.Notifications;

namespace EFlow.Booking.IntegrationEvents.BookingRecords;

[MemoryPackable]
public partial record BookingCancelledIntegrationEvent : IKafkaMessage
{
    public required Guid BookingRecordId { get; init; }

    public required Guid SlotId { get; init; }

    public required Guid StudentId { get; init; }

    public required DateTime SlotStartTime { get; init; }

    public required DateTime SlotEndTime { get; init; }

    public string? Location { get; init; }

    public required DateTime CancelledAt { get; init; }

    public required SubmissionSlotNotificationRecipientIntegration[] Recipients { get; init; }
}
