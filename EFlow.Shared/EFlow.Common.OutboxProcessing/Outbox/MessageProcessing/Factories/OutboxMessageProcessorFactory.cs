using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Factories.Interfaces;
using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Factories;

public class OutboxMessageProcessorFactory(IServiceProvider serviceProvider) : IOutboxMessageProcessorFactory
{
    public IOutboxMessageProcessor? Get(Type messageType) =>
        serviceProvider.GetKeyedService<IOutboxMessageProcessor>(messageType);
}