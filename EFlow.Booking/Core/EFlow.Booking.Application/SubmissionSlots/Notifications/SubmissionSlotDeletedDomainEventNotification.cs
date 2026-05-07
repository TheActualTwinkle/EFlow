using EFlow.Booking.Domain.SubmissionSlots.Events;
using EFlow.Common.Infrastructure;

namespace EFlow.Booking.Application.SubmissionSlots.Notifications;

public sealed record SubmissionSlotDeletedDomainEventNotification : IDomainNotification<SubmissionSlotDeletedDomainEvent>
{
    public required SubmissionSlotDeletedDomainEvent DomainEvent { get; init; }
}
