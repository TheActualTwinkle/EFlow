using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace EFlow.Common.Messaging.Producers;

public class CommitLogProducer<TKey, TValue> : ICommitLogProducer<TKey, TValue>
{
    private readonly IProducer<TKey, TValue> _producer;

    public CommitLogProducer(
        ProducerConfig producerConfig,
        ISerializer<TKey> keySerializer,
        ISerializer<TValue> valueSerializer,
        ILogger<CommitLogProducer<TKey, TValue>> logger) =>
        _producer = new ProducerBuilder<TKey, TValue>(producerConfig)
            .SetKeySerializer(keySerializer)
            .SetValueSerializer(valueSerializer)
            .SetErrorHandler((_, e) => logger.LogError("Kafka Error: {reason}", e.Reason))
            .Build();

    public async Task ProduceAsync(
        string topic,
        TKey key,
        TValue value,
        CancellationToken cancellationToken = new())
    {
        var message = new Message<TKey, TValue>
        {
            Key = key,
            Value = value
        };

        await _producer.ProduceAsync(topic, message, cancellationToken);
    }
}