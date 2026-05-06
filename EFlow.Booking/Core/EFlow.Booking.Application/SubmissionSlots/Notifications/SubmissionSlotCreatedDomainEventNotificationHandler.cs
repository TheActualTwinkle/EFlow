using EFlow.Booking.Application.Common.Outbox.Interfaces;
using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.IntegrationEvents.Booking.SubmissionSlots;
using EFlow.Common.Domain.Repositories;
using EFlow.Common.Infrastructure;
using EFlow.Common.IntegrationEvents.Booking.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EFlow.Booking.Application.SubmissionSlots.Notifications;

public sealed class SubmissionSlotCreatedDomainEventNotificationHandler(
    IUnitOfWork unitOfWork,
    IOutboxMessageFactory outboxMessageFactory,
    UserManager<Identity> userManager,
    ILogger<SubmissionSlotCreatedDomainEventNotificationHandler> logger)
    : INotificationHandler<SubmissionSlotCreatedDomainEventNotification>
{
    public async Task Handle(SubmissionSlotCreatedDomainEventNotification notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        var teacher = await unitOfWork
            .GetRepository<ITeacherRepository>()
            .GetByIdAsync(domainEvent.TeacherId, cancellationToken);

        if (teacher is null)
        {
            logger.LogError("Failed to create submission slot created integration event. " +
                            "Teacher with id {TeacherId} not found.", domainEvent.TeacherId);
            
            return;
        }
        
        var subject = await unitOfWork
            .GetRepository<ISubjectRepository>()
            .GetByIdAsync(domainEvent.SubjectId, cancellationToken);
        
        if (subject is null)
        {
            logger.LogError("Failed to create submission slot created integration event. " +
                            "Subject with id {SubjectId} not found.", domainEvent.SubjectId);
            
            return;
        }

        var affectedStudents = await GetNotificationRecipients(
            domainEvent.SubjectId,
            domainEvent.TeacherId,
            domainEvent.AllowAllGroups,
            domainEvent.AllowedGroupIds.ToArray(),
            cancellationToken);
        
        var integrationEvent = new SubmissionSlotCreatedIntegrationEvent
        {
            SubmissionSlot = new SubmissionSlotModel
            {
                Id = domainEvent.Id,
                SubjectName = subject.GetName(),
                TeacherFullName = teacher.GetFullName(),
                StartTime = domainEvent.StartTime,
                EndTime = domainEvent.EndTime,
                Location = domainEvent.Location,
                Comment = domainEvent.Comment,
                MaxStudents = domainEvent.MaxStudents,
                AllowAllGroups = domainEvent.AllowAllGroups,
                AllowedGroups = domainEvent.AllowAllGroups
                    ? []
                    : await GetAllowedGroupsAsync(domainEvent.AllowedGroupIds, cancellationToken)
            },
            NotificationRecipients = userManager.Users
                .Where(u => affectedStudents.Contains(u.Id))
                .Where(u => u.Email != null)
                .Select(u => new NotificationRecipient
                {
                    UserId = u.Id,
                    Email = u.Email!
                })
        };

        var outboxMessageRepository = unitOfWork.GetRepository<IOutboxMessageRepository>();

        await outboxMessageRepository.CreateAsync(
            outboxMessageFactory.Create(integrationEvent, domainEvent.CreatedAt),
            cancellationToken);
    }

    private async Task<IEnumerable<GroupModel>> GetAllowedGroupsAsync(
        IEnumerable<GroupId> allowedGroupIds,
        CancellationToken cancellationToken = new()) =>
        (await unitOfWork
            .GetRepository<IGroupRepository>()
            .GetByIdsAsync(allowedGroupIds, cancellationToken))
        .Select(g => new GroupModel
        {
            Id = g.Id.Value,
            Name = g.GetName()
        });

    private async Task<IEnumerable<Guid>> GetNotificationRecipients(
        SubjectId subjectId,
        TeacherId teacherId,
        bool allowAllGroups,
        ICollection<GroupId> allowedGroupIds,
        CancellationToken cancellationToken = new())
    {
        var subject = await unitOfWork
            .GetRepository<ISubjectRepository>()
            .GetByIdAsync(subjectId, cancellationToken);

        if (subject is null || subject.GetTeacherId() != teacherId)
            return [];

        var targetGroupIds = allowAllGroups
            ? subject.GetGroupIds()
            : subject
                .GetGroupIds()
                .Where(new HashSet<GroupId>(allowedGroupIds).Contains)
                .ToArray();

        if (targetGroupIds.Count == 0)
            return [];

        var students = await unitOfWork
            .GetRepository<IStudentRepository>()
            .GetByGroupIdsAsync(targetGroupIds, cancellationToken);

        return students
            .Select(student => student.Id.Value)
            .Distinct()
            .ToArray();
    }
}
