using EFlow.Booking.Messaging.Outbox.MessageProcessing.Interfaces;

namespace EFlow.Booking.Messaging.Outbox.MessageProcessing.Factories.Interfaces;

public interface IOutboxMessageProcessorFactory
{
    public IOutboxMessageProcessor? Get(Type messageType);
}