using EFlow.Booking.Application.Common.Outbox.Interfaces;
using EFlow.Booking.Application.Common.Mappers;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EFlow.Booking.Application.SubmissionSlots.Notifications;

public sealed class SubmissionSlotUpdatedDomainEventNotificationHandler(
    IUnitOfWork unitOfWork,
    IOutboxMessageFactory outboxMessageFactory,
    UserManager<Identity> userManager,
    ILogger<SubmissionSlotUpdatedDomainEventNotificationHandler> logger)
    : INotificationHandler<SubmissionSlotUpdatedDomainEventNotification>
{
    public async Task Handle(SubmissionSlotUpdatedDomainEventNotification notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var slot = await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetByIdAsync(domainEvent.SlotId, cancellationToken);

        if (slot is null)
        {
            logger.LogError(
                "Failed to create submission slot updated integration event. Submission slot with id {SlotId} not found.",
                domainEvent.SlotId);

            return;
        }

        var oldSubmissionSlot = await SubmissionSlotNotificationIntegrationMapper.MapAsync(
            domainEvent.OldSlot,
            unitOfWork,
            cancellationToken);

        if (oldSubmissionSlot is null)
        {
            logger.LogError(
                "Failed to create submission slot updated integration event. Old slot snapshot cannot be resolved for slot {SlotId}.",
                domainEvent.SlotId);

            return;
        }

        var newSubmissionSlot = await SubmissionSlotNotificationIntegrationMapper.MapAsync(
            domainEvent.NewSlot,
            unitOfWork,
            cancellationToken);

        if (newSubmissionSlot is null)
        {
            logger.LogError(
                "Failed to create submission slot updated integration event. New slot snapshot cannot be resolved for slot {SlotId}.",
                domainEvent.SlotId);

            return;
        }

        var integrationEvent = new SubmissionSlotUpdatedIntegrationEvent
        {
            NewSubmissionSlot = newSubmissionSlot,
            OldSubmissionSlot = oldSubmissionSlot,
            NotificationRecipients = await SubmissionSlotNotificationIntegrationMapper.MapRecipientsAsync(slot, userManager)
        };

        await unitOfWork
            .GetRepository<IOutboxMessageRepository>()
            .CreateAsync(outboxMessageFactory.Create(integrationEvent, domainEvent.UpdatedAt), cancellationToken);
    }
}
