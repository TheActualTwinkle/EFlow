using EFlow.Booking.Domain.Groups.Events;
using EFlow.Common.Infrastructure;

namespace EFlow.Booking.Application.Groups.Notifications;

public sealed record CacheInvalidationGroupNotification : 
    IDomainNotification<GroupCreatedDomainEvent>, 
    IDomainNotification<GroupDeletedDomainEvent>, 
    IDomainNotification<GroupUpdatedDomainEvent>
{
    GroupCreatedDomainEvent IDomainNotification<GroupCreatedDomainEvent>.DomainEvent { get; init; } = null!;

    GroupDeletedDomainEvent IDomainNotification<GroupDeletedDomainEvent>.DomainEvent { get; init; } = null!;

    GroupUpdatedDomainEvent IDomainNotification<GroupUpdatedDomainEvent>.DomainEvent { get; init; } = null!;
}