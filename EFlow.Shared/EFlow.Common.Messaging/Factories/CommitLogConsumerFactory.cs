using Confluent.Kafka;
using EFlow.Common.Infrastructure;
using EFlow.Common.Messaging.Consumers;
using EFlow.Common.Messaging.DeadLetter;
using EFlow.Common.Messaging.Producers;
using EFlow.Common.Messaging.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFlow.Common.Messaging.Factories;

public class CommitLogConsumerFactory(IServiceProvider serviceProvider) : ICommitLogConsumerFactory
{
    public ICommitLogConsumer<TKey, TValue> Create<TKey, TValue>(KafkaConsumerSettings consumerSettings)
    {
        var kafkaSettings = serviceProvider.GetRequiredService<IOptions<KafkaSettings>>().Value;

        var keyDeserializer = serviceProvider.GetRequiredService<IDeserializer<TKey>>();
        var valueDeserializer = serviceProvider.GetRequiredService<IDeserializer<TValue>>();

        var logger = serviceProvider.GetRequiredService<ILogger<CommitLogConsumer<TKey, TValue>>>();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            GroupId = consumerSettings.GroupId,
            AutoOffsetReset = consumerSettings.AutoOffsetReset,
            AllowAutoCreateTopics = false,
            ReconnectBackoffMs = 1000,
            EnableAutoCommit = false
        };

        var deadLetterQueueHandler = consumerSettings.UseDeadLetterQueue
            ? CreateDeadLetterQueueProducer<TKey, TValue>(consumerSettings.GroupId, kafkaSettings)
            : null;

        var consumer = new CommitLogConsumer<TKey, TValue>(
            consumerConfig,
            keyDeserializer,
            valueDeserializer,
            deadLetterQueueHandler,
            logger);

        return consumer;
    }

    private DeadLetterQueueProducer<TKey, TValue> CreateDeadLetterQueueProducer<TKey, TValue>(
        string consumerGroup,
        KafkaSettings kafkaSettings)
    {
        var keySerializer = serviceProvider.GetRequiredService<ISerializer<TKey>>();
        var valueSerializer = serviceProvider.GetRequiredService<ISerializer<TValue>>();
        var deadLetterQueueProducer = serviceProvider.GetRequiredService<ICommitLogProducer<byte[], DeadLetterMessage>>();
        var systemClock = serviceProvider.GetRequiredService<ISystemClock>();
        var logger = serviceProvider.GetRequiredService<ILogger<DeadLetterQueueProducer<TKey, TValue>>>();

        return new DeadLetterQueueProducer<TKey, TValue>(
            keySerializer,
            valueSerializer,
            deadLetterQueueProducer,
            new DeadLetterQueueProducerSettings
            {
                ConsumerGroup = consumerGroup,
                MaxAttempts = kafkaSettings.DlqMaxAttempts,
                DeadLetterTopic = KafkaTopics.DeadLetterTopic
            },
            systemClock,
            logger);
    }
}
