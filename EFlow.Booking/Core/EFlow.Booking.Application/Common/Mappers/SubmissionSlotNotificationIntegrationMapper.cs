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
            AllowedGroupNames = slot.GetAllowAllGroups()
                ? []
                : await GetAllowedGroupNamesAsync(unitOfWork, slot.GetAllowedGroupIds(), cancellationToken)
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
            AllowedGroupNames = snapshot.AllowAllGroups
                ? []
                : await GetAllowedGroupNamesAsync(unitOfWork, snapshot.AllowedGroupIds, cancellationToken)
        };
    }

    internal static async Task<IEnumerable<NotificationRecipient>> MapRecipientsAsync(
        SubmissionSlot slot,
        UserManager<Identity> userManager)
    {
        var recipients = slot.GetNotificationRecipients();
        var result = new List<NotificationRecipient>(recipients.Count);

        foreach (var recipient in recipients)
        {
            var user = await userManager.FindByIdAsync(recipient.UserId.ToString());

            if (user?.Email is null)
                continue;
            
            result.Add(
                new NotificationRecipient
                {
                    UserId = recipient.UserId,
                    Email = user.Email
                });
        }

        return result.ToArray();
    }

    internal static async Task<IEnumerable<NotificationRecipient>> MapRecipientsByBookingModeAsync(
        SubmissionSlot slot,
        UserManager<Identity> userManager,
        params BookingNotificationMode[] modes)
    {
        var allowedModes = new HashSet<BookingNotificationMode>(modes);
        var recipients = slot.GetNotificationRecipients();
        var result = new List<NotificationRecipient>(recipients.Count);

        foreach (var recipient in recipients)
        {
            if (recipient.BookingNotificationMode is not { } mode || !allowedModes.Contains(mode))
                continue;

            var user = await userManager.FindByIdAsync(recipient.UserId.ToString());

            if (user?.Email is null)
                continue;

            result.Add(
                new NotificationRecipient
                {
                    UserId = recipient.UserId,
                    Email = user.Email
                });
        }

        return result.ToArray();
    }

    private static async Task<IEnumerable<string>> GetAllowedGroupNamesAsync(
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
            .Select(group => group.GetName())
            .ToArray();
    }
}
