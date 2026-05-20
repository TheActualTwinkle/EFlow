using Confluent.Kafka;
using EFlow.Common.Messaging.Serialization;
using Microsoft.Extensions.Logging;

namespace EFlow.Common.Messaging.DeadLetter;

public sealed class DeadLetterQueueRetryJob(
    ProducerConfig producerConfig,
    ILogger<DeadLetterQueueRetryJob> logger)
{
    public const string QueueName = "eflow-dlq";

    [Hangfire.Queue(QueueName)]
    public async Task RetryAsync(DeadLetterMessage message, CancellationToken cancellationToken)
    {
        using var producer = new ProducerBuilder<byte[], byte[]>(producerConfig)
            .SetKeySerializer(new DefaultSerializer<byte[]>())
            .SetValueSerializer(new DefaultSerializer<byte[]>())
            .SetErrorHandler((_, e) => logger.LogError("Kafka DLQ retry job producer error: {Reason}", e.Reason))
            .Build();

        await producer.ProduceAsync(
            message.SourceTopic,
            new Message<byte[], byte[]>
            {
                Key = message.Key,
                Value = message.Payload,
                Headers = DeadLetterRetryHeaders.CreateRetryHeaders(message.ConsumerGroup, message.Attempt)
            },
            cancellationToken);
    }
}
