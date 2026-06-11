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

    /// <summary>
    /// Indicates whether the submission slot is open for all groups of the subject. If true, the AllowedGroupIds property should be ignored.
    /// </summary>
    internal bool AllowAllGroups { get; private set; }

    /// <summary>
    /// List of group ids that are allowed to book the submission slot. This property is ignored if AllowAllGroups is true.
    /// </summary>
    /// <remarks>This property is empty, if AllowAllGroups set to <c>true</c>.</remarks>
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
        IEnumerable<GroupId> subjectGroupIds,
        TeacherId subjectTeacherId,
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
        
        ThrowIfBroken(new AllowedGroupIdsMustBeWithinSubjectGroupIds(allowedGroupIds, subjectGroupIds));
        
        ThrowIfBroken(new TeacherMustOwnSubjectRule(teacherId, subjectTeacherId));

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

    public IReadOnlyCollection<StudentId> GetAdmittedStudentIds() =>
        Admissions.Select(admission => admission.StudentId).ToArray();
    
    public static SubmissionSlot Create(
        SubjectId subjectId,
        TeacherId teacherId,
        DateTime startTime,
        DateTime endTime,
        int maxStudents,
        bool allowAllGroups,
        DateTime nowUtc,
        IEnumerable<GroupId> subjectGroupIds,
        TeacherId subjectTeacherId,
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
            subjectGroupIds,
            subjectTeacherId,
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

    public SubmissionSlotId Delete(DateTime utcNow)
    {
        AddDomainEvent(
            new SubmissionSlotDeletedDomainEvent
            {
                SlotId = Id,
                Slot = CreateSnapshot(),
                DeletedAt = utcNow
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

    public void Update(
        SubmissionSlotPatch patch,
        IEnumerable<GroupId> subjectGroupIds,
        TeacherId subjectTeacherId,
        IEnumerable<Student> admittedStudents,
        DateTime utcNow)
    {
        var oldSlot = CreateSnapshot();
        
        var context = patch.ApplyInto(this);

        if (!context.HasChanges())
            return;

        _ = new SubmissionSlot(
            SubjectId,
            TeacherId,
            StartTime,
            EndTime,
            MaxStudents,
            AllowAllGroups,
            subjectGroupIds,
            subjectTeacherId,
            AllowedGroupIds,
            Location,
            Comment);

        RemoveAdmissionsOutsideAllowedGroups(admittedStudents);

        AddDomainEvent(new SubmissionSlotUpdatedDomainEvent
        {
            SlotId = Id,
            OldSlot = oldSlot,
            NewSlot = CreateSnapshot(),
            UpdatedAt = utcNow
        });
    }

    private SubmissionSlotSnapshot CreateSnapshot() =>
        new()
        {
            SlotId = Id,
            SubjectId = SubjectId,
            TeacherId = TeacherId,
            StartTime = StartTime,
            EndTime = EndTime,
            MaxStudents = MaxStudents,
            Location = Location,
            Comment = Comment,
            AllowAllGroups = AllowAllGroups,
            AllowedGroupIds = AllowedGroupIds.ToArray()
        };

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

    public void CancelBooking(BookingRecord bookingRecord, DateTime nowUtc)
    {
        bookingRecord.Cancel(nowUtc);

        var studentId = bookingRecord.StudentId.Value;
        
        var existingSettings = NotificationSettings.FirstOrDefault(s => s.UserId == studentId);

        if (existingSettings is null)
            return;

        NotificationSettings.Remove(existingSettings);
    }

    public SubmissionSlotAdmission AddAdmission(StudentId studentId, GroupId studentGroupId)
    {
        ThrowIfBroken(new StudentGroupMustBeAllowedToSubmissionSlotRule(AllowAllGroups, AllowedGroupIds, studentGroupId));
        
        var existingAdmission = Admissions.FirstOrDefault(admission => admission.StudentId == studentId);

        if (existingAdmission is not null)
            return existingAdmission;

        var admission = SubmissionSlotAdmission.Create(Id, studentId);

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
        ICollection<Guid> usersWithoutNotifications,
        ICollection<SubmissionRemindTime> bookingRecordRemindTime,
        BookingNotificationMode? bookingNotificationMode)
    {
        var existingSettings = NotificationSettings.FirstOrDefault(s => s.UserId == userId);

        if (existingSettings is not null)
            NotificationSettings.Remove(existingSettings);

        var settings = SubmissionSlotNotificationSettings.Create(
            Id,
            userId,
            usersWithoutNotifications,
            bookingRecordRemindTime,
            bookingNotificationMode);

        NotificationSettings.Add(settings);
    }

    private void RemoveAdmissionsOutsideAllowedGroups(IEnumerable<Student> admittedStudents)
    {
        if (AllowAllGroups)
            return;

        var allowedGroupIds = AllowedGroupIds.ToHashSet();
        var allowedStudentIds = admittedStudents
            .Where(student => allowedGroupIds.Contains(student.GetGroupId()))
            .Select(student => student.Id)
            .ToHashSet();

        var admissionsToRemove = Admissions
            .Where(admission => !allowedStudentIds.Contains(admission.StudentId))
            .ToArray();

        foreach (var admission in admissionsToRemove)
            Admissions.Remove(admission);
    }
}
