using System.Diagnostics;
using Confluent.Kafka;
using EFlow.Common.Infrastructure;
using EFlow.Common.IntegrationTests.Infrastructure.Collections;
using EFlow.Common.IntegrationTests.Infrastructure.Fixtures;
using EFlow.Common.Messaging.DeadLetter;
using EFlow.Common.Messaging.Serialization;
using EFlow.Common.Messaging.Settings;
using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace EFlow.Common.IntegrationTests.Messaging;

[Collection(KafkaTestCollection.Name)]
public sealed class DeadLetterQueueRetryProcessorIntegrationTests(KafkaTestStackFixture fixture)
{
    private const string Key = "key";
    private const string Payload = "payload";
    private const int DefaultMaxAttempts = 3;
    private static readonly TimeSpan AsyncWaitTimeout = TimeSpan.FromSeconds(20);

    private static readonly IReadOnlyList<TimeSpan> RetryDelays =
    [
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero
    ];

    [Fact]
    public async Task Retry_WhenMessageCanBeRetried_ShouldScheduleHangfireJob()
    {
        // Arrange
        var sourceTopic = KafkaTestStackFixture.CreateTopicName("dlq-schedule-source");
        var retryConsumerGroup = $"retry-consumer-{Guid.NewGuid():N}";
        const int attempt = 1;
        var backgroundJobClient = new RecordingBackgroundJobClient();
        await fixture.CreateTopicsAsync(sourceTopic, KafkaTopics.DeadLetterTopic);
        var deadLetterProducer = fixture.CreateProducer<byte[], DeadLetterMessage>();
        var retryProcessor = fixture.CreateRetryProcessor(DefaultMaxAttempts, RetryDelays, backgroundJobClient);

        await deadLetterProducer.ProduceAsync(
            KafkaTopics.DeadLetterTopic,
            Serialize(Key),
            CreateDeadLetterMessage(sourceTopic, Key, Payload, retryConsumerGroup, attempt, DefaultMaxAttempts));

        // Act
        await retryProcessor.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => JobsForSourceTopic(backgroundJobClient, sourceTopic).Count == 1, AsyncWaitTimeout);
        await retryProcessor.StopAsync(CancellationToken.None);

