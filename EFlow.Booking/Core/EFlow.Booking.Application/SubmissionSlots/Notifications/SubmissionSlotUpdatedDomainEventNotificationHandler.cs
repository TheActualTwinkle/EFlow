using EFlow.Booking.Application.Common.Outbox.Interfaces;
using EFlow.Booking.Application.Common.Mappers;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots.Events;
using EFlow.Booking.Domain.Subjects;
using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using EFlow.Common.IntegrationEvents.Booking.Models;
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
            NotificationRecipients = await GetNotificationRecipients(domainEvent.OldSlot, domainEvent.NewSlot, cancellationToken)
        };

        await unitOfWork
            .GetRepository<IOutboxMessageRepository>()
            .CreateAsync(outboxMessageFactory.Create(integrationEvent, domainEvent.UpdatedAt), cancellationToken);
    }

    private async Task<IEnumerable<NotificationRecipient>> GetNotificationRecipients(
        SubmissionSlotSnapshot oldSlot,
        SubmissionSlotSnapshot newSlot,
        CancellationToken cancellationToken = new())
    {
        var oldTargetGroupIds = await GetTargetGroupIdsAsync(oldSlot, cancellationToken);
        var newTargetGroupIds = await GetTargetGroupIdsAsync(newSlot, cancellationToken);
        
        var targetGroupIds = oldTargetGroupIds
            .Union(newTargetGroupIds)
            .ToArray();

        if (targetGroupIds.Length == 0)
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

    private async Task<IEnumerable<GroupId>> GetTargetGroupIdsAsync(
        SubmissionSlotSnapshot slot,
        CancellationToken cancellationToken)
    {
        if (!slot.AllowAllGroups)
            return slot.AllowedGroupIds;

        var subject = await unitOfWork
            .GetRepository<ISubjectRepository>()
            .GetByIdAsync(slot.SubjectId, cancellationToken);

        return subject?.GetGroupIds() ?? [];
    }
}
