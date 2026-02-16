using EFlow.Booking.Domain.Admins.Events;
using EFlow.Booking.Domain.Common.BusinessRules;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Admins;

public sealed class Admin : Entity, IAggreagateRoot
{
    internal AdminId Id { get; private set; }

    internal DateTime CreatedAt { get; private set; }

    internal Identity? Identity { get; private set; }

    private Admin(DateTime createdAt, DateTime utcNow)
    {
        ThrowIfBroken(new CreationTimeMustBeInPastRule(utcNow, createdAt));

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