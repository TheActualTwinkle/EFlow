using EFlow.Booking.IntegrationEvents;
using EFlow.Common.Domain.Entities;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using MediatR;
using MemoryPack;

namespace EFlow.Booking.Application.SubmissionSlots.Notifications;

public sealed class SubmissionSlotCreatedDomainEventNotificationHandler(IUnitOfWork unitOfWork)
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
            new OutboxMessage
            {
                Id = Guid.CreateVersion7(),
                Type = typeof(SubmissionSlotCreatedIntegrationEvent).AssemblyQualifiedName
                       ?? throw new InvalidOperationException($"Unable to resolve type name for {nameof(SubmissionSlotCreatedIntegrationEvent)}"),
                Payload = MemoryPackSerializer.Serialize(integrationEvent),
                CreatedAt = domainEvent.CreatedAt
            },
            cancellationToken);
    }
}
