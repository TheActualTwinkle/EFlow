using EFlow.Booking.Application.Common.Outbox.Interfaces;
using EFlow.Booking.IntegrationEvents.SubmissionSlots;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Notifications;

public sealed class SubmissionSlotCreatedDomainEventNotificationHandler(
    IUnitOfWork unitOfWork,
    IOutboxMessageFactory outboxMessageFactory)
    : INotificationHandler<SubmissionSlotCreatedDomainEventNotification>
{
    public async Task Handle(SubmissionSlotCreatedDomainEventNotification notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var integrationEvent = new SubmissionSlotCreatedIntegrationEvent
        {
            Id = domainEvent.SlotId.Value,
            SubjectId = domainEvent.SubjectId.Value,
            StartTime = domainEvent.StartTime,
            EndTime = domainEvent.EndTime,
            MaxStudents = domainEvent.MaxStudents,
            Location = domainEvent.Location
        };

        var outboxMessageRepository = unitOfWork.GetRepository<IOutboxMessageRepository>();

        await outboxMessageRepository.CreateAsync(
            outboxMessageFactory.Create(integrationEvent, domainEvent.CreatedAt),
            cancellationToken);
    }
}