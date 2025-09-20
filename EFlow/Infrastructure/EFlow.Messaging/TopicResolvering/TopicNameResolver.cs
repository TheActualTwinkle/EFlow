using EFlow.Common.Messaging.Settings;
using EFlow.Common.Models.SubmissionSlot;

namespace EFlow.Messaging.TopicResolving;

public class TopicNameResolver : ITopicNameResolver
{
    private readonly Dictionary<string, string> _typeToTopicMapping = new()
    {
        { typeof(SubmissionSlotCreatedMessage).AssemblyQualifiedName!, KafkaTopics.SubmissionSlotCreatedTopic }
    };

    public string? ResolveTopicName(string assemblyQualifiedName) =>
        _typeToTopicMapping.GetValueOrDefault(assemblyQualifiedName);
}