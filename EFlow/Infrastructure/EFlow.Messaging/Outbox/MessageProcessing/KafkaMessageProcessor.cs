using EFlow.Common.Messaging.Producers;
using EFlow.Common.Messaging.Settings;
using EFlow.Domain.Models;
using EFlow.Messaging.Outbox.MessageProcessing.Interfaces;

namespace EFlow.Messaging.Outbox.MessageProcessing;

/// <summary>
/// Outbox message processor for Kafka messages.
/// Processes <see cref="OutboxMessage"/> instances to be produced to Kafka.
/// </summary>
public class KafkaMessageProcessor(ICommitLogProducer<Guid, byte[]> producer) : IOutboxMessageProcessor
{
    public async Task ProcessAsync(OutboxMessage message, CancellationToken cancellationToken = new())
    {
        // TODO: Create TopicNameResolver to resolve topic names based on message type and other metadata.
        await producer.ProduceAsync(
            KafkaConstants.SubmissionSlotCreatedTopic,
            message.Id,
            message.Payload,
            cancellationToken);
    }
}