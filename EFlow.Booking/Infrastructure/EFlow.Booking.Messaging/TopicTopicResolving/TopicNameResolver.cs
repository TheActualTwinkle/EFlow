namespace EFlow.Booking.Messaging.TopicResolving;

public class TopicNameResolver : ITopicNameResolver
{
    private readonly Dictionary<string, string> _typeToTopicMapping = [];

    public string? ResolveTopicName(string assemblyQualifiedName) =>
        _typeToTopicMapping.GetValueOrDefault(assemblyQualifiedName);

    public void AddMapping(string assemblyQualifiedName, string topicName) =>
        _typeToTopicMapping.Add(assemblyQualifiedName, topicName);
}