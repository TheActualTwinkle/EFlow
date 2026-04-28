using EFlow.Common.Domain.Entities;

namespace EFlow.Booking.Application.Common.Outbox.Interfaces;

public interface IOutboxMessageFactory
{
    public OutboxMessage Create<TMessage>(TMessage message, DateTime createdAt);
}