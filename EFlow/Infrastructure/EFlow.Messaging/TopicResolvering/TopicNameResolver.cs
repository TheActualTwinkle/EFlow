using EFlow.Common.Models.SubmissionSlot;

namespace EFlow.Messaging.TopicResolvering;

public class TopicNameResolver : ITopicNameResolver
{
    private readonly Dictionary<string, string> _typeToTopicMapping = new()
    {
        { typeof(SubmissionSlotCreatedMessage).AssemblyQualifiedName!, "eflow-submission-slot-created" }
    };

    public string? ResolveTopicName(string assemblyQualifiedName) =>
        _typeToTopicMapping.GetValueOrDefault(assemblyQualifiedName);
}