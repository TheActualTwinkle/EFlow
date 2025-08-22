using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EFlow.Common.Messaging.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EFlow.Common.Messaging.Init;

public class TopicInitializer(IOptions<KafkaTopicsSettings> kafkaSettings, IAdminClient adminClient, ILogger<TopicInitializer> logger)
{
    public async Task EnsureTopicsCreatedAsync()
    {
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
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

                Environment.Exit(-1);
            }
    }
}