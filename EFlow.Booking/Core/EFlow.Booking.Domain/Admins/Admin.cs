using EFlow.Booking.Domain.Admins.Events;
using EFlow.Booking.Domain.Common.BusinessRules;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Admins;

public sealed class Admin : Entity, IAggreagateRoot
{
    public AdminId Id { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Identity? Identity { get; private set; }

    private Admin(DateTime createdAt, DateTime utcNow)
    {
        ThrowIfRuleBroken(new CreationTimeMustBeInPast(utcNow, createdAt));
        
        Id = new AdminId(Guid.CreateVersion7());
        CreatedAt = createdAt;
        
        AddDomainEvent(new AdminCreatedDomainEvent
        {
            AdminId = Id,
            CreatedAt = createdAt
        });
    }
    
    public static Admin Create(DateTime createdAt, DateTime utcNow) =>
        new(createdAt, utcNow);

    public void Delete() =>
        AddDomainEvent(new AdminDeletedDomainEvent
        {
            AdminId = Id
        });
}