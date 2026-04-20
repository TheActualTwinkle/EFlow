using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Events;

public sealed class SubmissionSlotUpdatedDomainEvent : DomainEvent
{
    public required SubmissionSlotId SlotId { get; init; }
    
    public required DateTime UpdatedAt { get; init; }
}