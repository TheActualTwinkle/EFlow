using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Notifications;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.SubmissionSlots.Events;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.IntegrationEvents.Booking.Models;
using EFlow.Common.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.Common.Mappers;

internal static class SubmissionSlotNotificationIntegrationMapper
{
    internal static async Task<SubmissionSlotModel?> MapAsync(
        SubmissionSlot slot,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = new())
    {
        var subject = await unitOfWork
            .GetRepository<ISubjectRepository>()
            .GetByIdAsync(slot.GetSubjectId(), cancellationToken);

        var teacher = await unitOfWork
            .GetRepository<ITeacherRepository>()
            .GetByIdAsync(slot.GetTeacherId(), cancellationToken);

        if (subject is null || teacher is null)
            return null;

        return new SubmissionSlotModel
        {
            Id = slot.Id.Value,
            SubjectName = subject.GetName(),
            TeacherFullName = teacher.GetFullName(),
            StartTime = slot.GetStartTime(),
            EndTime = slot.GetEndTime(),
            Location = slot.GetLocation(),
            Comment = slot.GetComment(),
            MaxStudents = slot.GetMaxStudents(),
            AllowAllGroups = slot.GetAllowAllGroups(),
            AllowedGroups = slot.GetAllowAllGroups()
                ? []
                : await GetAllowedGroupsAsync(unitOfWork, slot.GetAllowedGroupIds(), cancellationToken)
        };
    }

    internal static async Task<SubmissionSlotModel?> MapAsync(
        SubmissionSlotSnapshot snapshot,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = new())
    {
        var subject = await unitOfWork
            .GetRepository<ISubjectRepository>()
            .GetByIdAsync(snapshot.SubjectId, cancellationToken);

        var teacher = await unitOfWork
            .GetRepository<ITeacherRepository>()
            .GetByIdAsync(snapshot.TeacherId, cancellationToken);

        if (subject is null || teacher is null)
            return null;

        return new SubmissionSlotModel
        {
            Id = snapshot.SlotId.Value,
            SubjectName = subject.GetName(),
            TeacherFullName = teacher.GetFullName(),
            StartTime = snapshot.StartTime,
            EndTime = snapshot.EndTime,
            Location = snapshot.Location,
            Comment = snapshot.Comment,
            MaxStudents = snapshot.MaxStudents,
            AllowAllGroups = snapshot.AllowAllGroups,
            AllowedGroups = snapshot.AllowAllGroups
                ? []
                : await GetAllowedGroupsAsync(unitOfWork, snapshot.AllowedGroupIds, cancellationToken)
        };
    }

    private static async Task<IEnumerable<GroupModel>> GetAllowedGroupsAsync(
        IUnitOfWork unitOfWork,
        IEnumerable<GroupId> allowedGroupIds,
        CancellationToken cancellationToken)
    {
        var groupIds = allowedGroupIds.ToArray();

        if (groupIds.Length == 0)
            return [];

        return (await unitOfWork
                .GetRepository<IGroupRepository>()
                .GetByIdsAsync(groupIds, cancellationToken))
            .Select(group => new GroupModel
            {
                Id = group.Id.Value,
                Name = group.GetName()
            })
            .ToArray();
    }
}
