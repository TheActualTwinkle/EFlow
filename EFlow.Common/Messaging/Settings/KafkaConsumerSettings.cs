using Confluent.Kafka;

namespace EFlow.Common.Messaging.Settings;

public record KafkaConsumerSettings
{
    public required string GroupId { get; init; }

    public required AutoOffsetReset AutoOffsetReset { get; init; }
}