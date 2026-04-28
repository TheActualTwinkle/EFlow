using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Events;

public sealed class SubmissionSlotDeletedDomainEvent : DomainEvent
{
    public required SubmissionSlotId SlotId { get; init; }
}