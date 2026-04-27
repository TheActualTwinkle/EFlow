using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Notifications;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots.Admissions;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.SubmissionSlots.Events;
using EFlow.Booking.Domain.SubmissionSlots.NotificationSettings;
using EFlow.Booking.Domain.SubmissionSlots.Rules;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots;

public sealed class SubmissionSlot : Entity
{
    public SubmissionSlotId Id { get; private set; }

    internal SubjectId SubjectId { get; private set; }

    internal TeacherId TeacherId { get; private set; }

    internal DateTime StartTime { get; private set; }

    internal DateTime EndTime { get; private set; }

    internal int MaxStudents { get; private set; }

    internal bool AllowAllGroups { get; private set; }

    internal ICollection<GroupId> AllowedGroupIds { get; private set; }

    internal string? Location { get; private set; }

    internal string? Comment { get; private set; }

    internal ICollection<SubmissionSlotAdmission> Admissions { get; private set; } = [];

    internal ICollection<SubmissionSlotNotificationSettings> NotificationSettings { get; private set; } = [];

    private SubmissionSlot() { }

    private SubmissionSlot(
        SubjectId subjectId,
        TeacherId teacherId,
        DateTime startTime,
        DateTime endTime,
        int maxStudents,
        bool allowAllGroups,
        ICollection<GroupId>? allowedGroupIds = null,
        string? location = null,
        string? comment = null)
    {
        allowedGroupIds ??= [];

        ThrowIfBroken(new StartTimeMustBeBeforeEndTimeRule(startTime, endTime));

        ThrowIfBroken(new MaxStudentsMustBePositiveRule(maxStudents));

        ThrowIfBroken(new AllowedGroupIdsMustNotBeEmptyWhenAllowAllGroupsIsFalseRule(allowAllGroups, allowedGroupIds));

        ThrowIfBroken(new AllowedGroupIdsMustBeEmptyWhenAllowAllGroupsIsTrueRule(allowAllGroups, allowedGroupIds));

        ThrowIfBroken(new AllowedGroupIdsMustNotContainDuplicatesRule(allowedGroupIds));

        Id = new SubmissionSlotId(Guid.CreateVersion7());
        SubjectId = subjectId;
        TeacherId = teacherId;
        StartTime = startTime;
        EndTime = endTime;
        MaxStudents = maxStudents;
        AllowAllGroups = allowAllGroups;
        AllowedGroupIds = allowedGroupIds;
        Location = location;
        Comment = comment?.Trim();
    }

    public SubjectId GetSubjectId() =>
        SubjectId;

    public TeacherId GetTeacherId() =>
        TeacherId;

    public DateTime GetStartTime() =>
        StartTime;

    public DateTime GetEndTime() =>
        EndTime;

    public string? GetLocation() =>
        Location;

    public int GetMaxStudents() =>
        MaxStudents;

    public bool GetAllowAllGroups() =>
        AllowAllGroups;

    public IReadOnlyCollection<GroupId> GetAllowedGroupIds() =>
        AllowedGroupIds.ToArray();

    public string? GetComment() =>
        Comment;
    
    public static SubmissionSlot Create(
        SubjectId subjectId,
        TeacherId teacherId,
        DateTime startTime,
        DateTime endTime,
        int maxStudents,
        bool allowAllGroups,
        DateTime nowUtc,
        ICollection<GroupId>? allowedGroupIds = null,
        string? location = null,
        string? comment = null)
    {
        var slot = new SubmissionSlot(
            subjectId,
            teacherId,
            startTime,
            endTime,
            maxStudents,
            allowAllGroups,
            allowedGroupIds,
            location,
            comment);

        slot.AddDomainEvent(
            new SubmissionSlotCreatedDomainEvent
            {
                SlotId = slot.Id,
                SubjectId = slot.SubjectId,
                TeacherId = slot.TeacherId,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                MaxStudents = slot.MaxStudents,
                Location = slot.Location,
                Comment = slot.Comment,
                AllowAllGroups = slot.AllowAllGroups,
                AllowedGroupIds = slot.AllowedGroupIds,
                CreatedAt = nowUtc
            });

        return slot;
    }

    public SubmissionSlotId Delete()
    {
        AddDomainEvent(
            new SubmissionSlotDeletedDomainEvent
            {
                SlotId = Id,
            });

        return Id;
    }

    public IReadOnlyCollection<SubmissionSlotNotificationRecipient> GetNotificationRecipients() =>
        NotificationSettings
            .Select(settings => new SubmissionSlotNotificationRecipient(
                settings.UserId,
                settings.SubmissionRemindTimes,
                settings.BookingNotificationMode))
            .ToArray();

    // public void Update(SubmissionSlotPatch patch, DateTime utcNow)
    // {
    //     var updatedSlot = patch.ApplyTo(this).Entity;
    //     
    //     // TODO
    //     
    //     AddDomainEvent(new SubmissionSlotUpdatedDomainEvent
    //     {
    //         SlotId = Id,
    //         UpdatedAt = utcNow
    //     });
    // }

    public BookingRecord BookToSlot(
        Student student,
        IEnumerable<BookingRecord> existingBookings,
        DateTime nowUtc)
    {
        existingBookings = existingBookings as ICollection<BookingRecord> ?? existingBookings.ToList();
        
        var admission = Admissions.FirstOrDefault(x => x.StudentId == student.Id);

        ThrowIfBroken(new StudentMustHaveAdmissionToSubmissionSlotRule(admission));
        ThrowIfBroken(new StudentGroupMustBeAllowedToSubmissionSlotRule(AllowAllGroups, AllowedGroupIds, student.GroupId));
        ThrowIfBroken(new SubmissionSlotMustHaveAvailablePlacesRule(MaxStudents, existingBookings.Count()));
        ThrowIfBroken(new StudentMustNotBeBookedToSubmissionSlotTwiceRule(existingBookings, student.Id));

        return BookingRecord.Create(student.Id, Id, nowUtc, nowUtc);
    }

    public void CancelBooking(BookingRecord bookingRecord, DateTime nowUtc) =>
        bookingRecord.Cancel(nowUtc);

    public SubmissionSlotAdmission AddAdmission(StudentId studentId, DateTime nowUtc)
    {
        var existingAdmission = Admissions.FirstOrDefault(admission => admission.StudentId == studentId);

        if (existingAdmission is not null)
            return existingAdmission;

        var admission = SubmissionSlotAdmission.Create(Id, studentId, nowUtc, nowUtc);

        Admissions.Add(admission);

        return admission;
    }

    public void RemoveAdmission(StudentId studentId)
    {
        var existingAdmission = Admissions.FirstOrDefault(admission => admission.StudentId == studentId);

        if (existingAdmission is null)
            return;

        Admissions.Remove(existingAdmission);
    }

    public void UpdateNotificationSettings(
        Guid userId,
        ICollection<SubmissionRemindTime> bookingRecordRemindTime,
        BookingNotificationMode? bookingNotificationMode,
        DateTime nowUtc)
    {
        var existingSettings = NotificationSettings.FirstOrDefault(s => s.UserId == userId);

        if (existingSettings is not null)
            NotificationSettings.Remove(existingSettings);

        var settings = SubmissionSlotNotificationSettings.Create(
            Id,
            userId,
            bookingRecordRemindTime,
            bookingNotificationMode,
            nowUtc,
            nowUtc);

        NotificationSettings.Add(settings);
    }
}
