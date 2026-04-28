using EFlow.Booking.Domain.SubmissionSlots.Events;
using EFlow.Common.Infrastructure;

namespace EFlow.Booking.Application.SubmissionSlots.Notifications;

public sealed record SubmissionSlotUpdatedDomainEventNotification : IDomainNotification<SubmissionSlotUpdatedDomainEvent>
{
    public required SubmissionSlotUpdatedDomainEvent DomainEvent { get; init; }
}
