using EFlow.Booking.Domain.Admins.Events;
using EFlow.Booking.Domain.Common.BusinessRules;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Admins;

public sealed class Admin : Entity, IAggreagateRoot
{
    public AdminId Id { get; }

    internal DateTime CreatedAt { get; private set; }

    private Admin() { }
    
    private Admin(DateTime createdAt, DateTime utcNow)
    {
        ThrowIfBroken(new CreationTimeMustBeInPastRule(utcNow, createdAt));

        Id = new AdminId(Guid.CreateVersion7());
        CreatedAt = createdAt;
    }

    public static Admin Create(DateTime createdAt, DateTime utcNow)
    {
        var admin = new Admin(createdAt, utcNow);

        admin.AddDomainEvent(new AdminCreatedDomainEvent
        {
            AdminId = admin.Id,
            CreatedAt = admin.CreatedAt
        });
        
        return admin;
    }

    public void Delete() =>
        AddDomainEvent(new AdminDeletedDomainEvent
        {
            AdminId = Id
        });
}