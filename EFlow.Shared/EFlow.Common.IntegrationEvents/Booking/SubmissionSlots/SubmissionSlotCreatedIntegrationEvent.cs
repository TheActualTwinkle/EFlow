using EFlow.Common.IntegrationEvents.Booking.Models;
using EFlow.Common.Markers;
using MemoryPack;

namespace EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;

[MemoryPackable]
public partial record SubmissionSlotCreatedIntegrationEvent : IKafkaMessage
{
    public required SubmissionSlotModel SubmissionSlot { get; init; }
    
    public required IEnumerable<NotificationRecipient> NotificationRecipients { get; init; }
}
