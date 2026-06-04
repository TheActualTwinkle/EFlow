using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Subjects.Events;
using EFlow.Booking.Domain.Subjects.Rules;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Subjects;

public sealed class Subject : Entity, IAggreagateRoot
{
    public SubjectId Id { get; }

    internal string Name { get; private set; }

    internal TeacherId TeacherId { get; private set; }
    
    internal ICollection<GroupId> GroupIds { get; private set; }
    
    private Subject() { }
    
    private Subject(
        string name,
        TeacherId teacherId,
        ICollection<GroupId> groupIds)
    {
        var trimmedName = name.Trim();
        
        ThrowIfBroken(new SubjectNameMustBeProperLengthRule(trimmedName));
        ThrowIfBroken(new GroupIdsMustNotBeEmpty(groupIds));
        ThrowIfBroken(new GroupIdsMustNotDuplicateRule(groupIds));

        Id = new SubjectId(Guid.CreateVersion7());
        Name = trimmedName;
        TeacherId = teacherId;
        GroupIds = groupIds;
    }

    public string GetName() =>
        Name;

    public TeacherId GetTeacherId() =>
        TeacherId;

    public IReadOnlyCollection<GroupId> GetGroupIds() =>
        GroupIds.ToArray();

    public static Subject Create(
        string name,
        TeacherId teacherId,
        ICollection<GroupId> groupIds)
    {
        var subject = new Subject(name, teacherId, groupIds);
        
        subject.AddDomainEvent(new SubjectCreatedDomainEvent
        {
            SubjectId = subject.Id,
            TeacherId = teacherId,
            GroupIds = groupIds
        });

        return subject;
    }

    public SubjectId Delete()
    {
        AddDomainEvent(new SubjectDeletedDomainEvent
        {
            SubjectId = Id
        });

        return Id;
    }
    
    public void Update(SubjectUpdatePatch patch, DateTime utcNow)
    {
        var context = patch.ApplyInto(this);
        
        if (!context.HasChanges())
            return;

        _ = new Subject(
            Name,
            TeacherId,
            GroupIds);

        AddDomainEvent(new SubjectUpdatedDomainEvent
        {
            SubjectId = Id,
            UpdatedAt = utcNow
        });
    }
}
