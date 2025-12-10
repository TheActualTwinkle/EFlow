using EFlow.Booking.Domain.Models;

namespace EFlow.Booking.Messaging.Outbox.MessageProcessing.Interfaces;

public interface IOutboxMessageProcessor
{
    public Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken = new());
}