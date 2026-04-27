using EFlow.Common.Domain.Entities;

namespace EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Interfaces;

public interface IOutboxMessageProcessor
{
    public Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken = new());
}