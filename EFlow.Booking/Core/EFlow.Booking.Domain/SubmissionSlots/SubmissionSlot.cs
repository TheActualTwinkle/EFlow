using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.SubmissionSlots.Events;
using EFlow.Booking.Domain.SubmissionSlots.Rules;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots;

public sealed class SubmissionSlot : Entity
{
    public SubmissionSlotId Id { get; private set; }

    internal SubjectId SubjectId { get; private set; }

    internal DateTime StartTime { get; private set; }

    internal DateTime EndTime { get; private set; }

    internal int MaxStudents { get; private set; }

    internal bool AllowAllGroups { get; private set; }

    internal ICollection<GroupId> AllowedGroupIds { get; private set; }

    internal string? Location { get; private set; }
    
    private SubmissionSlot() { }
    
    private SubmissionSlot(
        SubjectId subjectId,
        DateTime startTime,
        DateTime endTime,
        int maxStudents,
        bool allowAllGroups,
        ICollection<GroupId>? allowedGroupIds = null,
        string? location = null)
    {
        allowedGroupIds ??= []; 
        
        ThrowIfBroken(new StartTimeMustBeBeforeEndTimeRule(startTime, endTime));
        
        ThrowIfBroken(new MaxStudentsMustBePositiveRule(maxStudents));
        
        ThrowIfBroken(new AllowedGroupIdsMustNotBeEmptyWhenAllowAllGroupsIsFalseRule(allowAllGroups, allowedGroupIds));
        
        ThrowIfBroken(new AllowedGroupIdsMustBeEmptyWhenAllowAllGroupsIsTrueRule(allowAllGroups, allowedGroupIds));
        
        ThrowIfBroken(new AllowedGroupIdsMustNotContainDuplicatesRule(allowedGroupIds));
        
        Id = new SubmissionSlotId(Guid.CreateVersion7());
        SubjectId = subjectId;
        StartTime = startTime;
        EndTime = endTime;
        MaxStudents = maxStudents;
        AllowAllGroups = allowAllGroups;
        AllowedGroupIds = allowedGroupIds;
        Location = location;
    }
    
    public static SubmissionSlot Create(
        SubjectId subjectId,
        DateTime startTime,
        DateTime endTime,
        int maxStudents,
        bool allowAllGroups,
        DateTime nowUtc,
        ICollection<GroupId>? allowedGroupIds = null,
        string? location = null)
    {
        var slot = new SubmissionSlot(subjectId, startTime, endTime, maxStudents, allowAllGroups, allowedGroupIds, location);
        
        slot.AddDomainEvent(new SubmissionSlotCreatedDomainEvent
        {
            SlotId = slot.Id,
            SubjectId = slot.SubjectId,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            MaxStudents = slot.MaxStudents,
            Location = slot.Location,
            CreatedAt = nowUtc
        });
        
        return slot;
    }

    public SubmissionSlotId Delete()
    {
        AddDomainEvent(new SubmissionSlotDeletedDomainEvent
        {
            SlotId = Id,
        });

        return Id;
    }

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
    
    public BookingRecord BookToSlot(StudentId studentId, DateTime nowUtc) =>
        BookingRecord.Create(studentId, Id, nowUtc, nowUtc);

    public void CancelBooking(BookingRecord bookingRecordId) =>
        bookingRecordId.Delete();
}