namespace EFlow.Messaging.TopicResolving;

public interface ITopicNameResolver
{
    public string? ResolveTopicName(string assemblyQualifiedName);
}