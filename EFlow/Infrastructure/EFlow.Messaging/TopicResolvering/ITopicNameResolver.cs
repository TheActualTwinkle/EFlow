namespace EFlow.Messaging.TopicResolvering;

public interface ITopicNameResolver
{
    public string? ResolveTopicName(string assemblyQualifiedName);
}