using EFlow.Booking.Domain.Groups.Events;
using EFlow.Booking.Domain.Groups.Rules;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Subjects;
using EFlow.Common.Domain;
using EFlow.Common.Domain.Students;

namespace EFlow.Booking.Domain.Groups;

public sealed class Group : Entity, IAggreagateRoot
{
    internal GroupId Id { get; private set; }

    internal string Name { get; private set; }

    internal ICollection<Student>? Students { get; private set; }

    internal ICollection<Subject>? Subjects { get; private set; }

    internal ICollection<SubmissionSlot>? SubmissionSlots { get; private set; }

    private Group(string name)
    {
        var trimmedName = name.Trim();
        ThrowIfBroken(new GroupNameMustBeProperLengthRule(trimmedName));

        Id = new GroupId(Guid.CreateVersion7());
        Name = trimmedName;
        
        AddDomainEvent(new GroupCreatedDomainEvent
        {
            GroupId = Id
        });
    }
    
    public static Group Create(string name) =>
        new(name);

    public GroupId Delete()
    {
        AddDomainEvent(new GroupDeletedDomainEvent
        {
            GroupId = Id
        });

        return Id;
    }
    
    public void Update(GroupUpdatePatch patch)
    {
        if (patch.NewName is not null)
        {
            var trimmedName = patch.NewName.Trim();
            ThrowIfBroken(new GroupNameMustBeProperLengthRule(trimmedName));
            
            Name = trimmedName;
        }

        AddDomainEvent(new GroupUpdatedDomainEvent
        {
            GroupId = Id,
            UpdatedAt = DateTime.UtcNow
        });
    }
    
    // AddStudent, RemoveStudent, AddSubject, RemoveSubject etc. methods must be added here
}