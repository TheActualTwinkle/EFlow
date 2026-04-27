using EFlow.Booking.Application.Common.Outbox.Interfaces;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.SubmissionSlots.Notifications;

public sealed class SubmissionSlotUpdatedDomainEventNotificationHandler(
    IUnitOfWork unitOfWork,
    IOutboxMessageFactory outboxMessageFactory,
    UserManager<Identity> userManager)
    : INotificationHandler<SubmissionSlotUpdatedDomainEventNotification>
{
    public async Task Handle(SubmissionSlotUpdatedDomainEventNotification notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var slot = await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetByIdAsync(domainEvent.SlotId, cancellationToken);

        if (slot is null)
            return;

        var integrationEvent = new SubmissionSlotUpdatedIntegrationEvent
        {
            NewSubmissionSlot = null!, // TODO
            OldSubmissionSlot = null!,
            NotificationRecipients = [] 
        };

        await unitOfWork
            .GetRepository<IOutboxMessageRepository>()
            .CreateAsync(outboxMessageFactory.Create(integrationEvent, domainEvent.UpdatedAt), cancellationToken);
    }
}