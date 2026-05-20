namespace EFlow.Common.Messaging.Settings;

public record KafkaSettings
{
    public const string SectionName = "KafkaSettings";

    public required string BootstrapServers { get; init; }

    public int MessageSendMaxRetries { get; init; } = 3;

    public int MessageTimeoutMs { get; init; } = 2000;

    public int DlqMaxAttempts { get; init; } = 5;

    public string DeadLetterConsumerGroup { get; init; } = "eflow-dlq-retry";

    public required IReadOnlyList<TimeSpan> DlqRetryDelays { get; init; } =
    [
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(20)
    ];
}

public record KafkaTopicsSettings
{
    public required Dictionary<string, TopicSettings> TopicsSettings { get; init; }

    public record TopicSettings
    {
        public required short ReplicationFactor { get; init; }

        public required int Partitions { get; init; }

        public Dictionary<string, object> Parameters { get; init; } = [];
    }
}
