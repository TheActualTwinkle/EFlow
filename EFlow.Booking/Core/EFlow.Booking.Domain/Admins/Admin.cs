using EFlow.Booking.Domain.Admins.Events;
using EFlow.Booking.Domain.Common.BusinessRules;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Admins;

public sealed class Admin : Entity, IAggreagateRoot
{
    public AdminId Id { get; }

    internal DateTime CreatedAt { get; private set; }

    private Admin() { }

    private Admin(AdminId id, DateTime createdAt, DateTime utcNow)
    {
        ThrowIfBroken(new CreationTimeMustBeInPastRule(utcNow, createdAt));

        Id = id;
        CreatedAt = createdAt;
    }

    public static Admin Create(AdminId id, DateTime createdAt, DateTime utcNow)
    {
        var admin = new Admin(id, createdAt, utcNow);

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
