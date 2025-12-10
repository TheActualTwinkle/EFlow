using EFlow.Booking.Messaging.Outbox.MessageProcessing.Factories.Interfaces;
using EFlow.Booking.Messaging.Outbox.MessageProcessing.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Booking.Messaging.Outbox.MessageProcessing.Factories;

public class OutboxMessageProcessorFactory(IServiceProvider serviceProvider) : IOutboxMessageProcessorFactory
{
    public IOutboxMessageProcessor? Get(Type messageType) =>
        serviceProvider.GetKeyedService<IOutboxMessageProcessor>(messageType);
}