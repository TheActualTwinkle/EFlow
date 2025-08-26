using EFlow.Common.Messaging.Producers;
using EFlow.Messaging.Outbox.MessageProcessing.Factories.Interfaces;
using EFlow.Messaging.Outbox.MessageProcessing.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Messaging.Outbox.MessageProcessing.Factories;

public class OutboxMessageProcessorFactory(IServiceProvider serviceProvider) : IOutboxMessageProcessorFactory
{
    public IOutboxMessageProcessor? Get(Type messageType) =>
        serviceProvider.GetKeyedService<IOutboxMessageProcessor>(messageType);
}