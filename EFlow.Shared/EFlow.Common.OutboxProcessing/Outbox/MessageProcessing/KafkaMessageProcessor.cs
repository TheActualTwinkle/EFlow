using EFlow.Common.Domain.Entities;
using EFlow.Common.Messaging.Producers;
using EFlow.Common.OutboxProcessing.Outbox.MessageProcessing.Interfaces;
using EFlow.Common.OutboxProcessing.TopicResolving;

namespace EFlow.Common.OutboxProcessing.Outbox.MessageProcessing;

/// <summary>
/// Outbox message processor for Kafka messages.
/// <see cref="OutboxMessage" /> will be produced to Kafka.
/// </summary>
public sealed class KafkaMessageProcessor(ICommitLogProducer<Guid, byte[]> producer, ITopicNameResolver topicNameResolver) : IOutboxMessageProcessor
{
    public async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken = new())
    {
        var topicName = topicNameResolver.ResolveTopicName(message.Type);

        if (topicName is null)
            throw new InvalidOperationException($"No topic mapping found for message type {message.Type}");

        await producer.ProduceAsync(
            topicName,
            message.Id,
            message.Payload,
            cancellationToken);
    }
}