using EFlow.Domain.Models;

namespace EFlow.Messaging.Outbox.MessageProcessing.Interfaces;

public interface IOutboxMessageProcessor
{
    public Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken = new());
}