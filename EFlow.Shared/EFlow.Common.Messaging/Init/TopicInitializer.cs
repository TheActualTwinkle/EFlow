using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EFlow.Common.Messaging.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFlow.Common.Messaging.Init;

public class TopicInitializer(
    IOptions<KafkaTopicsSettings> kafkaSettings,
    IAdminClient adminClient,
    ILogger<TopicInitializer> logger)
{
    private static readonly TimeSpan AdminClientRequestTimeout = TimeSpan.FromSeconds(10);

    public async Task CreateMissingTopicsAsync()
    {
        var metadata = adminClient.GetMetadata(AdminClientRequestTimeout);
        var existingTopics = metadata.Topics.Select(t => t.Topic).ToHashSet();

        var topicsToCreate = kafkaSettings.Value.TopicsSettings
            .Where(kvp => !existingTopics.Contains(kvp.Key))
            .Select(kvp =>
                new TopicSpecification
                {
                    Name = kvp.Key,
                    NumPartitions = kvp.Value.Partitions,
                    ReplicationFactor = kvp.Value.ReplicationFactor,
                    Configs = kvp.Value.Parameters?.ToDictionary(p => p.Key, p => p.Value.ToString()) ?? []
                })
            .ToList();

        if (topicsToCreate.Count > 0)
            try
            {
                await adminClient.CreateTopicsAsync(topicsToCreate);
            }
            catch (CreateTopicsException e)
            {
                logger.LogCritical("Failed to create topics: {Message}.", e.Message);

                throw;
            }
    }

    public async Task WaitForTopicsCreatedAsync(CancellationToken cancellationToken = new())
    {
        var requiredTopics = kafkaSettings.Value.TopicsSettings.Keys.ToHashSet();

        logger.LogInformation("Waiting for topics to be ready: {Topics}", string.Join(", ", requiredTopics));

        while (!cancellationToken.IsCancellationRequested)
        {
            var metadata = adminClient.GetMetadata(AdminClientRequestTimeout);
            var existingTopics = metadata.Topics.Select(t => t.Topic).ToHashSet();

            logger.LogDebug("Existing topics: {Topics}", string.Join(", ", existingTopics));

            if (requiredTopics.IsSubsetOf(existingTopics))
            {
                logger.LogInformation("All required topics are ready.");

                return;
            }

            logger.LogWarning("Not all required topics are ready. Waiting for {Timeout}...", AdminClientRequestTimeout);

            await Task.Delay(AdminClientRequestTimeout, cancellationToken);
        }

        throw new OperationCanceledException("Waiting for topics was canceled.", cancellationToken);
    }
}