        // Assert
        var scheduledJob = JobsForSourceTopic(backgroundJobClient, sourceTopic).Single();
        scheduledJob.Job.Type.Should().Be(typeof(DeadLetterQueueRetryJob));
        scheduledJob.Job.Method.Name.Should().Be(nameof(DeadLetterQueueRetryJob.RetryAsync));
        scheduledJob.State.Should().BeOfType<ScheduledState>();
        scheduledJob.Message.SourceTopic.Should().Be(sourceTopic);
        scheduledJob.Message.ConsumerGroup.Should().Be(retryConsumerGroup);
        scheduledJob.Message.Attempt.Should().Be(attempt);
        Deserialize<string>(scheduledJob.Message.Key).Should().Be(Key);
        Deserialize<string>(scheduledJob.Message.Payload).Should().Be(Payload);
    }

    [Fact]
    public async Task Retry_WhenFirstMessageHasLongDelay_ShouldStillScheduleFollowingRetryMessages()
    {
        // Arrange
        var sourceTopic = KafkaTestStackFixture.CreateTopicName("dlq-non-blocking-source");
        var retryConsumerGroup = $"retry-consumer-{Guid.NewGuid():N}";
        var now = DateTime.UtcNow;
        const string slowKey = "slow-key";
        const string fastKey = "fast-key";

        var retryDelays = new[]
        {
            TimeSpan.FromMinutes(30),
            TimeSpan.Zero
        };

        var backgroundJobClient = new RecordingBackgroundJobClient();
        await fixture.CreateTopicsAsync(sourceTopic, KafkaTopics.DeadLetterTopic);
        var deadLetterProducer = fixture.CreateProducer<byte[], DeadLetterMessage>();

        var retryProcessor = fixture.CreateRetryProcessor(
            DefaultMaxAttempts,
            retryDelays,
            backgroundJobClient,
            new FakeSystemClock(now));

        await deadLetterProducer.ProduceAsync(
            KafkaTopics.DeadLetterTopic,
            Serialize(slowKey),
            CreateDeadLetterMessage(sourceTopic, slowKey, "slow-payload", retryConsumerGroup, 1, DefaultMaxAttempts, failedAt: now));

        await deadLetterProducer.ProduceAsync(
            KafkaTopics.DeadLetterTopic,
            Serialize(fastKey),
            CreateDeadLetterMessage(sourceTopic, fastKey, "fast-payload", retryConsumerGroup, 2, DefaultMaxAttempts, failedAt: now));

        // Act
        await retryProcessor.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => JobsForSourceTopic(backgroundJobClient, sourceTopic).Count == 2, AsyncWaitTimeout);
        await retryProcessor.StopAsync(CancellationToken.None);

        // Assert
        var sourceTopicJobs = JobsForSourceTopic(backgroundJobClient, sourceTopic);
        var slowJob = sourceTopicJobs.Single(job => Deserialize<string>(job.Message.Key) == slowKey);
        var fastJob = sourceTopicJobs.Single(job => Deserialize<string>(job.Message.Key) == fastKey);
        slowJob.EnqueueAt.UtcDateTime.Should().Be(now.AddMinutes(30));
        fastJob.EnqueueAt.UtcDateTime.Should().Be(now);
    }

    [Theory]
    [InlineData(false, 1, 3)]
    [InlineData(true, 0, 3)]
    [InlineData(true, -1, 3)]
    [InlineData(true, 4, 3)]
    public async Task Retry_WhenMessageShouldNotBeRetried_ShouldNotScheduleHangfireJob(
        bool retryable,
        int attempt,
        int maxAttempts)
    {
        // Arrange
        var sourceTopic = KafkaTestStackFixture.CreateTopicName("dlq-skip-source");
        var retryConsumerGroup = $"retry-consumer-{Guid.NewGuid():N}";
        var backgroundJobClient = new RecordingBackgroundJobClient();
        await fixture.CreateTopicsAsync(sourceTopic, KafkaTopics.DeadLetterTopic);
        var deadLetterProducer = fixture.CreateProducer<byte[], DeadLetterMessage>();
        var retryProcessor = fixture.CreateRetryProcessor(maxAttempts, RetryDelays, backgroundJobClient);

        await deadLetterProducer.ProduceAsync(
            KafkaTopics.DeadLetterTopic,
            Serialize(Key),
            CreateDeadLetterMessage(
                sourceTopic,
                Key,
                Payload,
                retryConsumerGroup,
                attempt,
                maxAttempts,
                retryable));

        // Act
        await retryProcessor.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(2));
        await retryProcessor.StopAsync(CancellationToken.None);

        // Assert
        JobsForSourceTopic(backgroundJobClient, sourceTopic).Should().BeEmpty();
    }

    [Fact]
    public async Task Retry_WhenAttemptEqualsMaxAttempts_ShouldScheduleHangfireJob()
    {
        // Arrange
        var sourceTopic = KafkaTestStackFixture.CreateTopicName("dlq-last-attempt-source");
        var retryConsumerGroup = $"retry-consumer-{Guid.NewGuid():N}";
        const int attempt = 3;
        var backgroundJobClient = new RecordingBackgroundJobClient();
        await fixture.CreateTopicsAsync(sourceTopic, KafkaTopics.DeadLetterTopic);
        var deadLetterProducer = fixture.CreateProducer<byte[], DeadLetterMessage>();
        var retryProcessor = fixture.CreateRetryProcessor(DefaultMaxAttempts, RetryDelays, backgroundJobClient);

        await deadLetterProducer.ProduceAsync(
            KafkaTopics.DeadLetterTopic,
            Serialize(Key),
            CreateDeadLetterMessage(sourceTopic, Key, Payload, retryConsumerGroup, attempt, DefaultMaxAttempts));

        // Act
        await retryProcessor.StartAsync(CancellationToken.None);
        await WaitUntilAsync(() => JobsForSourceTopic(backgroundJobClient, sourceTopic).Count == 1, AsyncWaitTimeout);
        await retryProcessor.StopAsync(CancellationToken.None);

        // Assert
        JobsForSourceTopic(backgroundJobClient, sourceTopic).Single().Message.Attempt.Should().Be(attempt);
    }

    [Fact]
    public async Task RetryJob_WhenExecutedByHangfire_ShouldRepublishPayloadToSourceTopicWithRetryHeaders()
    {
        // Arrange
        var sourceTopic = KafkaTestStackFixture.CreateTopicName("dlq-job-source");
        var retryConsumerGroup = $"retry-consumer-{Guid.NewGuid():N}";
        var sourceConsumerGroup = $"source-consumer-{Guid.NewGuid():N}";
        const int attempt = 2;
        await fixture.CreateTopicsAsync(sourceTopic);
        using var sourceConsumer = fixture.CreateRawConsumer<string, string>(sourceConsumerGroup);
        sourceConsumer.Subscribe(sourceTopic);

        // Act
        await fixture.CreateRetryJob().RetryAsync(
            CreateDeadLetterMessage(sourceTopic, Key, Payload, retryConsumerGroup, attempt, DefaultMaxAttempts),
            CancellationToken.None);

        var retryResult = ConsumeRequired(sourceConsumer, AsyncWaitTimeout);

        // Assert
        retryResult.Message.Key.Should().Be(Key);
        retryResult.Message.Value.Should().Be(Payload);
        DeadLetterRetryHeaders.GetTargetConsumerGroup(retryResult.Message.Headers).Should().Be(retryConsumerGroup);
        DeadLetterRetryHeaders.GetAttempt(retryResult.Message.Headers).Should().Be(attempt);
    }

    [Fact]
    public async Task Retry_WhenProcessorSchedulesJobAndHangfireExecutesIt_ShouldRepublishPayloadToSourceTopic()
    {
        // Arrange
        var sourceTopic = KafkaTestStackFixture.CreateTopicName("dlq-e2e-source");
        var retryConsumerGroup = $"retry-consumer-{Guid.NewGuid():N}";
        var sourceConsumerGroup = $"source-consumer-{Guid.NewGuid():N}";
        const int attempt = 1;
        await fixture.CreateTopicsAsync(sourceTopic, KafkaTopics.DeadLetterTopic);
        var deadLetterProducer = fixture.CreateProducer<byte[], DeadLetterMessage>();
        using var sourceConsumer = fixture.CreateRawConsumer<string, string>(sourceConsumerGroup);
        var retryProcessor = fixture.CreateRetryProcessor(DefaultMaxAttempts, RetryDelays);
        sourceConsumer.Subscribe(sourceTopic);

        await deadLetterProducer.ProduceAsync(
            KafkaTopics.DeadLetterTopic,
            Serialize(Key),
            CreateDeadLetterMessage(sourceTopic, Key, Payload, retryConsumerGroup, attempt, DefaultMaxAttempts));

        // Act
        await retryProcessor.StartAsync(CancellationToken.None);
        var retryResult = ConsumeRequired(sourceConsumer, AsyncWaitTimeout);
        await retryProcessor.StopAsync(CancellationToken.None);

        // Assert
        retryResult.Message.Key.Should().Be(Key);
        retryResult.Message.Value.Should().Be(Payload);
        DeadLetterRetryHeaders.GetTargetConsumerGroup(retryResult.Message.Headers).Should().Be(retryConsumerGroup);
        DeadLetterRetryHeaders.GetAttempt(retryResult.Message.Headers).Should().Be(attempt);
    }

    private static DeadLetterMessage CreateDeadLetterMessage(
        string sourceTopic,
        string key,
        string payload,
        string consumerGroup,
        int attempt,
        int maxAttempts,
        bool retryable = true,
        DateTime? failedAt = null) =>
        new()
        {
            SourceTopic = sourceTopic,
            Key = Serialize(key),
            Payload = Serialize(payload),
            Exception = "Processing failed.",
            FailedAt = failedAt ?? new SystemClock().UtcNow,
            Retryable = retryable,
            ConsumerGroup = consumerGroup,
            Attempt = attempt,
            MaxAttempts = maxAttempts
        };

    private static byte[] Serialize<T>(T value) =>
        new DefaultSerializer<T>()
            .Serialize(value, new SerializationContext());

    private static T Deserialize<T>(byte[] data) =>
        new DefaultSerializer<T>()
            .Deserialize(data, false, new SerializationContext());

    private static ConsumeResult<TKey, TValue> ConsumeRequired<TKey, TValue>(
        IConsumer<TKey, TValue> consumer,
        TimeSpan timeout)
    {
        var result = consumer.Consume(timeout);

        result.Should().NotBeNull();

        return result!;
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            if (condition())
                return;

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        throw new TimeoutException("Expected condition was not met.");
    }

    private static IReadOnlyList<RecordedJob> JobsForSourceTopic(
        RecordingBackgroundJobClient backgroundJobClient,
        string sourceTopic) =>
        backgroundJobClient
            .Jobs
            .Where(job => job.Message.SourceTopic == sourceTopic)
            .ToList();

    private sealed class RecordingBackgroundJobClient : IBackgroundJobClient
    {
        private readonly List<RecordedJob> _jobs = [];

        public IReadOnlyList<RecordedJob> Jobs =>
            _jobs;

        public string Create(Job job, IState state)
        {
            state.Should().BeOfType<ScheduledState>();

            var message = job.Args.OfType<DeadLetterMessage>().Single();
            var scheduledState = (ScheduledState)state;

            _jobs.Add(new RecordedJob(job, state, message, scheduledState.EnqueueAt));

            return Guid.NewGuid().ToString("N");
        }

        public bool ChangeState(string jobId, IState state, string expectedState) =>
            true;
    }

    private sealed record RecordedJob(
        Job Job,
        IState State,
        DeadLetterMessage Message,
        DateTimeOffset EnqueueAt);
}
