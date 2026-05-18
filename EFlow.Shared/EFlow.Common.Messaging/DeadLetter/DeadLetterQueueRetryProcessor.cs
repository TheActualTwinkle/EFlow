using Confluent.Kafka;
using EFlow.Common.Infrastructure;
using EFlow.Common.Messaging.Serialization;
using EFlow.Common.Messaging.Settings;
using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFlow.Common.Messaging.DeadLetter;

public sealed class DeadLetterQueueRetryProcessor(
    IOptions<KafkaSettings> kafkaSettings,
    IBackgroundJobClient backgroundJobClient,
    ISystemClock systemClock,
    ILogger<DeadLetterQueueRetryProcessor> logger)
    : BackgroundService
{
    private IConsumer<byte[], DeadLetterMessage> _consumer = null!;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = kafkaSettings.Value;

        _consumer = new ConsumerBuilder<byte[], DeadLetterMessage>(
                new ConsumerConfig
                {
                    BootstrapServers = settings.BootstrapServers,
                    GroupId = settings.DeadLetterConsumerGroup,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    AllowAutoCreateTopics = false,
                    ReconnectBackoffMs = 1000,
                    EnableAutoCommit = false
                })
            .SetKeyDeserializer(new DefaultSerializer<byte[]>())
            .SetValueDeserializer(new DefaultSerializer<DeadLetterMessage>())
            .SetErrorHandler((_, e) => logger.LogError("Kafka DLQ retry consumer error: {Reason}", e.Reason))
            .Build();

        logger.LogInformation("Starting DLQ retry processor for topic {DeadLetterTopic}.", KafkaTopics.DeadLetterTopic);

        _consumer.Subscribe(KafkaTopics.DeadLetterTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    var result = _consumer.Consume(stoppingToken);

                    if (result?.Message?.Value is null)
                    {
                        logger.LogError("Kafka returned an empty DLQ consume result for topic {DeadLetterTopic}.", KafkaTopics.DeadLetterTopic);

                        continue;
                    }

                    Process(result.Message.Value, stoppingToken);

                    _consumer.Commit(result);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("DLQ retry processor operation was canceled.");

                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing message from DLQ topic {DeadLetterTopic}.", KafkaTopics.DeadLetterTopic);
                }
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();

            logger.LogInformation("DLQ retry processor for topic {DeadLetterTopic} closed.", KafkaTopics.DeadLetterTopic);
        }

        return Task.CompletedTask;
    }

    private void Process(DeadLetterMessage message, CancellationToken cancellationToken)
    {
        if (!message.Retryable)
            return;

        if (message.Attempt <= 0)
        {
            logger.LogError(
                "DLQ message for topic {SourceTopic} and consumer group {ConsumerGroup} has invalid retry attempt {Attempt}. Message will be committed and skipped.",
                message.SourceTopic,
                message.ConsumerGroup,
                message.Attempt);

            return;
        }
        
        if (message.Attempt >= message.MaxAttempts)
        {
            logger.LogWarning(
                "DLQ message for topic {SourceTopic} and consumer group {ConsumerGroup} reached max retry attempt {Attempt}/{MaxAttempts}. Message will be committed and skipped.",
                message.SourceTopic,
                message.ConsumerGroup,
                message.Attempt,
                message.MaxAttempts);

            return;
        }

        if (cancellationToken.IsCancellationRequested)
            return;

        var retryDelayIndex = message.Attempt - 1;
        var retryDelays = kafkaSettings.Value.DlqRetryDelays;

        if (retryDelayIndex >= retryDelays.Count)
        {
            logger.LogError(
                "DLQ message for topic {SourceTopic} and consumer group {ConsumerGroup} has retry attempt {Attempt}, but only {RetryDelayCount} retry delays are configured. Message will be committed and skipped.",
                message.SourceTopic,
                message.ConsumerGroup,
                message.Attempt,
                retryDelays.Count);

            return;
        }

        var retryDelay = retryDelays[retryDelayIndex];
        var retryAfter = message.FailedAt + retryDelay;
        var now = systemClock.UtcNow;

        if (retryAfter < now)
            retryAfter = now;

        backgroundJobClient.Schedule<DeadLetterQueueRetryJob>(
            job => job.RetryAsync(message, CancellationToken.None),
            retryAfter);

        logger.LogInformation(
            "DLQ message for topic {SourceTopic} and consumer group {ConsumerGroup} was scheduled for retry attempt {Attempt}/{MaxAttempts} at {RetryAfter}.",
            message.SourceTopic,
            message.ConsumerGroup,
            message.Attempt,
            message.MaxAttempts,
            retryAfter);
    }
}
