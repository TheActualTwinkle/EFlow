namespace EFlow.Common.Messaging.Settings;

public record KafkaSettings
{
    public required string BootstrapServers { get; init; }
}

public record KafkaTopicsSettings
{
    public required Dictionary<string, TopicSettings> TopicsSettings { get; init; }
    
    public record TopicSettings
    {
        public required short ReplicationFactor { get; init; }

        public required int Partitions { get; init; }

        public Dictionary<string, object>? Parameters { get; init; }
    }
}