using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EFlow.Common.Infrastructure;
using EFlow.Common.Messaging.Consumers;
using EFlow.Common.Messaging.DeadLetter;
using EFlow.Common.Messaging.Producers;
using EFlow.Common.Messaging.Serialization;
using EFlow.Common.Messaging.Settings;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Testcontainers.Kafka;

namespace EFlow.Common.IntegrationTests.Infrastructure.Fixtures;

public sealed class KafkaTestStackFixture : IAsyncLifetime
{
    private readonly KafkaContainer _kafkaContainer = new KafkaBuilder("apache/kafka:4.1.0")
        .Build();

    public string BootstrapServers => _kafkaContainer.GetBootstrapAddress();

    public async Task InitializeAsync() =>
        await _kafkaContainer.StartAsync();

    public async Task DisposeAsync() =>
        await _kafkaContainer.DisposeAsync();

    public async Task CreateTopicsAsync(params string[] topics)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = BootstrapServers
        }).Build();

        var existingTopics = adminClient
            .GetMetadata(TimeSpan.FromSeconds(10))
            .Topics
            .Select(topic => topic.Topic)
            .ToHashSet();

        var topicsToCreate = topics
            .Where(topic => !existingTopics.Contains(topic))
            .Select(topic => new TopicSpecification
            {
                Name = topic,
                NumPartitions = 1,
                ReplicationFactor = 1
            })
            .ToList();

        if (topicsToCreate.Count != 0)
            await adminClient.CreateTopicsAsync(topicsToCreate);
    }

    public ICommitLogProducer<TKey, TValue> CreateProducer<TKey, TValue>() =>
        new CommitLogProducer<TKey, TValue>(
            new ProducerConfig
            {
                BootstrapServers = BootstrapServers,
                AllowAutoCreateTopics = false,
                MessageTimeoutMs = 5000
            },
            new DefaultSerializer<TKey>(),
            new DefaultSerializer<TValue>(),
            NullLogger<CommitLogProducer<TKey, TValue>>.Instance);

    public ICommitLogConsumer<TKey, TValue> CreateConsumer<TKey, TValue>(
        string consumerGroup,
        int maxAttempts,
        bool useDeadLetterQueue = true) =>
        CreateBaseConsumer<TKey, TValue>(
            consumerGroup,
            maxAttempts,
            useDeadLetterQueue);

    private CommitLogConsumer<TKey, TValue> CreateBaseConsumer<TKey, TValue>(
        string consumerGroup,
        int maxAttempts,
        bool useDeadLetterQueue)
    {
        var keySerializer = new DefaultSerializer<TKey>();
        var valueSerializer = new DefaultSerializer<TValue>();
        var deadLetterQueueHandler = useDeadLetterQueue
            ? new DeadLetterQueueProducer<TKey, TValue>(
                keySerializer,
                valueSerializer,
                CreateProducer<byte[], DeadLetterMessage>(),
                new DeadLetterQueueProducerSettings
                {
                    ConsumerGroup = consumerGroup,
                    MaxAttempts = maxAttempts,
                    DeadLetterTopic = KafkaTopics.DeadLetterTopic
                },
                new SystemClock(),
                NullLogger<DeadLetterQueueProducer<TKey, TValue>>.Instance)
            : null;

        return new CommitLogConsumer<TKey, TValue>(
            new ConsumerConfig
            {
                BootstrapServers = BootstrapServers,
                GroupId = consumerGroup,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = false,
                EnableAutoCommit = false
            },
            keySerializer,
            valueSerializer,
            deadLetterQueueHandler,
            NullLogger<CommitLogConsumer<TKey, TValue>>.Instance);
    }

    public DeadLetterQueueRetryProcessor CreateRetryProcessor(
        int maxAttempts,
        IReadOnlyList<TimeSpan> retryDelays,
        IBackgroundJobClient? backgroundJobClient = null,
        ISystemClock? systemClock = null) =>
        new(
            Options.Create(new KafkaSettings
            {
                BootstrapServers = BootstrapServers,
                DeadLetterConsumerGroup = $"dlq-retry-{Guid.NewGuid():N}",
                DlqMaxAttempts = maxAttempts,
                DlqRetryDelays = retryDelays
            }),
            backgroundJobClient ?? new ImmediateBackgroundJobClient(CreateRetryJob()),
            systemClock ?? new SystemClock(),
            NullLogger<DeadLetterQueueRetryProcessor>.Instance);

    public DeadLetterQueueRetryJob CreateRetryJob() =>
        new(
            new ProducerConfig
            {
                BootstrapServers = BootstrapServers,
                AllowAutoCreateTopics = false,
                MessageTimeoutMs = 5000
            },
            NullLogger<DeadLetterQueueRetryJob>.Instance);

    public IConsumer<TKey, TValue> CreateRawConsumer<TKey, TValue>(string consumerGroup) =>
        new ConsumerBuilder<TKey, TValue>(
                new ConsumerConfig
                {
                    BootstrapServers = BootstrapServers,
                    GroupId = consumerGroup,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    AllowAutoCreateTopics = false,
                    EnableAutoCommit = false
                })
            .SetKeyDeserializer(new DefaultSerializer<TKey>())
            .SetValueDeserializer(new DefaultSerializer<TValue>())
            .Build();

    public static string CreateTopicName(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}";

    private sealed class ImmediateBackgroundJobClient(DeadLetterQueueRetryJob retryJob) : IBackgroundJobClient
    {
        public string Create(Job job, IState state)
        {
            var target = job.Type == typeof(DeadLetterQueueRetryJob) ? retryJob : null;

            if (target is null)
                throw new InvalidOperationException($"Unsupported Hangfire job type: {job.Type}.");

            var result = job.Method.Invoke(target, job.Args.ToArray());

            if (result is Task task)
                task.GetAwaiter().GetResult();

            return Guid.NewGuid().ToString("N");
        }

        public bool ChangeState(string jobId, IState state, string expectedState) =>
            true;
    }
}
