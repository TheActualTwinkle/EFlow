using EFlow.Messaging.Outbox.MessageProcessing.Interfaces;

namespace EFlow.Messaging.Outbox.MessageProcessing.Factories.Interfaces;

public interface IOutboxMessageProcessorFactory
{
    public IOutboxMessageProcessor? Get(Type messageType);
}