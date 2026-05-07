using EFlow.Booking.Application.Common.Mappers;
using EFlow.Booking.Application.Common.Outbox.Interfaces;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots.Events;
using EFlow.Booking.Domain.Subjects;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using EFlow.Common.IntegrationEvents.Booking.Models;
using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EFlow.Booking.Application.SubmissionSlots.Notifications;

public sealed class SubmissionSlotDeletedDomainEventNotificationHandler(
    IUnitOfWork unitOfWork,
    IOutboxMessageFactory outboxMessageFactory,
    UserManager<Identity> userManager,
    ILogger<SubmissionSlotDeletedDomainEventNotificationHandler> logger)
    : INotificationHandler<SubmissionSlotDeletedDomainEventNotification>
{
    public async Task Handle(SubmissionSlotDeletedDomainEventNotification notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var submissionSlot = await SubmissionSlotNotificationIntegrationMapper.MapAsync(
            domainEvent.Slot,
            unitOfWork,
            cancellationToken);

        if (submissionSlot is null)
        {
            logger.LogError(
                "Failed to create submission slot deleted integration event. Slot snapshot cannot be resolved for slot {SlotId}.",
                domainEvent.SlotId);

            return;
        }

        var integrationEvent = new SubmissionSlotDeletedIntegrationEvent
        {
            SubmissionSlot = submissionSlot,
            NotificationRecipients = await GetNotificationRecipients(domainEvent.Slot, cancellationToken)
        };

        await unitOfWork
            .GetRepository<IOutboxMessageRepository>()
            .CreateAsync(outboxMessageFactory.Create(integrationEvent, domainEvent.DeletedAt), cancellationToken);
    }

    private async Task<IEnumerable<NotificationRecipient>> GetNotificationRecipients(
        SubmissionSlotSnapshot slot,
        CancellationToken cancellationToken = new())
    {
        var targetGroupIds = await GetTargetGroupIdsAsync(slot, cancellationToken);

        if (targetGroupIds.Count == 0)
            return [];

        var students = await unitOfWork
            .GetRepository<IStudentRepository>()
            .GetByGroupIdsAsync(targetGroupIds, cancellationToken);

        var affectedStudentIds = students
            .Select(student => student.Id.Value)
            .ToHashSet();

        return userManager.Users
            .Where(user => affectedStudentIds.Contains(user.Id))
            .Where(user => user.Email != null)
            .Select(user => new NotificationRecipient
            {
                UserId = user.Id,
                Email = user.Email!
            })
            .ToArray();
    }

    private async Task<List<GroupId>> GetTargetGroupIdsAsync(
        SubmissionSlotSnapshot slot,
        CancellationToken cancellationToken)
    {
        if (!slot.AllowAllGroups)
            return slot.AllowedGroupIds.ToList();

        var subject = await unitOfWork
            .GetRepository<ISubjectRepository>()
            .GetByIdAsync(slot.SubjectId, cancellationToken);

        return subject?.GetGroupIds().ToList() ?? [];
    }
}
