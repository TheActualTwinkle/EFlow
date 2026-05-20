using System.Diagnostics;
using EFlow.Common.IntegrationTests.Infrastructure.Collections;
using EFlow.Common.IntegrationTests.Infrastructure.Fixtures;
using EFlow.Common.Messaging.DeadLetter;
using EFlow.Common.Messaging.Serialization;
using EFlow.Common.Messaging.Settings;
using FluentAssertions;

namespace EFlow.Common.IntegrationTests.Messaging;

[Collection(KafkaTestCollection.Name)]
public sealed class CommitLogConsumerIntegrationTests(KafkaTestStackFixture fixture)
{
    private static readonly TimeSpan AsyncWaitTimeout = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan ConsumerLoopTick = TimeSpan.FromSeconds(2);

    [Fact]
    public async Task Consume_WhenHandlerReturnsFalse_ShouldProduceMessageToDeadLetterTopic()
    {
        // Arrange
        var sourceTopic = KafkaTestStackFixture.CreateTopicName("commit-log-consumer-source");
        var sourceConsumerGroup = $"source-consumer-{Guid.NewGuid():N}";
        var deadLetterConsumerGroup = $"dlq-consumer-{Guid.NewGuid():N}";
        var key = $"key-{Guid.NewGuid():N}";
        const string payload = "payload";
        const string expectedException = "Message handler returned unsuccessful result, see inner logs for details.";
        const int maxRetries = 5;
        const int expectedAttempt = 1;
        await fixture.CreateTopicsAsync(sourceTopic, KafkaTopics.DeadLetterTopic);
        var sourceProducer = fixture.CreateProducer<string, string>();
        using var deadLetterConsumer = fixture.CreateRawConsumer<byte[], DeadLetterMessage>(deadLetterConsumerGroup);
        var sourceConsumer = fixture.CreateConsumer<string, string>(sourceConsumerGroup, maxRetries);
        deadLetterConsumer.Subscribe(KafkaTopics.DeadLetterTopic);

        // Act
        await sourceConsumer.StartAsync(sourceTopic, (_, _) => ValueTask.FromResult(false));
        await sourceProducer.ProduceAsync(sourceTopic, key, payload);
        var deadLetterResult = ConsumeRequired(
            deadLetterConsumer,
            message => message.SourceTopic == sourceTopic,
            AsyncWaitTimeout);
        await sourceConsumer.StopAsync();

        // Assert
        Deserialize<string>(deadLetterResult.Message.Key).Should().Be(key);
        Deserialize<string>(deadLetterResult.Message.Value.Key).Should().Be(key);
        Deserialize<string>(deadLetterResult.Message.Value.Payload).Should().Be(payload);
        deadLetterResult.Message.Value.Should().BeEquivalentTo(new
        {
            SourceTopic = sourceTopic,
            Exception = expectedException,
            Retryable = true,
            ConsumerGroup = sourceConsumerGroup,
            Attempt = expectedAttempt,
            MaxAttempts = maxRetries
        });
    }

    [Fact]
    public async Task Consume_WhenRetryTargetsAnotherConsumerGroup_ShouldSkipMessage()
    {
        // Arrange
        var sourceTopic = KafkaTestStackFixture.CreateTopicName("commit-log-consumer-retry-source");
        var sourceConsumerGroup = $"source-consumer-{Guid.NewGuid():N}";
        var targetConsumerGroup = $"target-consumer-{Guid.NewGuid():N}";
        var key = $"key-{Guid.NewGuid():N}";
        const string payload = "payload";
        const int maxRetries = 5;
        const int attempt = 1;
        await fixture.CreateTopicsAsync(sourceTopic, KafkaTopics.DeadLetterTopic);
        var sourceProducer = fixture.CreateProducer<string, string>();
        var sourceConsumer = fixture.CreateConsumer<string, string>(
            sourceConsumerGroup,
            maxRetries);
        var handledMessages = new List<string>();

        // Act
        await sourceConsumer.StartAsync(
            sourceTopic,
            (message, _) =>
            {
                handledMessages.Add(message);

                return ValueTask.FromResult(true);
            });
        await sourceProducer.ProduceAsync(
            sourceTopic,
            key,
            payload,
            DeadLetterRetryHeaders.CreateRetryHeaders(targetConsumerGroup, attempt));
        await Task.Delay(ConsumerLoopTick);
        await sourceConsumer.StopAsync();

        // Assert
        handledMessages.Should().BeEmpty();
    }

    private static T Deserialize<T>(byte[] data) =>
        new DefaultSerializer<T>()
            .Deserialize(data, false, new Confluent.Kafka.SerializationContext());

    private static Confluent.Kafka.ConsumeResult<TKey, DeadLetterMessage> ConsumeRequired<TKey>(
        Confluent.Kafka.IConsumer<TKey, DeadLetterMessage> consumer,
        Func<DeadLetterMessage, bool> predicate,
        TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            var result = consumer.Consume(TimeSpan.FromMilliseconds(500));

            if (result?.Message?.Value is not null && predicate(result.Message.Value))
                return result;
        }

        throw new TimeoutException("Expected Kafka message was not consumed.");
    }

    private static Confluent.Kafka.ConsumeResult<TKey, TValue> ConsumeRequired<TKey, TValue>(
        Confluent.Kafka.IConsumer<TKey, TValue> consumer,
        TimeSpan timeout)
    {
        var result = consumer.Consume(timeout);

        result.Should().NotBeNull();

        return result!;
    }
}
