using EFlow.Booking.Application.Common.Outbox.Interfaces;
using EFlow.Common.Domain.Entities;
using MemoryPack;

namespace EFlow.Booking.Application.Common.Outbox;

public sealed class OutboxMessageFactory : IOutboxMessageFactory
{
    public OutboxMessage Create<TMessage>(TMessage message, DateTime createdAt)
    {
        var type = typeof(TMessage).AssemblyQualifiedName
                   ?? throw new InvalidOperationException($"Unable to resolve type name for {typeof(TMessage).Name}");

        return new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            Type = type,
            Payload = MemoryPackSerializer.Serialize(message),
            CreatedAt = createdAt
        };
    }
}
