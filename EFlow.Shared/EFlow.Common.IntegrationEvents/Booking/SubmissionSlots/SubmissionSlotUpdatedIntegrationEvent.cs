using EFlow.Common.IntegrationEvents.Booking.Models;
using EFlow.Common.Markers;
using MemoryPack;

namespace EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;

[MemoryPackable]
public partial record SubmissionSlotUpdatedIntegrationEvent : IKafkaMessage
{
    public required SubmissionSlotModel OldSubmissionSlot { get; init; }
    
    public required SubmissionSlotModel NewSubmissionSlot { get; init; }
    
    public required IEnumerable<NotificationRecipient> NotificationRecipients { get; init; }
}
