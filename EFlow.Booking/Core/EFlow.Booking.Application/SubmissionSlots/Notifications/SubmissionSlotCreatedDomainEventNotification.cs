using EFlow.Booking.Domain.SubmissionSlots.Events;
using EFlow.Common.Infrastructure;

namespace EFlow.Booking.Application.SubmissionSlots.Notifications;

public sealed record SubmissionSlotCreatedDomainEventNotification : IDomainNotification<SubmissionSlotCreatedDomainEvent>
{
    public required SubmissionSlotCreatedDomainEvent DomainEvent { get; init; }
}
