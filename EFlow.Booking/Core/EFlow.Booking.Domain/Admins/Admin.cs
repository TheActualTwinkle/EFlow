using EFlow.Booking.Domain.Admins.Events;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Admins;

public sealed class Admin : Entity, IAggreagateRoot
{
    public Guid Id { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Identity? Identity { get; init; }

    private Admin(DateTime createdAt)
    {
        var id = Guid.CreateVersion7();
        
        Id = id;
        CreatedAt = createdAt;
        
        AddDomainEvent(new AdminCreatedDomainEvent
        {
            AdminId = id, 
            CreatedAt = createdAt
        });
    }
    
    public static Admin Create(DateTime createdAt) =>
        new(createdAt);
}