using EFlow.Booking.Domain.Groups.Events;
using EFlow.Booking.Domain.Groups.Rules;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups;

public sealed class Group : Entity, IAggreagateRoot
{
    public GroupId Id { get; }

    internal string Name { get; private set; }

    private Group(string name, IEnumerable<string> existingGroupNames)
    {
        var trimmedName = name.Trim();
        
        ThrowIfBroken(new GroupNameMustBeProperLengthRule(trimmedName));
        ThrowIfBroken(new GroupNameMustUniqueRule(trimmedName, existingGroupNames));

        Id = new GroupId(Guid.CreateVersion7());
        Name = trimmedName;
    }
    
    private Group() { }
    
    public static Group Create(string name, IEnumerable<string> existingGroupNames)
    {
        var group = new Group(name, existingGroupNames);

        group.AddDomainEvent(new GroupCreatedDomainEvent
        {
            GroupId = group.Id
        });
        
        return group;
    }

    public string GetName() =>
        Name;

    public GroupId Delete()
    {
        AddDomainEvent(new GroupDeletedDomainEvent
        {
            GroupId = Id
        });

        return Id;
    }
    
    public void Update(
        GroupUpdatePatch patch,
        DateTime utcNow,
        IEnumerable<string> existingGroupNames)
    {
        var context = patch.ApplyInto(this);
        
        if (!context.HasChanges())
            return;

        _ = new Group(Name, existingGroupNames);

        AddDomainEvent(new GroupUpdatedDomainEvent
        {
            GroupId = Id,
            UpdatedAt = utcNow
        });
    }
}
