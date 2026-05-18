using Confluent.Kafka;
using EFlow.Common.Infrastructure;
using EFlow.Common.Messaging.Producers;
using Microsoft.Extensions.Logging;

namespace EFlow.Common.Messaging.DeadLetter;

public sealed class DeadLetterQueueProducer<TKey, TValue>(
    ISerializer<TKey> keySerializer,
    ISerializer<TValue> valueSerializer,
    ICommitLogProducer<byte[], DeadLetterMessage> deadLetterQueueProducer,
    DeadLetterQueueProducerSettings settings,
    ISystemClock systemClock,
    ILogger<DeadLetterQueueProducer<TKey, TValue>> logger)
{
    public bool ShouldSkipRetryMessage(Headers? headers)
    {
        var targetConsumerGroup = DeadLetterRetryHeaders.GetTargetConsumerGroup(headers);

        return targetConsumerGroup is not null && targetConsumerGroup != settings.ConsumerGroup;
    }

    public async Task<bool> TryProduceAsync(
        string sourceTopic,
        ConsumeResult<TKey, TValue> result,
        string exception,
        bool retryable,
        CancellationToken cancellationToken)
    {
        var headers = result.Message.Headers;

        var deadLetterMessage = new DeadLetterMessage
        {
            SourceTopic = sourceTopic,
            Key = keySerializer.Serialize(
                result.Message.Key,
                new SerializationContext(MessageComponentType.Key, sourceTopic, headers)),
            Payload = valueSerializer.Serialize(
                result.Message.Value,
                new SerializationContext(MessageComponentType.Value, sourceTopic, headers)),
            Exception = exception,
            FailedAt = systemClock.UtcNow,
            Retryable = retryable,
            ConsumerGroup = settings.ConsumerGroup,
            Attempt = DeadLetterRetryHeaders.GetAttempt(headers) + 1,
            MaxAttempts = settings.MaxAttempts
        };

        try
        {
            await deadLetterQueueProducer.ProduceAsync(
                settings.DeadLetterTopic,
                deadLetterMessage.Key,
                deadLetterMessage,
                cancellationToken);

            logger.LogDebug(
                "Message \"{Type}\" was produced to DLQ topic {DeadLetterTopic}.",
                result.Message.Value?.GetType().Name,
                settings.DeadLetterTopic);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to produce message from topic {SourceTopic} to DLQ topic {DeadLetterTopic}. Source offset will not be committed.",
                sourceTopic,
                settings.DeadLetterTopic);

            return false;
        }
    }
}
