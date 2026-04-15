using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Events;

public sealed class SubmissionSlotCreatedDomainEvent : DomainEvent
{
    public required SubmissionSlotId SlotId { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}