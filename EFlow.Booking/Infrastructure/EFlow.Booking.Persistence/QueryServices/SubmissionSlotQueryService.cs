using EFlow.Booking.Contracts.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.Persistence.DatabaseContext;
using EFlow.Booking.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.QueryServices;

public sealed class SubmissionSlotQueryService(ApplicationDbContext context) : ISubmissionSlotQueryService
{
    public async Task<IEnumerable<SubmissionSlotView>> GetAllAsync(CancellationToken cancellationToken = new())
    {
        var slots = await QuerySlots()
            .ToListAsync(cancellationToken);

        return await MapToViewsAsync(slots, cancellationToken);
    }

    public async Task<SubmissionSlotView?> GetByIdAsync(SubmissionSlotId id, CancellationToken cancellationToken = new())
    {
        var slots = await QuerySlots()
            .Where(slot => slot.Id == id)
            .ToListAsync(cancellationToken);

        return (await MapToViewsAsync(slots, cancellationToken)).FirstOrDefault();
    }

    public async Task<IEnumerable<SubmissionSlotView>> GetBySubjectIdAsync(SubjectId subjectId, CancellationToken cancellationToken = new())
    {
        var slots = await QuerySlots()
            .Where(slot => slot.SubjectId == subjectId)
            .ToListAsync(cancellationToken);

        return await MapToViewsAsync(slots, cancellationToken);
    }

    public async Task<IEnumerable<SubmissionSlotView>> GetByTeacherIdAsync(TeacherId teacherId, CancellationToken cancellationToken = new())
    {
        var slots = await QuerySlots()
            .Where(slot => slot.TeacherId == teacherId)
            .ToListAsync(cancellationToken);

        return await MapToViewsAsync(slots, cancellationToken);
    }

    public async Task<IEnumerable<SubmissionSlotView>> GetAvailableSlotsAsync(DateTime fromDate, CancellationToken cancellationToken = new())
    {
        var slots = await QuerySlots()
            .Where(slot => slot.StartTime >= fromDate)
            // TODO: filter out slots that are already fully booked
            .ToListAsync(cancellationToken);

        return await MapToViewsAsync(slots, cancellationToken);
    }

    public async Task<IEnumerable<SubmissionSlotReminderSnapshotView>> GetReminderSnapshotAsync(CancellationToken cancellationToken = new())
    {
        var slots = await QuerySlots()
            .Include(slot => slot.NotificationSettings)
            .ToListAsync(cancellationToken);

        if (slots.Count == 0)
            return [];

        var recipientsBySlotId = slots.ToDictionary(
            slot => slot.Id,
            slot => slot.GetNotificationRecipients());

        var userIds = recipientsBySlotId.Values
            .SelectMany(recipients => recipients.Select(recipient => recipient.UserId))
            .Distinct()
            .ToArray();

        var usersById = await context.Users
            .AsNoTracking()
            .Where(user => userIds.AsEnumerable().Contains(user.Id) && user.Email != null)
            .ToDictionaryAsync(user => user.Id, cancellationToken);

        var slotViewsById = (await MapToViewsAsync(slots, cancellationToken))
            .ToDictionary(slot => new SubmissionSlotId(slot.Id));

        return slots
            .Select(slot =>
            {
                var recipients = recipientsBySlotId[slot.Id]
                    .SelectMany(recipient =>
                    {
                        if (!usersById.TryGetValue(recipient.UserId, out var user) || user.Email is null)
                            return [];

                        return recipient.SubmissionRemindTimes.Select(
                            remindTime => new SubmissionSlotReminderRecipientView
                            {
                                UserId = recipient.UserId,
                                Email = user.Email,
                                RemindTime = remindTime
                            });
                    })
                    .ToArray();

                return new
                {
                    Slot = slot,
                    Recipients = recipients
                };
            })
            .Where(snapshot => snapshot.Recipients.Length > 0 && slotViewsById.ContainsKey(snapshot.Slot.Id))
            .Select(snapshot => new SubmissionSlotReminderSnapshotView
            {
                SubmissionSlot = slotViewsById[snapshot.Slot.Id],
                Recipients = snapshot.Recipients
            })
            .ToArray();
    }

    private IQueryable<SubmissionSlot> QuerySlots() =>
        context.SubmissionSlots
            .AsNoTracking()
            .Include(slot => slot.AllowedGroupIds)
            .Include(slot => slot.Admissions);

    private async Task<IReadOnlyCollection<SubmissionSlotView>> MapToViewsAsync(
        IReadOnlyCollection<SubmissionSlot> slots,
        CancellationToken cancellationToken)
    {
        if (slots.Count == 0)
            return [];

        var subjectIds = slots
            .Select(slot => slot.SubjectId)
            .Distinct()
            .ToArray();

        var teacherIds = slots
            .Select(slot => slot.TeacherId)
            .Distinct()
            .ToArray();

        var allowedGroupIds = slots
            .SelectMany(slot => slot.AllowedGroupIds)
            .Distinct()
            .ToArray();

        var admittedStudentIds = slots
            .SelectMany(slot => slot.Admissions.Select(admission => admission.StudentId))
            .Distinct()
            .ToArray();

        var subjects = await context.Subjects
            .AsNoTracking()
            .Where(subject => subjectIds.AsEnumerable().Contains(subject.Id))
            .ToDictionaryAsync(subject => subject.Id, cancellationToken);

        var teachers = await context.Teachers
            .AsNoTracking()
            .Where(teacher => teacherIds.AsEnumerable().Contains(teacher.Id))
            .ToDictionaryAsync(teacher => teacher.Id, cancellationToken);

        var allowedGroups = await context.Groups
            .AsNoTracking()
            .Where(group => allowedGroupIds.AsEnumerable().Contains(group.Id))
            .ToDictionaryAsync(group => group.Id, cancellationToken);

        var admittedStudents = await context.Students
            .AsNoTracking()
            .Where(student => admittedStudentIds.AsEnumerable().Contains(student.Id))
            .ToDictionaryAsync(student => student.Id, cancellationToken);

        return slots
            .Select(slot =>
            {
                subjects.TryGetValue(slot.SubjectId, out var subject);
                teachers.TryGetValue(slot.TeacherId, out var teacher);

                var slotAllowedGroups = slot.AllowedGroupIds
                    .Where(allowedGroups.ContainsKey)
                    .Select(groupId => allowedGroups[groupId]);

                var slotAdmittedStudents = slot.Admissions
                    .Select(admission => admission.StudentId)
                    .Where(admittedStudents.ContainsKey)
                    .Select(studentId => admittedStudents[studentId]);

                return slot.ToSubmissionSlotView(
                    teacher,
                    subject,
                    slotAllowedGroups,
                    slotAdmittedStudents);
            })
            .ToArray();
    }
}
