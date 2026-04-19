using EFlow.Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Common.Infrastructure;

public sealed class DomainEventDispatcher(DbContext context) : IDomainEventDispatcher
{
    public async Task DispatchEventsAsync()
    {
        var domainEvents = context.ChangeTracker
            .Entries<Entity>()
            .SelectMany(entry => entry.Entity.DequeueDomainEvents())
            .ToList();

        if (domainEvents.Count == 0)
            return;
    }
}