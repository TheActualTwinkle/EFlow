using EFlow.Common.Messaging.Consumers;
using EFlow.Common.Messaging.Settings;

namespace EFlow.Common.Messaging.Factories;

public interface ICommitLogConsumerFactory
{
    public ICommitLogConsumer<TKey, TValue> Create<TKey, TValue>(KafkaConsumerSettings consumerSettings);
}