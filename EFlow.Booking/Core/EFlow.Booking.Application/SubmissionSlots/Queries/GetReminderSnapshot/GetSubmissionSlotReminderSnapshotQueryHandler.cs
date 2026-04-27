using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Notifications;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.IntegrationEvents.Booking.Models;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public sealed class GetSubmissionSlotReminderSnapshotQueryHandler(
    IUnitOfWork unitOfWork,
    UserManager<Identity> userManager)
    : IRequestHandler<GetSubmissionSlotReminderSnapshotQuery, Result<IEnumerable<SubmissionSlotReminderSnapshotDto>>>
{
    public async Task<Result<IEnumerable<SubmissionSlotReminderSnapshotDto>>> Handle(
        GetSubmissionSlotReminderSnapshotQuery request,
        CancellationToken cancellationToken)
    {
        var slots = (await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetAllAsync(cancellationToken))
            .ToList();

        var subjectRepository = unitOfWork.GetRepository<ISubjectRepository>();
        var teacherRepository = unitOfWork.GetRepository<ITeacherRepository>();
        var groupRepository = unitOfWork.GetRepository<IGroupRepository>();
        var groupsById = (await groupRepository
                .GetByIdsAsync(
                    slots.SelectMany(slot => slot.GetAllowedGroupIds()).Distinct(),
                    cancellationToken))
            .ToDictionary(group => group.Id, group => group.GetName());
        
        var result = new List<SubmissionSlotReminderSnapshotDto>();

        foreach (var slot in slots)
        {
            var recipients = slot.GetNotificationRecipients();

            if (recipients.Count == 0)
                continue;

            var mappedRecipients = new List<SubmissionSlotReminderRecipientDto>(recipients.Count);

            foreach (var recipient in recipients)
            {
                var user = await userManager.FindByIdAsync(recipient.UserId.ToString());

                if (user?.Email is null)
                    continue;

                foreach (var remindTime in recipient.SubmissionRemindTimes)
                    mappedRecipients.Add(
                        new SubmissionSlotReminderRecipientDto
                        {
                            UserId = recipient.UserId,
                            Email = user.Email,
                            RemindTime = MapRemindTime(remindTime)
                        });
            }

            if (mappedRecipients.Count == 0)
                continue;

            var subject = await subjectRepository.GetByIdAsync(slot.GetSubjectId(), cancellationToken);
            var teacher = await teacherRepository.GetByIdAsync(slot.GetTeacherId(), cancellationToken);

            if (subject is null || teacher is null)
                continue;

            var allowedGroupNames = slot.GetAllowedGroupIds()
                .Select(groupsById.GetValueOrDefault)
                .OfType<string>()
                .ToArray();

            result.Add(
                new SubmissionSlotReminderSnapshotDto
                {
                    SubmissionSlot = new SubmissionSlotModel
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
                        AllowedGroupNames = allowedGroupNames
                    },
                    Recipients = mappedRecipients.ToArray()
                });
        }

        return Result.Ok<IEnumerable<SubmissionSlotReminderSnapshotDto>>(result);
    }

    private static SubmissionRemindTimeModel MapRemindTime(SubmissionRemindTime remindTime) =>
        remindTime switch
        {
            SubmissionRemindTime.TwoWeeks => SubmissionRemindTimeModel.TwoWeeks,
            SubmissionRemindTime.OneWeek => SubmissionRemindTimeModel.OneWeek,
            SubmissionRemindTime.TwoDays => SubmissionRemindTimeModel.TwoDays,
            SubmissionRemindTime.FourHours => SubmissionRemindTimeModel.FourHours,
            _ => throw new ArgumentOutOfRangeException(nameof(remindTime), remindTime, null)
        };
}
