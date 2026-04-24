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

public sealed class BookingRecordDeletedDomainEventNotificationHandler(
    IUnitOfWork unitOfWork,
    IOutboxMessageFactory outboxMessageFactory,
    UserManager<Identity> userManager)
    : INotificationHandler<BookingRecordDeletedDomainEventNotification>
{
    public async Task Handle(BookingRecordDeletedDomainEventNotification notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var slot = await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetByIdAsync(domainEvent.SlotId, cancellationToken);

        if (slot is null)
            return;

        var integrationEvent = new BookingCancelledIntegrationEvent
        {
            BookingRecordId = domainEvent.BookingRecordId.Value,
            SlotId = domainEvent.SlotId.Value,
            StudentId = domainEvent.StudentId.Value,
            SlotStartTime = slot.GetStartTime(),
            SlotEndTime = slot.GetEndTime(),
            Location = slot.GetLocation(),
            CancelledAt = domainEvent.CancelledAt,
            Recipients = await SubmissionSlotNotificationIntegrationMapper.MapRecipientsAsync(slot, userManager)
        };

        await unitOfWork
            .GetRepository<IOutboxMessageRepository>()
            .CreateAsync(outboxMessageFactory.Create(integrationEvent, domainEvent.CancelledAt), cancellationToken);
    }
}
