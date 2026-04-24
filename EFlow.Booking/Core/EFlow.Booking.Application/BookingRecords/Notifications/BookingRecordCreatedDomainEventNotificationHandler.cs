using EFlow.Booking.Application.Common.Mappers;
using EFlow.Booking.Application.Common.Outbox.Interfaces;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.IntegrationEvents.BookingRecords;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.BookingRecords.Notifications;

public sealed class BookingRecordCreatedDomainEventNotificationHandler(
    IUnitOfWork unitOfWork,
    IOutboxMessageFactory outboxMessageFactory,
    UserManager<Identity> userManager)
    : INotificationHandler<BookingRecordCreatedDomainEventNotification>
{
    public async Task Handle(BookingRecordCreatedDomainEventNotification notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var slot = await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetByIdAsync(domainEvent.SlotId, cancellationToken);

        if (slot is null)
            return;

        var integrationEvent = new BookingCreatedIntegrationEvent
        {
            BookingRecordId = domainEvent.BookingRecordId.Value,
            SlotId = domainEvent.SlotId.Value,
            StudentId = domainEvent.StudentId.Value,
            SlotStartTime = slot.GetStartTime(),
            SlotEndTime = slot.GetEndTime(),
            Location = slot.GetLocation(),
            Recipients = await SubmissionSlotNotificationIntegrationMapper.MapRecipientsAsync(slot, userManager)
        };

        await unitOfWork
            .GetRepository<IOutboxMessageRepository>()
            .CreateAsync(outboxMessageFactory.Create(integrationEvent, domainEvent.CreatedAt), cancellationToken);
    }
}
