using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Interfaces;

namespace EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Factories.Interfaces;

public interface IOutboxMessageProcessorFactory
{
    public IOutboxMessageProcessor? Get(Type messageType);
}