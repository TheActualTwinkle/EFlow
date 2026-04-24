using EFlow.Common.Markers;
using MemoryPack;
using EFlow.Booking.IntegrationEvents.SubmissionSlots.Notifications;

namespace EFlow.Booking.IntegrationEvents.SubmissionSlots;

[MemoryPackable]
public partial record SubmissionSlotUpdatedIntegrationEvent : IKafkaMessage
{
    public required Guid SlotId { get; init; }

    public required Guid SubjectId { get; init; }

    public required Guid TeacherId { get; init; }

    public required DateTime SlotStartTime { get; init; }

    public required DateTime SlotEndTime { get; init; }

    public string? Location { get; init; }

    public required DateTime UpdatedAt { get; init; }

    public required SubmissionSlotNotificationRecipientIntegration[] Recipients { get; init; }
}
