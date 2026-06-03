using EFlow.Booking.Application.Common.Outbox.Interfaces;
using EFlow.Booking.Application.Common.Mappers;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Notifications;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.IntegrationEvents.Booking.BookingRecords;
using EFlow.Common.IntegrationEvents.Booking.Models;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EFlow.Booking.Application.BookingRecords.Notifications;

public sealed class BookingRecordDeletedDomainEventNotificationHandler(
    IUnitOfWork unitOfWork,
    IOutboxMessageFactory outboxMessageFactory,
    UserManager<Identity> userManager,
    ILogger<BookingRecordDeletedDomainEventNotificationHandler> logger)
    : INotificationHandler<BookingRecordDeletedDomainEventNotification>
{
    public async Task Handle(BookingRecordDeletedDomainEventNotification notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var slot = await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetByIdAsync(domainEvent.SlotId, cancellationToken);

        if (slot is null)
        {
            logger.LogError(
                "Failed to create booking cancelled integration event. Submission slot with id {SlotId} not found.",
                domainEvent.SlotId);

            return;
        }

        var student = await unitOfWork
            .GetRepository<IStudentRepository>()
            .GetByIdAsync(domainEvent.StudentId, cancellationToken);

        if (student is null)
        {
            logger.LogError(
                "Failed to create booking cancelled integration event. Student with id {StudentId} not found.",
                domainEvent.StudentId);

            return;
        }

        var submissionSlot = await SubmissionSlotNotificationIntegrationMapper.MapAsync(slot, unitOfWork, cancellationToken);

        if (submissionSlot is null)
        {
            logger.LogError(
                "Failed to create booking cancelled integration event. Related slot model cannot be built for slot {SlotId}.",
                domainEvent.SlotId);

            return;
        }

        var integrationEvent = new BookingCancelledIntegrationEvent
        {
            BookingRecord = new BookingRecordModel
            {
                Id = domainEvent.BookingRecordId.Value,
                StudentFullName = student.GetFullName(),
                SubmissionSlotModel = submissionSlot
            },
            NotificationRecipients = await GetNotificationRecipientsAsync(slot)
        };

        await unitOfWork
            .GetRepository<IOutboxMessageRepository>()
            .CreateAsync(outboxMessageFactory.Create(integrationEvent, domainEvent.CancelledAt), cancellationToken);
    }
    
    private async Task<IEnumerable<NotificationRecipient>> GetNotificationRecipientsAsync(SubmissionSlot slot)
    {
        var teacherId = slot.GetTeacherId();

        var user = await userManager.FindByIdAsync(teacherId.Value.ToString());

        if (user?.Email is null)
            return [];

        var teacherNotificationSettings = slot
            .GetNotificationRecipients()
            .SingleOrDefault(r => r.UserId == teacherId.Value);

        if (teacherNotificationSettings?.BookingNotificationMode is BookingNotificationMode.All or BookingNotificationMode.OnlyCancellation)
            return
            [
                new NotificationRecipient
                {
                    Email = user.Email,
                    UserId = user.Id
                }
            ];

        return [];
    }
}
