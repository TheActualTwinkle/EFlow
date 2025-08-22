using Confluent.Kafka;
using EFlow.Common.Messaging.Consumers;
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
            AutoOffsetReset = consumerSettings.AutoOffsetReset
        };

        return new CommitLogConsumer<TKey, TValue>(
            consumerConfig,
            keyDeserializer,
            valueDeserializer,
            logger);
    }
}